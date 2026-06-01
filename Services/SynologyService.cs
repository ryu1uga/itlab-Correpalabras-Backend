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

        private string? _cachedSid;
        private string? _cachedSynoToken;
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

            string url = $"{_synologyBaseUrl}/webapi/auth.cgi?api=SYNO.API.Auth&version=3&method=login&account={Uri.EscapeDataString(_username)}&passwd={Uri.EscapeDataString(_password)}&session=FileStation&format=sid&enable_syno_token=yes";

            var rawResponse = await _httpClient.GetStringAsync(url);
            Console.WriteLine($"=== AUTH BODY: {rawResponse} ===");

            var response = JsonSerializer.Deserialize<SynologyLoginResponse>(rawResponse);

            if (response != null && response.Success && response.Data != null)
            {
                _cachedSid = response.Data.Sid;
                _cachedSynoToken = response.Data.SynoToken;
                _sidExpiration = DateTime.UtcNow.AddHours(6);
                return _cachedSid;
            }

            throw new Exception($"Error de autenticación Synology. Código: {response?.Error?.Code}. Raw: {rawResponse}");
        }

        // Obtiene el display_path del Team Folder CPAPPDEV usando SYNO.SynologyDrive.Files
        private async Task<string> GetTeamFolderDisplayPathAsync(string sid)
        {
            // Listamos los team folders disponibles
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.SynologyDrive.TeamFolders&version=1&method=list&_sid={sid}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(_cachedSynoToken))
                req.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

            var resp = await _httpClient.SendAsync(req);
            var raw = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"=== TEAM FOLDERS: {raw} ===");

            // Parseamos para encontrar CPAPPDEV
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("list", out var list))
            {
                foreach (var item in list.EnumerateArray())
                {
                    var name = item.TryGetProperty("name", out var n) ? n.GetString() : null;
                    if (name == "CPAPPDEV")
                    {
                        // display_path es como "/team-folders/CPAPPDEV" o similar
                        if (item.TryGetProperty("display_path", out var dp))
                            return dp.GetString() ?? "/CPAPPDEV";
                        if (item.TryGetProperty("path", out var p))
                            return p.GetString() ?? "/CPAPPDEV";
                    }
                }
            }

            // Fallback
            return "/CPAPPDEV";
        }

        // Crea carpeta usando SYNO.SynologyDrive.Files
        private async Task CreateDriveFolderAsync(string parentPath, string folderName, string sid)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrEmpty(_cachedSynoToken))
                req.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

            // SYNO.SynologyDrive.Files usa JSON body
            var body = new
            {
                api = "SYNO.SynologyDrive.Files",
                version = 6,
                method = "create_folder",
                path = parentPath,
                name = folderName,
                force_parent = true
            };

            req.Content = new StringContent(
                JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var resp = await _httpClient.SendAsync(req);
            var raw = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"=== DRIVE CREATE FOLDER === parent: [{parentPath}] | name: [{folderName}] | result: {raw}");
        }

        private async Task EnsureDriveFolderHierarchyAsync(string fullFolderPath, string sid)
        {
            var parts = fullFolderPath.Trim('/').Split('/');

            for (int i = 1; i < parts.Length; i++)
            {
                string parentPath = "/" + string.Join("/", parts[..i]);
                string folderName = parts[i];
                await CreateDriveFolderAsync(parentPath, folderName, sid);
            }
        }

        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            string sid = await GetSidAsync();

            // 👇 TEMPORAL: listar por file_id
            var filesUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.SynologyDrive.Files&version=6&method=list&file_id=953441541020491937&_sid={sid}";
            using var filesReq = new HttpRequestMessage(HttpMethod.Get, filesUrl);
            if (!string.IsNullOrEmpty(_cachedSynoToken)) filesReq.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);
            var filesResp = await _httpClient.SendAsync(filesReq);
            Console.WriteLine($"=== DRIVE FILES LIST: {await filesResp.Content.ReadAsStringAsync()} ===");

            // 👇 TEMPORAL: crear carpeta por file_id
            var createUrl = $"{_synologyBaseUrl}/webapi/entry.cgi";
            using var createReq = new HttpRequestMessage(HttpMethod.Post, createUrl);
            if (!string.IsNullOrEmpty(_cachedSynoToken)) createReq.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);
            var createBody = new { api = "SYNO.SynologyDrive.Files", version = 6, method = "create_folder", file_id = "953441541020491937", name = "img-test" };
            createReq.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(createBody), System.Text.Encoding.UTF8, "application/json");
            var createResp = await _httpClient.SendAsync(createReq);
            Console.WriteLine($"=== DRIVE CREATE BY ID: {await createResp.Content.ReadAsStringAsync()} ===");

            // Crear la jerarquía de carpetas con la API de Drive
            await EnsureDriveFolderHierarchyAsync(destinationFolder, sid);

            // Upload usando SYNO.FileStation.Upload (con el SID ya autenticado, el upload puede funcionar
            // ahora que la carpeta existe vía Drive API)
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrEmpty(_cachedSynoToken))
                requestMessage.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

            using var content = new MultipartFormDataContent("AaB03x");
            content.Add(new StringContent("SYNO.FileStation.Upload"), "api");
            content.Add(new StringContent("2"), "version");
            content.Add(new StringContent("upload"), "method");
            content.Add(new StringContent(destinationFolder), "path");
            content.Add(new StringContent("true"), "create_parents");
            content.Add(new StringContent("true"), "overwrite");
            content.Add(new StringContent(sid), "_sid");

            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", fileName);

            requestMessage.Content = content;

            var response = await _httpClient.SendAsync(requestMessage);
            var rawBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"=== SYNOLOGY UPLOAD RESPONSE ===\nStatus: {response.StatusCode}\nFolder: {destinationFolder}\nBody: {rawBody}\n================================");

            var result = JsonSerializer.Deserialize<SynologyBaseResponse>(rawBody);
            if (result == null || !result.Success)
                throw new Exception($"Error al subir archivo a Synology. Code: {result?.Error?.Code}");

            return await GenerateSharingLinkAsync($"{destinationFolder}/{fileName}", sid);
        }

        public async Task DeleteBySharingUrlAsync(string sharingUrl)
        {
            if (string.IsNullOrEmpty(sharingUrl)) return;

            string sid = await GetSidAsync();
            string listUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=list&_sid={sid}";

            using var listRequest = new HttpRequestMessage(HttpMethod.Get, listUrl);
            if (!string.IsNullOrEmpty(_cachedSynoToken))
                listRequest.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

            var listHttpResponse = await _httpClient.SendAsync(listRequest);
            var listResponse = await listHttpResponse.Content.ReadFromJsonAsync<SynologySharingResponse>();

            if (listResponse != null && listResponse.Success && listResponse.Data?.Links != null)
            {
                var matchedLink = listResponse.Data.Links.FirstOrDefault(l => l.Url == sharingUrl);
                if (matchedLink != null)
                {
                    string deleteFileUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Delete&version=2&method=delete&path=%5B%22{Uri.EscapeDataString(matchedLink.Path)}%22%5D&recursive=true&_sid={sid}";
                    using var deleteFileRequest = new HttpRequestMessage(HttpMethod.Get, deleteFileUrl);
                    if (!string.IsNullOrEmpty(_cachedSynoToken))
                        deleteFileRequest.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);
                    await _httpClient.SendAsync(deleteFileRequest);

                    string cleanLinkUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=delete&id=%5B%22{matchedLink.Id}%22%5D&_sid={sid}";
                    using var cleanLinkRequest = new HttpRequestMessage(HttpMethod.Get, cleanLinkUrl);
                    if (!string.IsNullOrEmpty(_cachedSynoToken))
                        cleanLinkRequest.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);
                    await _httpClient.SendAsync(cleanLinkRequest);
                }
            }
        }

        private async Task<string> GenerateSharingLinkAsync(string filePath, string sid)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=create&path=%22{Uri.EscapeDataString(filePath)}%22&_sid={sid}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(_cachedSynoToken))
                requestMessage.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

            var httpResponse = await _httpClient.SendAsync(requestMessage);
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
                [JsonPropertyName("sid")] public string Sid { get; set; } = string.Empty;
                [JsonPropertyName("synotoken")] public string SynoToken { get; set; } = string.Empty;
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