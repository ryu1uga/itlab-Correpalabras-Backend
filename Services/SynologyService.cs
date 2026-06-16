using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CorrePalabras.Services
{
    public class SynologyService : ISynologyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;

        private string? _sid;
        private string? _synoToken;
        private string  _cookieHeader = "";
        private DateTime _sidExpiry = DateTime.MinValue;
        private readonly SemaphoreSlim _loginLock = new(1, 1);

        public SynologyService(HttpClient httpClient, CookieContainer _)
        {
            _httpClient = httpClient;
            _baseUrl  = (Environment.GetEnvironmentVariable("SYNOLOGY_BASE_URL") ?? "http://localhost:5000").TrimEnd('/');
            _username = Environment.GetEnvironmentVariable("SYNOLOGY_USERNAME") ?? "";
            _password = Environment.GetEnvironmentVariable("SYNOLOGY_PASSWORD") ?? "";
        }

        // ── Auth ─────────────────────────────────────────────────────────────

        private static Dictionary<string, string> ParseSetCookies(HttpResponseMessage res)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!res.Headers.TryGetValues("Set-Cookie", out var values)) return dict;
            foreach (var sc in values)
            {
                var nameVal = sc.Split(';')[0].Trim();
                var eq = nameVal.IndexOf('=');
                if (eq > 0) dict[nameVal[..eq].Trim()] = nameVal[(eq + 1)..].Trim();
            }
            return dict;
        }

        private async Task EnsureSessionAsync()
        {
            if (!string.IsNullOrEmpty(_sid) && DateTime.UtcNow < _sidExpiry) return;
            await _loginLock.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_sid) && DateTime.UtcNow < _sidExpiry) return;

                // Pre-flight GET obtiene cookies de sesión antes del login
                var preflight = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    using var pfReq = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/");
                    pfReq.Headers.TryAddWithoutValidation("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    preflight = ParseSetCookies(await _httpClient.SendAsync(pfReq));
                }
                catch { /* preflight opcional */ }

                var preflightCookies = preflight.Count > 0
                    ? string.Join("; ", preflight.Select(kv => $"{kv.Key}={kv.Value}"))
                    : "";

                using var req = new HttpRequestMessage(HttpMethod.Post,
                    $"{_baseUrl}/webapi/auth.cgi?api=SYNO.API.Auth&version=3&method=login" +
                    "&session=SynologyDrive&format=cookie&enable_syno_token=yes")
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["account"]    = _username,
                        ["passwd"]     = _password,
                        ["stay_login"] = "1"
                    })
                };
                if (!string.IsNullOrEmpty(preflightCookies))
                    req.Headers.TryAddWithoutValidation("Cookie", preflightCookies);

                var res = await _httpClient.SendAsync(req);
                var raw = await res.Content.ReadAsStringAsync();

                var setCookies = new Dictionary<string, string>(preflight, StringComparer.OrdinalIgnoreCase);
                foreach (var kv in ParseSetCookies(res)) setCookies[kv.Key] = kv.Value;

                var data = Parse<SynologyResponse<LoginData>>(raw);

                // Con format=cookie el SID viene en la cookie 'id', no en el JSON
                if (setCookies.TryGetValue("id", out var sidCookie) && !string.IsNullOrEmpty(sidCookie))
                {
                    data ??= new SynologyResponse<LoginData> { Success = true };
                    data.Data ??= new LoginData();
                    data.Data.Sid = sidCookie;
                }

                if (data?.Success != true || string.IsNullOrEmpty(data.Data?.Sid))
                    throw new Exception($"Login falló: {raw}");

                _sid       = data.Data!.Sid;
                _synoToken = data.Data.SynoToken ?? setCookies.GetValueOrDefault("io");

                var cookieParts = setCookies.Select(kv => $"{kv.Key}={kv.Value}").ToList();
                if (!setCookies.ContainsKey("stay_login")) cookieParts.Add("stay_login=1");
                _cookieHeader = string.Join("; ", cookieParts);

                _sidExpiry = DateTime.UtcNow.AddHours(6);
            }
            finally { _loginLock.Release(); }
        }

        private bool IsAuthError(SynologyBaseResponse r) =>
            !r.Success && r.Error != null && (r.Error.Code == 106 || r.Error.Code == 119);

        // ── HTTP ─────────────────────────────────────────────────────────────

        private async Task<string> CallAsync(HttpMethod method, string apiParams, HttpContent? body = null)
        {
            await EnsureSessionAsync();
            var tokenSuffix = !string.IsNullOrEmpty(_synoToken) ? $"&SynoToken={Uri.EscapeDataString(_synoToken)}" : "";
            var prefix = string.IsNullOrEmpty(apiParams) ? "" : $"{apiParams}&";
            var url = $"{_baseUrl}/webapi/entry.cgi?{prefix}_sid={Uri.EscapeDataString(_sid!)}{tokenSuffix}";

            using var req = new HttpRequestMessage(method, url);
            if (body != null) { req.Content = body; req.Headers.ExpectContinue = false; }
            if (!string.IsNullOrEmpty(_cookieHeader)) req.Headers.TryAddWithoutValidation("Cookie", _cookieHeader);
            if (!string.IsNullOrEmpty(_synoToken))    req.Headers.TryAddWithoutValidation("X-Syno-Token", _synoToken);

            return await (await _httpClient.SendAsync(req)).Content.ReadAsStringAsync();
        }

        private static T? Parse<T>(string raw) where T : class =>
            JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // ── Carpetas ─────────────────────────────────────────────────────────

        private async Task<string?> GetFolderIdAsync(string path)
        {
            var raw = await CallAsync(HttpMethod.Get,
                $"api=SYNO.SynologyDrive.Files&version=2&method=get&path={Uri.EscapeDataString(path)}");
            var doc = JsonDocument.Parse(raw).RootElement;
            return doc.TryGetProperty("success", out var s) && s.GetBoolean()
                && doc.TryGetProperty("data", out var d) && d.TryGetProperty("file_id", out var fid)
                ? fid.GetString() : null;
        }

        private async Task<string> GetOrCreateFolderAsync(string path)
        {
            var existing = await GetFolderIdAsync(path);
            if (existing != null) return existing;

            var raw = await CallAsync(HttpMethod.Put,
                $"api=SYNO.SynologyDrive.Files&version=2&method=create" +
                $"&path={Uri.EscapeDataString(path)}&type=folder&conflict_action=skip");
            var doc = JsonDocument.Parse(raw).RootElement;

            if (doc.TryGetProperty("success", out var s) && s.GetBoolean()
                && doc.TryGetProperty("data", out var d) && d.TryGetProperty("file_id", out var fid))
                return fid.GetString()!;

            var code = doc.TryGetProperty("error", out var err) && err.TryGetProperty("code", out var c)
                ? c.GetInt32() : 0;
            if (code is 400 or 105 or 405 or 1022 or 1100)
            {
                var retry = await GetFolderIdAsync(path);
                if (retry != null) return retry;
            }

            throw new Exception($"Error creando carpeta '{path}': {raw}");
        }

        private async Task EnsureFolderPathAsync(string fullPath)
        {
            var current = "";
            foreach (var seg in fullPath.Trim('/').Split('/'))
            {
                current = $"{current}/{seg}";
                if (!string.Equals(seg, "team-folders", StringComparison.OrdinalIgnoreCase))
                    await GetOrCreateFolderAsync(current);
            }
        }

        // ── Upload ────────────────────────────────────────────────────────────

        private async Task<(string FullPath, string? FileId)> UploadAsync(string folderPath, IFormFile file, string fileName)
        {
            var fullPath = $"{folderPath.TrimEnd('/')}/{fileName}";

            using var ms = new MemoryStream();
            await file.OpenReadStream().CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            async Task<(string Raw, SynologyBaseResponse Parsed)> TryAsync()
            {
                await EnsureSessionAsync();
                var modifiedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
                var uploadParams =
                    $"api=SYNO.SynologyDrive.Files&method=upload&version=2" +
                    $"&modified_time={modifiedTime:F3}" +
                    $"&path={Uri.EscapeDataString(fullPath)}" +
                    $"&type=file&conflict_action=overwrite";

                var contentType = file.ContentType ?? "application/octet-stream";
                var boundary    = "----WebKitFormBoundaryCorrePalabrasUpload";
                var header = Encoding.UTF8.GetBytes(
                    $"--{boundary}\r\n" +
                    $"Content-Disposition: form-data; name=\"file\"; filename=\"{fileName}\"\r\n" +
                    $"Content-Type: {contentType}\r\n\r\n");
                var footer = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

                var body = new byte[header.Length + fileBytes.Length + footer.Length];
                Buffer.BlockCopy(header,    0, body, 0,                                header.Length);
                Buffer.BlockCopy(fileBytes, 0, body, header.Length,                    fileBytes.Length);
                Buffer.BlockCopy(footer,    0, body, header.Length + fileBytes.Length, footer.Length);

                var uploadBody = new ByteArrayContent(body);
                uploadBody.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");

                var raw = await CallAsync(HttpMethod.Post, uploadParams, uploadBody);
                return (raw, Parse<SynologyBaseResponse>(raw)!);
            }

            await EnsureFolderPathAsync(folderPath);
            var (raw, parsed) = await TryAsync();
            if (IsAuthError(parsed)) { _sid = null; (raw, parsed) = await TryAsync(); }

            if (!parsed.Success)
                throw new Exception($"Upload falló (código {parsed.Error?.Code}): {raw}");

            string? fileId = null;
            try
            {
                var root = JsonDocument.Parse(raw).RootElement;
                if (root.TryGetProperty("data", out var d)
                    && d.TryGetProperty("file_id", out var fid)
                    && fid.ValueKind == JsonValueKind.String)
                    fileId = fid.GetString();
            }
            catch { /* ignorar */ }

            return (fullPath, fileId);
        }

        // ── Sharing ───────────────────────────────────────────────────────────

        private async Task<string> CreateShareAsync(string drivePath, string? fileId = null)
        {
            var idRef    = !string.IsNullOrEmpty(fileId) ? $"id:{fileId}" : drivePath;
            var formBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("path",    $"\"{idRef}\""),
                new KeyValuePair<string,string>("api",     "SYNO.SynologyDrive.AdvanceSharing"),
                new KeyValuePair<string,string>("method",  "create"),
                new KeyValuePair<string,string>("version", "1"),
            });

            var raw = await CallAsync(HttpMethod.Post, "", formBody);
            var res = Parse<SynologyBaseResponse>(raw);
            if (IsAuthError(res!)) { _sid = null; raw = await CallAsync(HttpMethod.Post, "", formBody); res = Parse<SynologyBaseResponse>(raw); }
            if (res?.Success != true) throw new Exception($"Share falló: {raw}");

            string? shareUrl    = null;
            string? shareFileId = fileId;
            var root = JsonDocument.Parse(raw).RootElement;
            if (root.TryGetProperty("data", out var data))
            {
                if (data.TryGetProperty("url",  out var u) && u.ValueKind == JsonValueKind.String) shareUrl = u.GetString();
                if (shareUrl == null && data.TryGetProperty("link", out var l) && l.ValueKind == JsonValueKind.String) shareUrl = l.GetString();
                if (shareUrl == null && data.TryGetProperty("links", out var ls)
                    && ls.ValueKind == JsonValueKind.Array && ls.GetArrayLength() > 0)
                {
                    var first = ls[0];
                    if (first.TryGetProperty("url",  out var lu)) shareUrl = lu.GetString();
                    if (shareUrl == null && first.TryGetProperty("link", out var ll)) shareUrl = ll.GetString();
                    if (first.TryGetProperty("file_id", out var fid) && fid.ValueKind == JsonValueKind.String) shareFileId = fid.GetString();
                }
                if (shareFileId == null && data.TryGetProperty("file_id", out var fid2) && fid2.ValueKind == JsonValueKind.String)
                    shareFileId = fid2.GetString();
            }

            if (string.IsNullOrEmpty(shareUrl)) throw new Exception($"Share falló (sin URL): {raw}");

            if (!string.IsNullOrEmpty(shareFileId))
                await TrySetSharePublicAsync(shareFileId!);

            return shareUrl.Contains('?')
                ? $"{shareUrl}&path={Uri.EscapeDataString(drivePath)}"
                : $"{shareUrl}?path={Uri.EscapeDataString(drivePath)}";
        }

        private async Task TrySetSharePublicAsync(string fileId)
        {
            foreach (var memberType in new[] { "anyone", "everyone", "internal" })
            {
                var permissions = JsonSerializer.Serialize(new[]
                {
                    new { action = "update", member = new { type = memberType }, role = "viewer" }
                });
                var apiParams = $"api=SYNO.SynologyDrive.Sharing&method=update&version=1" +
                                $"&path={Uri.EscapeDataString($"\"id:{fileId}\"")}" +
                                $"&permissions={Uri.EscapeDataString(permissions)}";
                try
                {
                    var raw = await CallAsync(HttpMethod.Post, apiParams);
                    var res = Parse<SynologyBaseResponse>(raw);
                    if (IsAuthError(res!)) { _sid = null; raw = await CallAsync(HttpMethod.Post, apiParams); res = Parse<SynologyBaseResponse>(raw); }
                    if (res?.Success == true) return;
                }
                catch { /* best-effort */ }
            }
        }

        // ── Delete ────────────────────────────────────────────────────────────

        private static string? ExtractPathFromUrl(string url)
        {
            try
            {
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(url).Query);
                return query.TryGetValue("path", out var v) ? v.ToString() : null;
            }
            catch { return null; }
        }

        // ── Interfaz pública ──────────────────────────────────────────────────

        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            if (!destinationFolder.StartsWith("/team-folders", StringComparison.OrdinalIgnoreCase))
                destinationFolder = $"/team-folders{(destinationFolder.StartsWith('/') ? "" : "/")}{destinationFolder}";

            var (drivePath, fileId) = await UploadAsync(destinationFolder, file, fileName);
            return await CreateShareAsync(drivePath, fileId);
        }

        public async Task DeleteBySharingUrlAsync(string sharingUrl)
        {
            if (string.IsNullOrEmpty(sharingUrl)) return;
            var path = ExtractPathFromUrl(sharingUrl);
            if (!string.IsNullOrEmpty(path)) await DeleteByPathAsync(path);
        }

        public async Task DeleteByPathAsync(string filePath)
        {
            await EnsureSessionAsync();

            if (!filePath.StartsWith("/team-folders", StringComparison.OrdinalIgnoreCase))
                filePath = $"/team-folders{(filePath.StartsWith('/') ? "" : "/")}{filePath}";

            // Paso 1: obtener file_id
            var getRaw = await CallAsync(HttpMethod.Get,
                $"api=SYNO.SynologyDrive.Files&version=2&method=get&path={Uri.EscapeDataString(filePath)}");

            using var getDoc = JsonDocument.Parse(getRaw);
            var getRoot = getDoc.RootElement;
            if (!getRoot.TryGetProperty("success", out var ok) || !ok.GetBoolean())
                throw new Exception($"Delete: no se pudo obtener file_id para '{filePath}': {getRaw[..Math.Min(200, getRaw.Length)]}");

            var fileId = getRoot.GetProperty("data").GetProperty("file_id").GetString()!;

            // Paso 2: delete con files=["id:xxx"]
            var deleteBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("api",       "SYNO.SynologyDrive.Files"),
                new KeyValuePair<string,string>("version",   "2"),
                new KeyValuePair<string,string>("method",    "delete"),
                new KeyValuePair<string,string>("files",     $"[\"id:{fileId}\"]"),
                new KeyValuePair<string,string>("revisions", "1"),
                new KeyValuePair<string,string>("permanent", "false"),
            });
            var raw = await CallAsync(HttpMethod.Post, "", deleteBody);
            var res = Parse<SynologyBaseResponse>(raw);

            if (IsAuthError(res!)) { _sid = null; await DeleteByPathAsync(filePath); return; }
            if (res?.Success != true) throw new Exception($"Delete falló (código {res?.Error?.Code}): {raw}");
        }

        public async Task<byte[]> DownloadFileAsync(string filePath)
        {
            if (!filePath.StartsWith("/team-folders", StringComparison.OrdinalIgnoreCase))
                filePath = $"/team-folders{(filePath.StartsWith('/') ? "" : "/")}{filePath}";
            if (filePath.EndsWith("/download", StringComparison.OrdinalIgnoreCase))
                filePath = filePath[..^"/download".Length];

            await EnsureSessionAsync();

            var pathJson    = $"[\"{filePath.Replace("\"", "\\\"")}\"]";
            var tokenSuffix = !string.IsNullOrEmpty(_synoToken) ? $"&SynoToken={Uri.EscapeDataString(_synoToken)}" : "";

            foreach (var url in new[]
            {
                $"{_baseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Download&version=2&method=download&path={Uri.EscapeDataString(pathJson)}&mode=download&_sid={Uri.EscapeDataString(_sid!)}{tokenSuffix}",
                $"{_baseUrl}/webapi/entry.cgi?api=SYNO.SynologyDrive.Files&version=2&method=download&path={Uri.EscapeDataString(pathJson)}&_sid={Uri.EscapeDataString(_sid!)}{tokenSuffix}",
            })
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(_cookieHeader)) req.Headers.TryAddWithoutValidation("Cookie", _cookieHeader);
                if (!string.IsNullOrEmpty(_synoToken))    req.Headers.TryAddWithoutValidation("X-Syno-Token", _synoToken);

                var res  = await _httpClient.SendAsync(req);
                var body = await res.Content.ReadAsByteArrayAsync();
                var ct   = res.Content.Headers.ContentType?.MediaType ?? "";

                if (!res.IsSuccessStatusCode) continue;
                if (ct.StartsWith("image") || ct == "application/octet-stream") return body;
                var preview = Encoding.UTF8.GetString(body, 0, Math.Min(300, body.Length));
                if (body.Length > 1024 && !preview.TrimStart().StartsWith("{")) return body;
            }

            throw new Exception($"No se pudo descargar '{filePath}'.");
        }

        public async Task<(byte[] Bytes, string ContentType)> DownloadBySharingUrlAsync(string sharingUrl)
        {
            await EnsureSessionAsync();

            var filePath = ExtractPathFromUrl(sharingUrl) ?? "";
            if (filePath.EndsWith("/download", StringComparison.OrdinalIgnoreCase))
                filePath = filePath[..^"/download".Length];
            if (!filePath.StartsWith("/team-folders", StringComparison.OrdinalIgnoreCase))
                filePath = $"/team-folders{(filePath.StartsWith('/') ? "" : "/")}{filePath}";

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            var fallbackCt = ext switch
            {
                ".png"           => "image/png",
                ".jpg" or ".jpeg"=> "image/jpeg",
                ".gif"           => "image/gif",
                ".webp"          => "image/webp",
                _                => "application/octet-stream"
            };

            // Paso 1: obtener file_id y version_id
            var getRaw = await CallAsync(HttpMethod.Get,
                $"api=SYNO.SynologyDrive.Files&version=2&method=get&path={Uri.EscapeDataString(filePath)}");
            using var getDoc = JsonDocument.Parse(getRaw);
            var getRoot = getDoc.RootElement;
            if (!getRoot.TryGetProperty("success", out var ok) || !ok.GetBoolean())
                throw new Exception($"method=get falló: {getRaw[..Math.Min(200, getRaw.Length)]}");

            var data      = getRoot.GetProperty("data");
            var fileId    = data.GetProperty("file_id").GetString()!;
            var versionId = data.TryGetProperty("sync_id",    out var sp) ? sp.GetInt64().ToString()
                          : data.TryGetProperty("version_id", out var vp) ? vp.ToString() : "1";

            // Paso 2: get_thumbnail (respuesta binaria)
            var tokenSuffix = !string.IsNullOrEmpty(_synoToken) ? $"&SynoToken={Uri.EscapeDataString(_synoToken)}" : "";
            var thumbUrl = $"{_baseUrl}/webapi/entry.cgi"
                + $"?api=SYNO.SynologyDrive.Files&method=get_thumbnail&version=2"
                + $"&path={Uri.EscapeDataString($"\"id:{fileId}\"")}"
                + $"&animate=true&size={Uri.EscapeDataString("\"large\"")}"
                + $"&version_id={Uri.EscapeDataString($"\"{versionId}\"")}"
                + $"&online_convert=false&is_preview=true"
                + $"&_sid={Uri.EscapeDataString(_sid!)}{tokenSuffix}";

            using var thumbReq = new HttpRequestMessage(HttpMethod.Get, thumbUrl);
            if (!string.IsNullOrEmpty(_cookieHeader)) thumbReq.Headers.TryAddWithoutValidation("Cookie", _cookieHeader);
            if (!string.IsNullOrEmpty(_synoToken))    thumbReq.Headers.TryAddWithoutValidation("X-Syno-Token", _synoToken);

            var thumbRes  = await _httpClient.SendAsync(thumbReq);
            var thumbBody = await thumbRes.Content.ReadAsByteArrayAsync();
            var thumbCt   = thumbRes.Content.Headers.ContentType?.MediaType ?? "";

            if (thumbRes.IsSuccessStatusCode && !thumbCt.Contains("html") && thumbBody.Length > 100)
                return (thumbBody, string.IsNullOrEmpty(thumbCt) ? fallbackCt : thumbCt);

            var preview = Encoding.UTF8.GetString(thumbBody, 0, Math.Min(200, thumbBody.Length));
            throw new Exception($"get_thumbnail falló: HTTP {(int)thumbRes.StatusCode} — {preview}");
        }

        // ── DTOs ─────────────────────────────────────────────────────────────

        private class SynologyBaseResponse
        {
            [JsonPropertyName("success")] public bool Success { get; set; }
            [JsonPropertyName("error")]   public SynologyError? Error { get; set; }
        }

        private class SynologyResponse<T> : SynologyBaseResponse
        {
            [JsonPropertyName("data")] public T? Data { get; set; }
        }

        private class SynologyError
        {
            [JsonPropertyName("code")] public int Code { get; set; }
        }

        private class LoginData
        {
            [JsonPropertyName("sid")]       public string  Sid       { get; set; } = "";
            [JsonPropertyName("did")]       public string? Did       { get; set; }
            [JsonPropertyName("synotoken")] public string? SynoToken { get; set; }
        }
    }
}
