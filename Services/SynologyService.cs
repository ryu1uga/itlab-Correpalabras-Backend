using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
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

        // file_id conocido de /team-folders/CPAPPDEV/img/stories
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

        private async Task<string> GetSidAsync()
        {
            if (!string.IsNullOrEmpty(_cachedSid) && DateTime.UtcNow < _sidExpiration)
                return _cachedSid;

            // Usamos format=cookie para que Synology acepte cookies en requests posteriores
            string url =
                $"{_synologyBaseUrl}/webapi/auth.cgi" +
                "?api=SYNO.API.Auth" +
                "&version=3" +
                "&method=login" +
                $"&account={Uri.EscapeDataString(_username)}" +
                $"&passwd={Uri.EscapeDataString(_password)}" +
                "&session=FileStation" +
                "&format=cookie";

            var rawResponse = await _httpClient.GetStringAsync(url);
            Console.WriteLine($"=== AUTH BODY: {rawResponse} ===");

            var response = JsonSerializer.Deserialize<SynologyLoginResponse>(rawResponse);

            if (response != null && response.Success && response.Data != null)
            {
                _cachedSid = response.Data.Sid;
                _cachedDid = response.Data.Did;
                _sidExpiration = DateTime.UtcNow.AddHours(6);
                return _cachedSid;
            }

            throw new Exception($"Error de autenticación Synology. Código: {response?.Error?.Code}. Raw: {rawResponse}");
        }

        // Agrega headers de autenticación: Cookie (did + id/sid) y X-SYNO-TOKEN
        private void AddAuthHeaders(HttpRequestMessage req)
        {
            if (!string.IsNullOrEmpty(_cachedDid) &&
                !string.IsNullOrEmpty(_cachedSid))
            {
                req.Headers.Add(
                    "Cookie",
                    $"did={_cachedDid}; id={_cachedSid}");
            }
        }

        private bool IsSessionExpired(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);

                if (!doc.RootElement.TryGetProperty("success", out var success))
                    return false;

                if (success.GetBoolean())
                    return false;

                if (!doc.RootElement.TryGetProperty("error", out var error))
                    return false;

                var code = error.GetProperty("code").GetInt32();

                return code == 106 || code == 119;
            }
            catch
            {
                return false;
            }
        }

        private async Task RefreshLoginAsync()
        {
            _cachedDid = null;
            _cachedSid = null;

            await GetSidAsync();
        }

        // Crea una carpeta usando file_id del parent con autenticación por cookies
        private async Task<string> CreateFolderByIdAsync(string parentFileId, string folderName)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            AddAuthHeaders(req);

            var formData = new Dictionary<string, string>
            {
                { "api", "SYNO.SynologyDrive.Files" },
                { "version", "6" },
                { "method", "create_folder" },
                { "file_id", parentFileId },
                { "name", folderName }
            };

            req.Content = new FormUrlEncodedContent(formData);

            var resp = await _httpClient.SendAsync(req);
            var raw = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"=== CREATE FOLDER BY ID === parent_id: [{parentFileId}] | name: [{folderName}] | result: {raw}");

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("file_id", out var fid))
                    return fid.GetString() ?? "";
            }

            // Si falló, puede ser que ya existe — buscarla por nombre
            return await GetFileIdByNameAsync(parentFileId, folderName);
        }

        // Lista el contenido de una carpeta por su path (Drive acepta path para list)
        // y busca el file_id de un item por nombre
        private async Task<string> GetFileIdByNameAsync(string parentFileId, string name)
        {
            // Usamos path conocido según el parentFileId
            string parentPath = parentFileId == StoriesFileId
                ? "/team-folders/CPAPPDEV/img/stories"
                : "/team-folders/CPAPPDEV";

            string url = $"{_synologyBaseUrl}/webapi/entry.cgi" +
                         $"?api=SYNO.SynologyDrive.Files&version=6&method=list" +
                         $"&path={Uri.EscapeDataString(parentPath)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeaders(req);

            var resp = await _httpClient.SendAsync(req);
            var raw = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"=== LIST FOR FILE_ID === path: [{parentPath}] | looking for: [{name}] | result: {raw.Substring(0, Math.Min(500, raw.Length))}... ===");

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var itemName = item.TryGetProperty("name", out var n) ? n.GetString() : null;
                    if (itemName == name && item.TryGetProperty("file_id", out var fid))
                        return fid.GetString() ?? "";
                }
            }

            throw new Exception($"No se encontró '{name}' en '{parentPath}'");
        }

        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            string sid = await GetSidAsync();

            // destinationFolder = /CPAPPDEV/img/stories/{guid}
            var storyGuid = destinationFolder.TrimEnd('/').Split('/').Last();
            Console.WriteLine($"=== STORY GUID: {storyGuid} ===");

            // 1. Crear carpeta del story dentro de stories (por file_id)
            var storyFolderFileId = await CreateFolderByIdAsync(StoriesFileId, storyGuid);
            Console.WriteLine($"=== STORY FOLDER FILE_ID: {storyFolderFileId} ===");

            // 2. Subir archivo con cookies y file_id de la carpeta destino
            string uploadUrl = $"{_synologyBaseUrl}/webapi/entry.cgi";
            using var uploadReq = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            AddAuthHeaders(uploadReq);

            using var content = new MultipartFormDataContent("AaB03x");
            content.Add(new StringContent("SYNO.SynologyDrive.Files"), "api");
            content.Add(new StringContent("6"), "version");
            content.Add(new StringContent("upload"), "method");
            content.Add(new StringContent(storyFolderFileId), "file_id");
            content.Add(new StringContent("true"), "overwrite");

            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", fileName);
            uploadReq.Content = content;

            var uploadResp = await _httpClient.SendAsync(uploadReq);
            var uploadBody = await uploadResp.Content.ReadAsStringAsync();
            Console.WriteLine($"=== DRIVE UPLOAD RESPONSE ===\nStatus: {uploadResp.StatusCode}\nBody: {uploadBody}\n================================");

            var uploadResult = JsonSerializer.Deserialize<SynologyBaseResponse>(uploadBody);
            if (uploadResult == null || !uploadResult.Success)
                throw new Exception($"Error al subir archivo a Synology Drive. Code: {uploadResult?.Error?.Code}");

            // 3. Generar sharing link con FileStation
            return await GenerateSharingLinkAsync($"{destinationFolder}/{fileName}", sid);
        }

        public async Task DeleteBySharingUrlAsync(string sharingUrl)
        {
            if (string.IsNullOrEmpty(sharingUrl)) return;

            string sid = await GetSidAsync();
            string listUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=list&_sid={sid}";

            using var listRequest = new HttpRequestMessage(HttpMethod.Get, listUrl);
            AddAuthHeaders(listRequest);

            var listHttpResponse = await _httpClient.SendAsync(listRequest);
            var listResponse = await listHttpResponse.Content.ReadFromJsonAsync<SynologySharingResponse>();

            if (listResponse != null && listResponse.Success && listResponse.Data?.Links != null)
            {
                var matchedLink = listResponse.Data.Links.FirstOrDefault(l => l.Url == sharingUrl);
                if (matchedLink != null)
                {
                    string deleteFileUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Delete&version=2&method=delete" +
                                           $"&path=%5B%22{Uri.EscapeDataString(matchedLink.Path)}%22%5D&recursive=true&_sid={sid}";
                    using var deleteFileRequest = new HttpRequestMessage(HttpMethod.Get, deleteFileUrl);
                    AddAuthHeaders(deleteFileRequest);
                    await _httpClient.SendAsync(deleteFileRequest);

                    string cleanLinkUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=delete" +
                                          $"&id=%5B%22{matchedLink.Id}%22%5D&_sid={sid}";
                    using var cleanLinkRequest = new HttpRequestMessage(HttpMethod.Get, cleanLinkUrl);
                    AddAuthHeaders(cleanLinkRequest);
                    await _httpClient.SendAsync(cleanLinkRequest);
                }
            }
        }

        private async Task<string> GenerateSharingLinkAsync(string filePath, string sid)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=create" +
                         $"&path=%22{Uri.EscapeDataString(filePath)}%22&_sid={sid}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeaders(req);

            var httpResponse = await _httpClient.SendAsync(req);
            var response = await httpResponse.Content.ReadFromJsonAsync<SynologySharingResponse>();

            if (response != null && response.Success && response.Data?.Links?.Length > 0)
                return response.Data.Links[0].Url;

            throw new Exception("No se pudo generar el enlace compartido en Synology.");
        }

        #region DTOs Internos
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
                [JsonPropertyName("sid")]
                public string Sid { get; set; } = string.Empty;

                [JsonPropertyName("did")]
                public string Did { get; set; } = string.Empty;
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
                [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
                [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
                [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
            }
        }
        #endregion
    }
}