using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly string _synologyBaseUrl;
        private readonly string _username;
        private readonly string _password;

        private const string StoriesPath = "/CPAPPDEV/img/stories";

        private string? _cachedSid;
        private string? _cachedDid;
        private DateTime _sidExpiration = DateTime.MinValue;
        private readonly SemaphoreSlim _loginSemaphore = new(1, 1);

        public SynologyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

            string url = $"{_synologyBaseUrl}/webapi/auth.cgi?api=SYNO.API.Auth&version=3&method=login" +
                         $"&account={Uri.EscapeDataString(_username)}" +
                         $"&passwd={Uri.EscapeDataString(_password)}" +
                         "&session=FileStation&format=xml";

            var raw = await _httpClient.GetStringAsync(url);
            Console.WriteLine($"[Login Response Raw] {raw}");

            try
            {
                var xml = LoadXml(raw);
                var success = GetXmlValue(xml, "/response/success");
                if (string.Equals(success, "true", StringComparison.OrdinalIgnoreCase))
                {
                    _cachedSid = GetXmlValue(xml, "/response/data/sid");
                    _cachedDid = GetXmlValue(xml, "/response/data/did");
                    _sidExpiration = DateTime.UtcNow.AddHours(6);
                    Console.WriteLine("✅ [Synology] Login exitoso");
                    return;
                }

                throw new Exception($"❌ Login falló (XML): {xml.OuterXml}");
            }
            catch (Exception xmlEx)
            {
                Console.WriteLine($"[Login] XML parsing failed, trying JSON: {xmlEx.Message}");
                
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var response = System.Text.Json.JsonSerializer.Deserialize<SynologyLoginResponse>(raw, options);
                    
                    if (response?.Success == true && response.Data != null)
                    {
                        _cachedSid = response.Data.Sid;
                        _cachedDid = response.Data.Did;
                        _sidExpiration = DateTime.UtcNow.AddHours(6);
                        Console.WriteLine("✅ [Synology] Login exitoso (JSON)");
                        return;
                    }

                    throw new Exception($"❌ Login falló (JSON): {raw}");
                }
                catch (Exception jsonEx)
                {
                    throw new Exception($"❌ Login falló (both XML and JSON): {raw}", jsonEx);
                }
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

        private async Task<XmlDocument> SendXmlRequestAsync(string url, HttpContent? content = null)
        {
            using var request = new HttpRequestMessage(content == null ? HttpMethod.Get : HttpMethod.Post, url);
            if (content != null)
                request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var raw = await response.Content.ReadAsStringAsync();

            try
            {
                return LoadXml(raw);
            }
            catch (XmlException ex)
            {
                Console.WriteLine($"[SendXmlRequestAsync] Response not XML: {raw}");
                throw new Exception($"Respuesta inválida de Synology: {raw}", ex);
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

        // ======================= UPLOAD CORREGIDO =======================
        private async Task<string> UploadFileAsync(string targetPath, IFormFile file, string fileName)
        {
            async Task<XmlDocument> ExecuteAsync()
            {
                await EnsureValidSessionAsync();

                var uploadUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Upload&version=2&method=upload" +
                                $"&path={Uri.EscapeDataString(targetPath)}" +
                                $"&overwrite=true&create_parents=true" +
                                $"&format=xml";

                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType ?? "application/octet-stream");
                content.Add(streamContent, "file", fileName);
                content.Add(new StringContent(fileName), "filename");

                await LogMultipartContentAsync(content, fileName, file.Length);

                return await SendXmlRequestAsync(uploadUrl, content);
            }

            var xml = await ExecuteAsync();
            if (IsAuthError(xml.OuterXml))
            {
                await RefreshLoginAsync();
                xml = await ExecuteAsync();
            }

            Console.WriteLine($"[UploadFile] {fileName} → Response: {xml.OuterXml}");

            if (GetXmlValue(xml, "/response/success") == "true")
            {
                Console.WriteLine("✅ Archivo subido correctamente");
                return $"{targetPath}/{fileName}";
            }

            throw new Exception($"Error al subir archivo: {xml.OuterXml}");
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

            var storyGuid = destinationFolder.TrimEnd('/').Split('/').Last();
            var fullFolderPath = $"{StoriesPath}/{storyGuid}";

            await CreateFolderAsync(StoriesPath, storyGuid);
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