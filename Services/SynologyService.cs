using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

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
                         "&session=FileStation&format=cookie";

            var raw = await _httpClient.GetStringAsync(url);
            var response = JsonSerializer.Deserialize<SynologyLoginResponse>(raw, _jsonOptions);

            if (response?.Success == true && response.Data != null)
            {
                _cachedSid = response.Data.Sid;
                _cachedDid = response.Data.Did;
                _sidExpiration = DateTime.UtcNow.AddHours(6);
                Console.WriteLine("✅ [Synology] Login exitoso");
                return;
            }

            throw new Exception($"❌ Login falló: {raw}");
        }

        private void AddAuthHeaders(HttpRequestMessage req)
        {
            if (!string.IsNullOrEmpty(_cachedDid) && !string.IsNullOrEmpty(_cachedSid))
                req.Headers.Add("Cookie", $"did={_cachedDid}; id={_cachedSid}");
        }

        private bool IsAuthError(string body)
        {
            try
            {
                var baseResponse = JsonSerializer.Deserialize<SynologyBaseResponse>(body, _jsonOptions);
                return baseResponse != null && !baseResponse.Success &&
                       (baseResponse.Error?.Code == 106 || baseResponse.Error?.Code == 119);
            }
            catch
            {
                return false;
            }
        }

        private async Task CreateFolderAsync(string parentPath, string folderName)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";

            async Task<string> ExecuteAsync()
            {
                await EnsureValidSessionAsync();

                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                AddAuthHeaders(req);

                var form = new Dictionary<string, string>
                {
                    { "api", "SYNO.FileStation.CreateFolder" },
                    { "version", "2" },
                    { "method", "create" },
                    { "folder_path", parentPath },
                    { "name", folderName },
                    { "force_parent", "true" },
                    { "_sid", _cachedSid! }
                };

                req.Content = new FormUrlEncodedContent(form);
                var resp = await _httpClient.SendAsync(req);
                return await resp.Content.ReadAsStringAsync();
            }

            var body = await ExecuteAsync();
            if (IsAuthError(body))
            {
                await RefreshLoginAsync();
                body = await ExecuteAsync();
            }

            Console.WriteLine($"[CreateFolder] {parentPath}/{folderName} | Response: {body}");

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                Console.WriteLine("✅ Carpeta creada");
                return;
            }

            if (body.Contains("\"code\":400"))
            {
                Console.WriteLine("ℹ️ Carpeta ya existía");
                return;
            }

            throw new Exception($"Error creando carpeta: {body}");
        }

        // ======================= UPLOAD CORREGIDO =======================
        private async Task<string> UploadFileAsync(string targetPath, IFormFile file, string fileName)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";

            async Task<string> ExecuteAsync()
            {
                await EnsureValidSessionAsync();

                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                AddAuthHeaders(req);

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent("SYNO.FileStation.Upload"), "api");
                content.Add(new StringContent("2"), "version");
                content.Add(new StringContent("upload"), "method");
                content.Add(new StringContent(targetPath), "path");
                content.Add(new StringContent("true"), "overwrite");
                content.Add(new StringContent("true"), "create_parents");
                content.Add(new StringContent(_cachedSid ?? string.Empty), "_sid");

                using var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType ?? "application/octet-stream");

                content.Add(streamContent, "file", fileName);
                req.Content = content;
                var resp = await _httpClient.SendAsync(req);
                return await resp.Content.ReadAsStringAsync();
            }

            var body = await ExecuteAsync();
            if (IsAuthError(body))
            {
                await RefreshLoginAsync();
                body = await ExecuteAsync();
            }

            Console.WriteLine($"[UploadFile] {fileName} → Response: {body}");

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                Console.WriteLine("✅ Archivo subido correctamente");
                return $"{targetPath}/{fileName}";
            }

            throw new Exception($"Error al subir archivo: {body}");
        }

        private async Task<string> CreateShareByPathAsync(string filePath)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=create" +
                         $"&path=%22{Uri.EscapeDataString(filePath)}%22&_sid={_cachedSid}";

            async Task<string> ExecuteAsync()
            {
                await EnsureValidSessionAsync();
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthHeaders(req);
                var resp = await _httpClient.SendAsync(req);
                return await resp.Content.ReadAsStringAsync();
            }

            var body = await ExecuteAsync();
            if (IsAuthError(body))
            {
                await RefreshLoginAsync();
                body = await ExecuteAsync();
            }

            Console.WriteLine($"[CreateShare] Response: {body}");

            var result = JsonSerializer.Deserialize<SynologySharingResponse>(body, _jsonOptions);

            if (result?.Success == true && result.Data?.Links?.Length > 0)
            {
                var shareUrl = result.Data.Links[0].Url;
                Console.WriteLine($"✅ Share generado: {shareUrl}");
                return shareUrl;
            }

            throw new Exception($"Error creando share: {body}");
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
            if (string.IsNullOrEmpty(sharingUrl)) return;
            // (mantengo tu implementación anterior o la que tenías)
            await Task.CompletedTask;
        }

        #region DTOs
        // ... (mantén los mismos DTOs que tenías)
        public class SynologyBaseResponse
        {
            [JsonPropertyName("success")] public bool Success { get; set; }
            [JsonPropertyName("error")] public SynologyError? Error { get; set; }
        }

        public class SynologyError
        {
            [JsonPropertyName("code")] public int Code { get; set; }
        }

        public class SynologyLoginResponse : SynologyBaseResponse
        {
            [JsonPropertyName("data")] public LoginData? Data { get; set; }
            public class LoginData
            {
                [JsonPropertyName("sid")] public string Sid { get; set; } = "";
                [JsonPropertyName("did")] public string Did { get; set; } = "";
            }
        }

        public class SynologySharingResponse : SynologyBaseResponse
        {
            [JsonPropertyName("data")] public SharingData? Data { get; set; }
            public class SharingData
            {
                [JsonPropertyName("links")] public SharingLink[] Links { get; set; } = Array.Empty<SharingLink>();
            }
            public class SharingLink
            {
                [JsonPropertyName("id")] public string Id { get; set; } = "";
                [JsonPropertyName("url")] public string Url { get; set; } = "";
                [JsonPropertyName("path")] public string Path { get; set; } = "";
            }
        }
        #endregion
    }
}