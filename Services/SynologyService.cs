using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace CorrePalabras.Services
{
    public class SynologyService : ISynologyService
    {
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;
        private readonly string _synologyBaseUrl;
        private readonly string _username;
        private readonly string _password;

        private string? _cachedSid;
        private string? _cachedDid;
        private DateTime _sidExpiration = DateTime.MinValue;
        private readonly SemaphoreSlim _loginSemaphore = new(1, 1);

        public SynologyService(HttpClient httpClient, CookieContainer cookieContainer)
        {
            _httpClient = httpClient;
            _cookieContainer = cookieContainer;
            _synologyBaseUrl = Environment.GetEnvironmentVariable("SYNOLOGY_BASE_URL") ?? "http://localhost:5000";
            _username = Environment.GetEnvironmentVariable("SYNOLOGY_USERNAME") ?? "";
            _password = Environment.GetEnvironmentVariable("SYNOLOGY_PASSWORD") ?? "";
        }

        private async Task EnsureValidSessionAsync()
        {
            if (!string.IsNullOrEmpty(_cachedSid) && DateTime.UtcNow < _sidExpiration)
                return;

            await _loginSemaphore.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_cachedSid) && DateTime.UtcNow < _sidExpiration)
                    return;

                await RefreshLoginAsync();
            }
            finally
            {
                _loginSemaphore.Release();
            }
        }

        private async Task RefreshLoginAsync()
        {
            _cachedSid = _cachedDid = null;
            _sidExpiration = DateTime.MinValue;

            // Credentials go in the POST body so they never appear in logged URLs.
            string url = $"{_synologyBaseUrl}/webapi/auth.cgi?api=SYNO.API.Auth&version=3&method=login&session=SynologyDrive&format=cookie";

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["account"] = _username,
                    ["passwd"]  = _password
                })
            };
            var response = await _httpClient.SendAsync(request);
            var raw = await response.Content.ReadAsStringAsync();

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                Console.WriteLine($"[Login] Set-Cookie: {string.Join("; ", setCookies)}");
            }

            LogCurrentCookies();
            Console.WriteLine($"[Login Response Raw] {raw}");

            // The API always responds with JSON — parse it directly.
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var responseData = JsonSerializer.Deserialize<SynologyLoginResponse>(raw, options);

                if (responseData?.Success == true && responseData.Data != null)
                {
                    _cachedSid = responseData.Data.Sid;
                    _cachedDid = responseData.Data.Did;
                    _sidExpiration = DateTime.UtcNow.AddHours(6);
                    EnsureCookieContainerHasAuthCookies();
                    Console.WriteLine("✅ [Synology] Login exitoso");
                    return;
                }

                throw new Exception($"❌ Login falló: {raw}");
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Login falló: {raw}", ex);
            }
        }

        private bool IsAuthError(string body)
        {
            try
            {
                var xml = LoadXml(body);
                var successValue = GetXmlValue(xml, "/response/success");
                if (!string.Equals(successValue, "true", StringComparison.OrdinalIgnoreCase))
                {
                    var code = GetXmlValue(xml, "/response/error/code");
                    return code == "106" || code == "119";
                }
            }
            catch
            {
                try
                {
                    using var document = JsonDocument.Parse(body);
                    if (document.RootElement.TryGetProperty("error", out var errorElement) &&
                        errorElement.TryGetProperty("code", out var codeElement) &&
                        codeElement.TryGetInt32(out var code))
                    {
                        return code == 106 || code == 119;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private async Task LogMultipartContentAsync(MultipartFormDataContent content, string uploadFileName, long uploadLength)
        {
            Console.WriteLine("[UploadFile] Multipart request preview:");
            foreach (var part in content)
            {
                var disposition = part.Headers.ContentDisposition;
                var name = disposition?.Name?.Trim('"') ?? "(unknown)";
                var filename = disposition?.FileName?.Trim('"');
                var contentType = part.Headers.ContentType?.MediaType ?? "text/plain";
                var value = string.Empty;

                if (filename is null)
                {
                    value = await part.ReadAsStringAsync();
                }
                else
                {
                    value = $"<file: {filename}, length={uploadLength}, type={contentType}>";
                }

                Console.WriteLine($"  part: name='{name}', filename='{filename}', type='{contentType}', value='{value}'");
            }
        }

        private XmlDocument LoadXml(string raw)
        {
            var xml = new XmlDocument();
            xml.LoadXml(raw);
            return xml;
        }

        private string? GetXmlValue(XmlDocument xml, string xpath)
        {
            var node = xml.SelectSingleNode(xpath);
            return node?.InnerText;
        }

        private void LogCurrentCookies()
        {
            try
            {
                var uri = new Uri(_synologyBaseUrl);
                var cookies = _cookieContainer.GetCookies(uri);
                Console.WriteLine($"[CookieContainer] {uri.Host} cookies: {cookies.Count}");
                foreach (System.Net.Cookie cookie in cookies)
                {
                    Console.WriteLine($"  cookie: {cookie.Name}={cookie.Value}; path={cookie.Path}; expires={cookie.Expires}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CookieContainer] Failed to log cookies: {ex.Message}");
            }
        }

        private void EnsureCookieContainerHasAuthCookies()
        {
            try
            {
                var uri = new Uri(_synologyBaseUrl);
                var cookies = _cookieContainer.GetCookies(uri);
                if (cookies["id"] == null && !string.IsNullOrEmpty(_cachedSid))
                {
                    _cookieContainer.Add(uri, new Cookie("id", _cachedSid, "/") { HttpOnly = true });
                }
                if (cookies["did"] == null && !string.IsNullOrEmpty(_cachedDid))
                {
                    _cookieContainer.Add(uri, new Cookie("did", _cachedDid, "/") { HttpOnly = true });
                }
                LogCurrentCookies();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EnsureCookieContainerHasAuthCookies] Failed: {ex.Message}");
            }
        }

        private async Task<XmlDocument> SendXmlRequestAsync(string url, HttpContent? content = null)
        {
            using var request = new HttpRequestMessage(content == null ? HttpMethod.Get : HttpMethod.Post, url);
            if (content != null)
            {
                request.Content = content;
                request.Headers.ExpectContinue = false;
            }

            if (request.RequestUri != null)
            {
                var cookies = _cookieContainer.GetCookies(request.RequestUri);
                var cookieHeader = string.Join("; ", cookies.Cast<System.Net.Cookie>().Select(c => $"{c.Name}={c.Value}"));
                if (!string.IsNullOrWhiteSpace(cookieHeader) && !request.Headers.Contains("Cookie"))
                {
                    request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
                }

                Console.WriteLine($"[SendXmlRequestAsync] Request cookies: {cookieHeader}");
                if (request.Headers.Contains("Cookie"))
                {
                    var headers = string.Join("; ", request.Headers.GetValues("Cookie"));
                    Console.WriteLine($"[SendXmlRequestAsync] Cookie header sent: {headers}");
                }
            }

            var response = await _httpClient.SendAsync(request);
            var raw = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[SendXmlRequestAsync] {request.Method} {url} -> {response.StatusCode}");
            return ParseSynologyResponse(raw);
        }

        private XmlDocument ParseSynologyResponse(string raw)
        {
            try
            {
                return LoadXml(raw);
            }
            catch (XmlException)
            {
                try
                {
                    using var document = JsonDocument.Parse(raw);
                    var xml = new XmlDocument();
                    var root = xml.CreateElement("response");
                    xml.AppendChild(root);
                    PopulateXmlFromJsonElement(xml, root, document.RootElement);
                    Console.WriteLine($"[ParseSynologyResponse] Converted JSON response to XML");
                    return xml;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[ParseSynologyResponse] Response not XML or JSON: {raw}");
                    throw new Exception($"Respuesta inválida de Synology: {raw}", ex);
                }
            }
        }

        private void PopulateXmlFromJsonElement(XmlDocument xml, XmlElement parent, JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var child = xml.CreateElement(property.Name);
                        parent.AppendChild(child);
                        PopulateXmlFromJsonElement(xml, child, property.Value);
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        var child = xml.CreateElement("item");
                        parent.AppendChild(child);
                        PopulateXmlFromJsonElement(xml, child, item);
                    }
                    break;
                case JsonValueKind.String:
                    parent.InnerText = element.GetString() ?? string.Empty;
                    break;
                case JsonValueKind.Number:
                    parent.InnerText = element.GetRawText();
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    parent.InnerText = element.GetBoolean().ToString().ToLowerInvariant();
                    break;
                case JsonValueKind.Null:
                    parent.InnerText = string.Empty;
                    break;
            }
        }

        private async Task<byte[]> DownloadRawAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        private async Task CreateFolderAsync(string parentPath, string folderName)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.CreateFolder&version=2&method=create" +
                         $"&folder_path={Uri.EscapeDataString(parentPath)}" +
                         $"&name={Uri.EscapeDataString(folderName)}" +
                         $"&force_parent=true" +
                         $"&format=xml";

            async Task<XmlDocument> ExecuteAsync()
            {
                await EnsureValidSessionAsync();
                return await SendXmlRequestAsync(url);
            }

            var xml = await ExecuteAsync();
            if (IsAuthError(xml.OuterXml))
            {
                await RefreshLoginAsync();
                xml = await ExecuteAsync();
            }

            Console.WriteLine($"[CreateFolder] {parentPath}/{folderName} | Response: {xml.OuterXml}");

            if (GetXmlValue(xml, "/response/success") == "true")
            {
                Console.WriteLine("✅ Carpeta creada");
                return;
            }

            var code = GetXmlValue(xml, "/response/error/code");
            if (code == "400")
            {
                Console.WriteLine("ℹ️ Carpeta ya existía");
                return;
            }

            throw new Exception($"Error creando carpeta: {xml.OuterXml}");
        }

        // ======================= UPLOAD (Synology Drive API) =======================
        private async Task<string> UploadFileAsync(string targetPath, IFormFile file, string fileName)
        {
            // Synology Drive Team Folders require the Drive-native upload API.
            // The path for a Team Folder must start with /team-folders/<folderName>/...
            // We derive that from the NAS path: /CPAPPDEV/img/... → /team-folders/CPAPPDEV/img/...
            string driveDestPath = ConvertNasPathToDrivePath(targetPath);

            MultipartFormDataContent CreateContent()
            {
                var content = new MultipartFormDataContent();
                var stream = new StreamContent(file.OpenReadStream());
                stream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType ?? "application/octet-stream");
                content.Add(stream, "content", fileName);
                return content;
            }

            async Task<XmlDocument> TryUploadAsync()
            {
                await EnsureValidSessionAsync();
                var sid = _cachedSid ?? string.Empty;
                var filePath = $"{driveDestPath}/{fileName}";

                // path and type must go in the query string, not the body
                var uploadUrl = $"{_synologyBaseUrl}/webapi/entry.cgi" +
                                $"?api=SYNO.SynologyDrive.Files&version=2&method=upload" +
                                $"&_sid={Uri.EscapeDataString(sid)}" +
                                $"&path={Uri.EscapeDataString(filePath)}" +
                                $"&type=file" +
                                $"&conflict_action=version";

                using var content = CreateContent();
                await LogMultipartContentAsync(content, fileName, file.Length);

                using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                {
                    Content = content
                };
                request.Headers.ExpectContinue = false;
                request.Headers.UserAgent.ParseAdd("CorrePalabras/1.0");

                Console.WriteLine($"[TryUpload] Drive upload → dest: {driveDestPath}, file: {fileName}");

                var response = await _httpClient.SendAsync(request);
                var raw = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[TryUpload] Status: {(int)response.StatusCode} | Body: {raw}");

                return ParseSynologyResponse(raw);
            }

            var xml = await TryUploadAsync();

            if (IsAuthError(xml.OuterXml) || GetXmlValue(xml, "/response/error/code") == "401")
            {
                Console.WriteLine("[UploadFile] Auth error, refreshing session and retrying.");
                await RefreshLoginAsync();
                xml = await TryUploadAsync();
            }

            Console.WriteLine($"[UploadFile] {fileName} → {xml.OuterXml}");

            if (GetXmlValue(xml, "/response/success") == "true")
            {
                Console.WriteLine("✅ Archivo subido correctamente");
                // Return NAS-style path so CreateShareByPathAsync can share it via FileStation
                return $"{targetPath}/{fileName}";
            }

            throw new Exception($"Error al subir archivo: {xml.OuterXml}");
        }

        /// <summary>
        /// Converts a NAS filesystem path to the Synology Drive virtual path.
        /// Example: /CPAPPDEV/img/avatars → /team-folders/CPAPPDEV/img/avatars
        /// </summary>
        private static string ConvertNasPathToDrivePath(string nasPath)
        {
            // nasPath starts with /CPAPPDEV/... which is a Team Folder root named CPAPPDEV
            var trimmed = nasPath.TrimStart('/');
            return "/team-folders/" + trimmed;
        }
        private async Task<string> CreateShareByPathAsync(string filePath)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=create" +
                         $"&path={Uri.EscapeDataString($"[\"{filePath.Replace("\"", "\\\"")}\"]")}" +
                         $"&format=xml";

            async Task<XmlDocument> ExecuteAsync()
            {
                await EnsureValidSessionAsync();
                return await SendXmlRequestAsync(url);
            }

            var xml = await ExecuteAsync();
            if (IsAuthError(xml.OuterXml))
            {
                await RefreshLoginAsync();
                xml = await ExecuteAsync();
            }

            Console.WriteLine($"[CreateShare] Response: {xml.OuterXml}");

            if (GetXmlValue(xml, "/response/success") == "true")
            {
                var shareUrl = GetXmlValue(xml, "/response/data/links/link/url");
                if (!string.IsNullOrEmpty(shareUrl))
                {
                    Console.WriteLine($"✅ Share generado: {shareUrl}");
                    return shareUrl;
                }
            }

            throw new Exception($"Error creando share: {xml.OuterXml}");
        }

        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            Console.WriteLine($"[UploadAndShare] Iniciando para: {destinationFolder}");

            // Normalize: remove trailing slash
            var fullFolderPath = destinationFolder.TrimEnd('/');

            // Ensure the full folder hierarchy exists.
            // We split the path and create each segment with force_parent=true,
            // which lets Synology create all intermediate directories in one call.
            var lastSlash = fullFolderPath.LastIndexOf('/');
            var parentPath = lastSlash > 0 ? fullFolderPath[..lastSlash] : "/";
            var folderName = fullFolderPath[(lastSlash + 1)..];

            await CreateFolderAsync(parentPath, folderName);
            var fullFilePath = await UploadFileAsync(fullFolderPath, file, fileName);

            return await CreateShareByPathAsync(fullFilePath);
        }

        public async Task DeleteBySharingUrlAsync(string sharingUrl)
        {
            if (string.IsNullOrEmpty(sharingUrl))
                return;

            var path = ExtractPathFromSharingUrl(sharingUrl);
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("No se pudo extraer la ruta de archivo del sharingUrl.", nameof(sharingUrl));

            await DeleteByPathAsync(path);
        }

        public async Task DeleteByPathAsync(string filePath)
        {
            var deleteUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Delete&version=2&method=delete" +
                            $"&path={Uri.EscapeDataString($"[\"{filePath.Replace("\"", "\\\"")}\"]")}" +
                            $"&format=xml";

            async Task<XmlDocument> ExecuteAsync()
            {
                await EnsureValidSessionAsync();
                return await SendXmlRequestAsync(deleteUrl);
            }

            var xml = await ExecuteAsync();
            if (IsAuthError(xml.OuterXml))
            {
                await RefreshLoginAsync();
                xml = await ExecuteAsync();
            }

            if (GetXmlValue(xml, "/response/success") == "true")
                return;

            throw new Exception($"Error borrando archivo: {xml.OuterXml}");
        }

        public async Task<byte[]> DownloadFileAsync(string filePath)
        {
            var downloadUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Download&version=2&method=download" +
                              $"&path={Uri.EscapeDataString(filePath)}";

            await EnsureValidSessionAsync();
            return await DownloadRawAsync(downloadUrl);
        }

        private string? ExtractPathFromSharingUrl(string sharingUrl)
        {
            try
            {
                var uri = new Uri(sharingUrl);
                var query = QueryHelpers.ParseQuery(uri.Query);
                if (query.TryGetValue("path", out var pathValues))
                    return pathValues.FirstOrDefault();

                return null;
            }
            catch
            {
                return null;
            }
        }

        #region DTOs
        public class SynologyBaseResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("success")] public bool Success { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("error")] public SynologyError? Error { get; set; }
        }

        public class SynologyError
        {
            [System.Text.Json.Serialization.JsonPropertyName("code")] public int Code { get; set; }
        }

        public class SynologyLoginResponse : SynologyBaseResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("data")] public LoginData? Data { get; set; }
            public class LoginData
            {
                [System.Text.Json.Serialization.JsonPropertyName("sid")] public string Sid { get; set; } = "";
                [System.Text.Json.Serialization.JsonPropertyName("did")] public string Did { get; set; } = "";
            }
        }
        #endregion

    }
}