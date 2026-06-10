using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        private readonly string _baseUrl;    // e.g. https://apidrive.ulima.edu.pe/drive  (Drive webapi)
        private readonly string _dsmBaseUrl; // e.g. https://apidrive.ulima.edu.pe        (DSM root webapi)
        private readonly string _username;
        private readonly string _password;

        private string? _sid;
        private string? _did;
        private string? _synoToken;
        private string  _cookieHeader = "";   // all cookies from Set-Cookie, forwarded verbatim
        private DateTime _sidExpiry = DateTime.MinValue;
        private readonly SemaphoreSlim _loginLock = new(1, 1);

        public SynologyService(HttpClient httpClient, CookieContainer _)
        {
            _httpClient = httpClient;
            _baseUrl = (Environment.GetEnvironmentVariable("SYNOLOGY_BASE_URL") ?? "http://localhost:5000").TrimEnd('/');
            _username = Environment.GetEnvironmentVariable("SYNOLOGY_USERNAME") ?? "";
            _password = Environment.GetEnvironmentVariable("SYNOLOGY_PASSWORD") ?? "";

            // DSM portal sits at the root authority (strip the /drive path segment).
            // Auth at the DSM level grants full write permissions needed for uploads.
            var uri = new Uri(_baseUrl);
            _dsmBaseUrl = $"{uri.Scheme}://{uri.Authority}";
        }

        // ── Auth ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses every Set-Cookie header from an HTTP response into a dictionary.
        /// </summary>
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

                // ── Step 1: pre-flight GET to the Drive web UI ─────────────────
                // A browser always navigates to /drive/ before logging in; the server
                // sets _SSID (and possibly other session cookies) in response.
                // Sending those cookies with the auth POST and subsequent requests
                // produces the dotted SynoToken (e.g. "UIC6.niSy68f6") that the
                // upload endpoint requires, instead of the limited non-dotted form.
                var preflight = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    using var pfReq = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/");
                    pfReq.Headers.TryAddWithoutValidation("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    var pfRes = await _httpClient.SendAsync(pfReq);
                    preflight = ParseSetCookies(pfRes);
                    Console.WriteLine($"[Preflight] Set-Cookie keys: [{string.Join(", ", preflight.Keys)}]");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Preflight] skipped: {ex.Message}");
                }

                // ── Step 2: authenticate ────────────────────────────────────────
                // stay_login=1 (integer, not "yes") triggers the dotted SynoToken.
                // All preflight cookies are forwarded with the auth POST so the server
                // binds the session to any _SSID it already set.
                const string authSuffix =
                    "?api=SYNO.API.Auth&version=3&method=login" +
                    "&session=SynologyDrive&format=cookie&enable_syno_token=yes";

                SynologyResponse<LoginData>? data = null;
                string raw = "";
                var setCookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // Carry preflight cookies into the auth request
                var preflightCookieHeader = preflight.Count > 0
                    ? string.Join("; ", preflight.Select(kv => $"{kv.Key}={kv.Value}"))
                    : "";

                foreach (var authBase in new[] { _dsmBaseUrl, _baseUrl })
                {
                    var url = $"{authBase}/webapi/auth.cgi{authSuffix}";
                    Console.WriteLine($"[Login] Trying {authBase}/webapi/auth.cgi …");
                    using var req = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            ["account"]    = _username,
                            ["passwd"]     = _password,
                            ["stay_login"] = "1"       // integer form — produces dotted SynoToken
                        })
                    };
                    if (!string.IsNullOrEmpty(preflightCookieHeader))
                        req.Headers.TryAddWithoutValidation("Cookie", preflightCookieHeader);
                    try
                    {
                        var res = await _httpClient.SendAsync(req);
                        raw = await res.Content.ReadAsStringAsync();
                        Console.WriteLine($"[Login] {authBase} HTTP {(int)res.StatusCode} → {raw[..Math.Min(300, raw.Length)]}");

                        // Merge preflight cookies with auth Set-Cookie (auth takes precedence)
                        setCookies = new Dictionary<string, string>(preflight, StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in ParseSetCookies(res))
                            setCookies[kv.Key] = kv.Value;
                        Console.WriteLine($"[Login] Set-Cookie keys: [{string.Join(", ", setCookies.Keys)}]");

                        data = Parse<SynologyResponse<LoginData>>(raw);

                        // With format=cookie the SID lives in the 'id' cookie, not in JSON.
                        if (setCookies.TryGetValue("id", out var sidFromCookie)
                            && !string.IsNullOrEmpty(sidFromCookie))
                        {
                            data ??= new SynologyResponse<LoginData> { Success = true };
                            data.Data ??= new LoginData();
                            data.Data.Sid = sidFromCookie;
                        }

                        if (data?.Success == true && !string.IsNullOrEmpty(data.Data?.Sid))
                            break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Login] {authBase} unreachable: {ex.Message}");
                    }
                }

                if (data?.Success != true || string.IsNullOrEmpty(data.Data?.Sid))
                    throw new Exception($"Login falló: {raw}");

                _sid       = data.Data!.Sid;
                _did       = setCookies.GetValueOrDefault("did") ?? data.Data.Did;
                _synoToken = data.Data.SynoToken
                          ?? setCookies.GetValueOrDefault("io");

                // Build cookie header — all server-set cookies + stay_login=1 if absent
                var cookieParts = setCookies.Select(kv => $"{kv.Key}={kv.Value}").ToList();
                if (!setCookies.ContainsKey("stay_login"))
                    cookieParts.Add("stay_login=1");
                _cookieHeader = string.Join("; ", cookieParts);
                Console.WriteLine($"[Login] cookieHeader = {_cookieHeader}");

                _sidExpiry = DateTime.UtcNow.AddHours(6);
                Console.WriteLine($"✅ Login exitoso — did={_did ?? "(none)"} SynoToken={_synoToken ?? "(none)"}");
            }
            finally { _loginLock.Release(); }
        }

        private bool IsAuthError(SynologyBaseResponse r) =>
            !r.Success && r.Error != null && (r.Error.Code == 106 || r.Error.Code == 119);

        // ── HTTP ──────────────────────────────────────────────────────────────

        /// <summary>Sends an authenticated request to the Drive webapi.</summary>
        private async Task<string> CallAsync(HttpMethod method, string apiParams, HttpContent? body = null)
        {
            await EnsureSessionAsync();
            var tokenSuffix = !string.IsNullOrEmpty(_synoToken)
                ? $"&SynoToken={Uri.EscapeDataString(_synoToken)}"
                : "";
            var url = $"{_baseUrl}/webapi/entry.cgi?{apiParams}&_sid={Uri.EscapeDataString(_sid!)}{tokenSuffix}";

            using var req = new HttpRequestMessage(method, url);
            if (body != null)
            {
                req.Content = body;
                req.Headers.ExpectContinue = false;
            }

            // Forward the full cookie jar captured at login (includes _SSID, id, did,
            // stay_login, io, etc.) — same as what the browser sends.
            if (!string.IsNullOrEmpty(_cookieHeader))
                req.Headers.TryAddWithoutValidation("Cookie", _cookieHeader);

            // X-Syno-Token header — the browser sends SynoToken both in URL and as a header.
            if (!string.IsNullOrEmpty(_synoToken))
                req.Headers.TryAddWithoutValidation("X-Syno-Token", _synoToken);

            var res = await _httpClient.SendAsync(req);
            var raw = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"[{method} entry.cgi?{apiParams[..Math.Min(80, apiParams.Length)]}...] {raw[..Math.Min(400, raw.Length)]}");
            return raw;
        }

        private static T? Parse<T>(string raw) where T : class =>
            JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // ── Folder helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Gets the file_id of an existing folder, or null if it doesn't exist.
        /// The file_id is required by the upload API (as parent_id).
        /// </summary>
        private async Task<string?> GetFolderIdAsync(string drivePath)
        {
            var raw = await CallAsync(HttpMethod.Get,
                $"api=SYNO.SynologyDrive.Files&version=2&method=get&path={Uri.EscapeDataString(drivePath)}");
            Console.WriteLine($"[GetFolderId] {drivePath} → {raw}");
            var doc = JsonDocument.Parse(raw).RootElement;
            if (!doc.TryGetProperty("success", out var s) || s.GetBoolean() != true) return null;
            if (doc.TryGetProperty("data", out var data) &&
                data.TryGetProperty("file_id", out var fid))
                return fid.GetString();
            return null;
        }

        /// <summary>
        /// Ensures a folder exists and returns its file_id.
        /// </summary>
        private async Task<string> GetOrCreateFolderAsync(string drivePath)
        {
            var existing = await GetFolderIdAsync(drivePath);
            if (existing != null)
            {
                Console.WriteLine($"ℹ️  Carpeta ya existe ({existing}): {drivePath}");
                return existing;
            }

            var raw = await CallAsync(HttpMethod.Put,
                $"api=SYNO.SynologyDrive.Files&version=2&method=create" +
                $"&path={Uri.EscapeDataString(drivePath)}&type=folder&conflict_action=skip");
            Console.WriteLine($"[CreateFolder] {drivePath} → {raw}");

            var doc = JsonDocument.Parse(raw).RootElement;
            if (doc.TryGetProperty("success", out var s) && s.GetBoolean() &&
                doc.TryGetProperty("data", out var data) &&
                data.TryGetProperty("file_id", out var fid))
            {
                var id = fid.GetString()!;
                Console.WriteLine($"✅ Carpeta creada ({id}): {drivePath}");
                return id;
            }

            var code = doc.TryGetProperty("error", out var err) && err.TryGetProperty("code", out var c)
                ? c.GetInt32() : 0;
            if (code is 400 or 105 or 405 or 1022 or 1100)
            {
                // Already existed but skip returned no data — try GET again
                var retry = await GetFolderIdAsync(drivePath);
                if (retry != null) return retry;
            }

            throw new Exception($"Error creando carpeta '{drivePath}': {raw}");
        }

        /// <summary>
        /// Creates every folder segment and returns the file_id of the last one.
        /// </summary>
        private async Task<string> EnsureFolderPathAsync(string fullPath)
        {
            var segments = fullPath.Trim('/').Split('/');
            var current  = "";
            var lastId   = "";
            foreach (var seg in segments)
            {
                current = $"{current}/{seg}";
                if (string.Equals(seg, "team-folders", StringComparison.OrdinalIgnoreCase))
                    continue;
                lastId = await GetOrCreateFolderAsync(current);
            }
            return lastId;
        }

        // ── Upload ────────────────────────────────────────────────────────────

        private async Task<string> UploadAsync(string driveFolderPath, IFormFile file, string fileName)
        {
            var fullPath = $"{driveFolderPath.TrimEnd('/')}/{fileName}";

            // Read file bytes upfront — reused on auth-retry without re-reading the stream.
            using var ms = new MemoryStream();
            await file.OpenReadStream().CopyToAsync(ms);
            var fileBytes = ms.ToArray();
            Console.WriteLine($"[Upload] fileBytes={fileBytes.Length} contentType={file.ContentType}");

            async Task<(string Raw, SynologyBaseResponse Parsed)> TryAsync()
            {
                await EnsureSessionAsync();

                var modifiedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
                var uploadParams =
                    $"api=SYNO.SynologyDrive.Files&method=upload&version=2" +
                    $"&modified_time={modifiedTime:F3}" +
                    $"&path={Uri.EscapeDataString(fullPath)}" +
                    $"&type=file&conflict_action=overwrite";

                // Build multipart body manually to exactly match the browser's format.
                // Using a WebKit-style boundary and explicit CRLF gives the server the
                // same structure it receives from Chrome — no surprises in the parser.
                var contentType = file.ContentType ?? "application/octet-stream";
                var boundary = "----WebKitFormBoundaryCorrePalabrasUpload";
                var header = Encoding.UTF8.GetBytes(
                    $"--{boundary}\r\n" +
                    $"Content-Disposition: form-data; name=\"file\"; filename=\"{fileName}\"\r\n" +
                    $"Content-Type: {contentType}\r\n" +
                    $"\r\n");
                var footer = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

                var body = new byte[header.Length + fileBytes.Length + footer.Length];
                Buffer.BlockCopy(header, 0, body, 0,                    header.Length);
                Buffer.BlockCopy(fileBytes, 0, body, header.Length,     fileBytes.Length);
                Buffer.BlockCopy(footer, 0, body, header.Length + fileBytes.Length, footer.Length);

                var uploadBody = new ByteArrayContent(body);
                uploadBody.Headers.ContentType =
                    MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");

                Console.WriteLine($"[Upload] body={body.Length}B path={fullPath}");
                var raw = await CallAsync(HttpMethod.Post, uploadParams, uploadBody);
                return (raw, Parse<SynologyBaseResponse>(raw)!);
            }

            // Ensure the destination folder exists (creates it if needed)
            await EnsureFolderPathAsync(driveFolderPath);

            var (raw, parsed) = await TryAsync();

            if (IsAuthError(parsed)) { _sid = null; (raw, parsed) = await TryAsync(); }

            Console.WriteLine($"[UploadFile] {fileName} → {raw}");

            if (!parsed.Success)
                throw new Exception($"Upload falló (código {parsed.Error?.Code}): {raw}");

            Console.WriteLine($"✅ Archivo subido: {fullPath}");
            return fullPath;
        }

        // ── Sharing ───────────────────────────────────────────────────────────

        private async Task<string> CreateShareAsync(string drivePath)
        {
            // Try the known Drive sharing API variants in order.
            // Error 101 = invalid parameters / unknown method; try next variant.
            var attempts = new[]
            {
                $"api=SYNO.SynologyDrive.Sharing&version=2&method=create&path={Uri.EscapeDataString(drivePath)}",
                $"api=SYNO.SynologyDrive.Sharing&version=1&method=create&path={Uri.EscapeDataString(drivePath)}",
                $"api=SYNO.SynologyDrive.Sharing&version=3&method=create&path={Uri.EscapeDataString(drivePath)}",
            };

            string raw = "";
            SynologyBaseResponse? res = null;

            foreach (var attempt in attempts)
            {
                raw = await CallAsync(HttpMethod.Post, attempt);
                res = Parse<SynologyBaseResponse>(raw);
                Console.WriteLine($"[Share attempt] {attempt[..Math.Min(60,attempt.Length)]} → {raw[..Math.Min(200,raw.Length)]}");

                if (IsAuthError(res!)) { _sid = null; raw = await CallAsync(HttpMethod.Post, attempt); res = Parse<SynologyBaseResponse>(raw); }

                if (res?.Success == true) break;
                if (res?.Error?.Code != 101 && res?.Error?.Code != 102 && res?.Error?.Code != 119) break; // unexpected error
            }

            string? shareUrl = null;
            if (res?.Success == true)
            {
                var root = JsonDocument.Parse(raw).RootElement;
                if (root.TryGetProperty("data", out var data))
                {
                    if (data.TryGetProperty("url",   out var u)  && u.ValueKind  == JsonValueKind.String) shareUrl = u.GetString();
                    if (shareUrl == null && data.TryGetProperty("link",  out var l)  && l.ValueKind  == JsonValueKind.String) shareUrl = l.GetString();
                    if (shareUrl == null && data.TryGetProperty("links", out var ls) && ls.ValueKind == JsonValueKind.Array && ls.GetArrayLength() > 0)
                    {
                        var first = ls[0];
                        if (first.TryGetProperty("url",  out var lu)) shareUrl = lu.GetString();
                        if (shareUrl == null && first.TryGetProperty("link", out var ll)) shareUrl = ll.GetString();
                    }
                }
            }

            if (string.IsNullOrEmpty(shareUrl))
                throw new Exception($"Share falló: {raw}");

            // Embed the file path so DeleteBySharingUrlAsync can resolve it later
            var finalUrl = shareUrl.Contains('?')
                ? $"{shareUrl}&path={Uri.EscapeDataString(drivePath)}"
                : $"{shareUrl}?path={Uri.EscapeDataString(drivePath)}";

            Console.WriteLine($"✅ Share: {finalUrl}");
            return finalUrl;
        }

        // ── Delete ────────────────────────────────────────────────────────────

        private static string? ExtractPathFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                return query.TryGetValue("path", out var v) ? v.ToString() : null;
            }
            catch { return null; }
        }

        // ── Public interface ──────────────────────────────────────────────────

        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            // Callers pass FileStation-style paths (/CPAPPDEV/...).
            // Drive API requires the /team-folders/ prefix.
            if (!destinationFolder.StartsWith("/team-folders", StringComparison.OrdinalIgnoreCase))
                destinationFolder = $"/team-folders{(destinationFolder.StartsWith('/') ? "" : "/")}{destinationFolder}";

            Console.WriteLine($"[UploadAndShare] folder={destinationFolder} file={fileName}");

            var drivePath = await UploadAsync(destinationFolder, file, fileName);
            return await CreateShareAsync(drivePath);
        }

        public async Task DeleteBySharingUrlAsync(string sharingUrl)
        {
            if (string.IsNullOrEmpty(sharingUrl)) return;

            var path = ExtractPathFromUrl(sharingUrl);
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine($"⚠️  No se pudo extraer path de: {sharingUrl}");
                return;
            }

            await DeleteByPathAsync(path);
        }

        public async Task DeleteByPathAsync(string filePath)
        {
            if (!filePath.StartsWith("/team-folders", StringComparison.OrdinalIgnoreCase))
                filePath = $"/team-folders{(filePath.StartsWith('/') ? "" : "/")}{filePath}";

            var pathJson = $"[\"{filePath.Replace("\"", "\\\"")}\"]";
            var raw = await CallAsync(HttpMethod.Post,
                $"api=SYNO.SynologyDrive.Files&version=2&method=delete" +
                $"&path={Uri.EscapeDataString(pathJson)}");

            var res = Parse<SynologyBaseResponse>(raw);
            if (IsAuthError(res!)) { _sid = null; await DeleteByPathAsync(filePath); return; }
            if (!res!.Success) throw new Exception($"Delete falló (código {res.Error?.Code}): {raw}");

            Console.WriteLine($"✅ Eliminado: {filePath}");
        }

        public async Task<byte[]> DownloadFileAsync(string filePath)
        {
            if (!filePath.StartsWith("/team-folders", StringComparison.OrdinalIgnoreCase))
                filePath = $"/team-folders{(filePath.StartsWith('/') ? "" : "/")}{filePath}";

            await EnsureSessionAsync();
            var url = $"{_baseUrl}/webapi/entry.cgi" +
                      $"?api=SYNO.SynologyDrive.Files&version=2&method=download" +
                      $"&path={Uri.EscapeDataString(filePath)}" +
                      $"&_sid={Uri.EscapeDataString(_sid!)}";

            var res = await _httpClient.GetAsync(url);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsByteArrayAsync();
        }

        // ── DTOs ──────────────────────────────────────────────────────────────

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
            // With format=cookie, sid comes from the Set-Cookie 'id' header, not JSON.
            // We patch it in manually after parsing the response.
            [JsonPropertyName("sid")]        public string  Sid        { get; set; } = "";
            [JsonPropertyName("did")]        public string? Did        { get; set; }
            [JsonPropertyName("synotoken")]  public string? SynoToken  { get; set; }
        }
    }
}
