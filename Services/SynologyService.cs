using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
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

        private const string StoriesFileId = "953604116253293456";

        private string? _cachedSid;
        private string? _cachedDid;
        private DateTime _sidExpiration = DateTime.MinValue;

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

            await RefreshLoginAsync();
        }

        private async Task RefreshLoginAsync()
        {
            _cachedSid = null;
            _cachedDid = null;
            _sidExpiration = DateTime.MinValue;

            string url = $"{_synologyBaseUrl}/webapi/auth.cgi" +
                         "?api=SYNO.API.Auth" +
                         "&version=3" +
                         "&method=login" +
                         $"&account={Uri.EscapeDataString(_username)}" +
                         $"&passwd={Uri.EscapeDataString(_password)}" +
                         "&session=FileStation" +
                         "&format=cookie";

            var rawResponse = await _httpClient.GetStringAsync(url);
            var response = JsonSerializer.Deserialize<SynologyLoginResponse>(rawResponse);

            if (response?.Success == true && response.Data != null)
            {
                _cachedSid = response.Data.Sid;
                _cachedDid = response.Data.Did;
                _sidExpiration = DateTime.UtcNow.AddHours(6);
                return;
            }

            throw new Exception($"Error de login Synology. Código: {response?.Error?.Code}");
        }

        private void AddAuthHeaders(HttpRequestMessage req)
        {
            if (!string.IsNullOrEmpty(_cachedDid) && !string.IsNullOrEmpty(_cachedSid))
            {
                req.Headers.Add("Cookie", $"did={_cachedDid}; id={_cachedSid}");
            }
        }

        private async Task<string> CreateFolderByIdAsync(string parentFileId, string folderName)
        {
            await EnsureValidSessionAsync();

            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            AddAuthHeaders(req);

            var form = new Dictionary<string, string>
            {
                { "api", "SYNO.SynologyDrive.Files" },
                { "version", "6" },
                { "method", "create_folder" },
                { "file_id", parentFileId },
                { "name", folderName },
                { "force_parent", "true" }
            };

            req.Content = new FormUrlEncodedContent(form);

            var response = await _httpClient.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("file_id", out var fid))
                {
                    return fid.GetString() ?? "";
                }
            }

            // Si ya existe → buscar por nombre
            return await GetFileIdByNameAsync(parentFileId, folderName);
        }

        private async Task<string> GetFileIdByNameAsync(string parentFileId, string name)
        {
            await EnsureValidSessionAsync();

            string path = parentFileId == StoriesFileId 
                ? "/team-folders/CPAPPDEV/img/stories" 
                : "/team-folders/CPAPPDEV";

            string url = $"{_synologyBaseUrl}/webapi/entry.cgi" +
                         $"?api=SYNO.SynologyDrive.Files&version=6&method=list" +
                         $"&path={Uri.EscapeDataString(path)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeaders(req);

            var resp = await _httpClient.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("name", out var n) && n.GetString() == name &&
                        item.TryGetProperty("file_id", out var fid))
                    {
                        return fid.GetString() ?? "";
                    }
                }
            }

            throw new Exception($"No se encontró la carpeta '{name}'");
        }

        private async Task<string> UploadFileAsync(string targetFolderFileId, IFormFile file, string fileName)
        {
            await EnsureValidSessionAsync();

            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            AddAuthHeaders(req);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("SYNO.SynologyDrive.Files"), "api");
            content.Add(new StringContent("6"), "version");
            content.Add(new StringContent("upload"), "method");
            content.Add(new StringContent(targetFolderFileId), "file_id");
            content.Add(new StringContent("true"), "overwrite");
            content.Add(new StringContent("true"), "create_parents");

            using var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType ?? "application/octet-stream");

            content.Add(streamContent, "file", fileName);

            req.Content = content;

            var response = await _httpClient.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("file_id", out var fid))
                {
                    return fid.GetString() ?? "";
                }
            }

            throw new Exception($"Error al subir archivo: {body}");
        }

        private async Task<string> CreateShareByFileIdAsync(string fileId)
        {
            await EnsureValidSessionAsync();

            string url = $"{_synologyBaseUrl}/webapi/entry.cgi" +
                         "?api=SYNO.FileStation.Sharing" +
                         "&version=3" +
                         "&method=create" +
                         $"&path=%22{fileId}%22" +  // Usar file_id es más confiable
                         "&_sid=" + _cachedSid;

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeaders(req);

            var response = await _httpClient.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<SynologySharingResponse>(body);

            if (result?.Success == true && result.Data?.Links?.Length > 0)
                return result.Data.Links[0].Url;

            throw new Exception("No se pudo generar el enlace compartido");
        }

        // ======================= MÉTODO PRINCIPAL =======================
        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            var storyGuid = destinationFolder.TrimEnd('/').Split('/').Last();

            // 1. Crear/obtener carpeta del story
            var storyFolderFileId = await CreateFolderByIdAsync(StoriesFileId, storyGuid);

            // 2. Subir archivo
            var uploadedFileId = await UploadFileAsync(storyFolderFileId, file, fileName);

            // 3. Generar enlace compartido
            return await CreateShareByFileIdAsync(uploadedFileId);
        }

        public async Task DeleteBySharingUrlAsync(string sharingUrl)
        {
            if (string.IsNullOrEmpty(sharingUrl)) return;

            await EnsureValidSessionAsync();

            // Listar shares
            string listUrl = $"{_synologyBaseUrl}/webapi/entry.cgi" +
                             "?api=SYNO.FileStation.Sharing" +
                             "&version=3" +
                             "&method=list" +
                             "&_sid=" + _cachedSid;

            using var listReq = new HttpRequestMessage(HttpMethod.Get, listUrl);
            AddAuthHeaders(listReq);

            var listResp = await _httpClient.SendAsync(listReq);
            var sharingResponse = await listResp.Content.ReadFromJsonAsync<SynologySharingResponse>();

            var link = sharingResponse?.Data?.Links?.FirstOrDefault(l => l.Url == sharingUrl);
            if (link == null) return;

            // Eliminar archivo
            string deleteUrl = $"{_synologyBaseUrl}/webapi/entry.cgi" +
                               "?api=SYNO.FileStation.Delete" +
                               "&version=2" +
                               "&method=delete" +
                               $"&path=%5B%22{Uri.EscapeDataString(link.Path)}%22%5D" +
                               "&recursive=true" +
                               "&_sid=" + _cachedSid;

            await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, deleteUrl) { Headers = { /* auth */ } });

            // Eliminar share
            string cleanShareUrl = $"{_synologyBaseUrl}/webapi/entry.cgi" +
                                   "?api=SYNO.FileStation.Sharing" +
                                   "&version=3" +
                                   "&method=delete" +
                                   $"&id=%5B%22{link.Id}%22%5D" +
                                   "&_sid=" + _cachedSid;

            await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, cleanShareUrl));
        }

        #region DTOs
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