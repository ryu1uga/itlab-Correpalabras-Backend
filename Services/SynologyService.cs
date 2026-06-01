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

        // El path base del team folder en la Drive API
        private const string DriveTeamFolderBase = "/team-folders/CPAPPDEV";

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

            string url = $"{_synologyBaseUrl}/webapi/auth.cgi?api=SYNO.API.Auth&version=3&method=login" +
                         $"&account={Uri.EscapeDataString(_username)}&passwd={Uri.EscapeDataString(_password)}" +
                         $"&session=FileStation&format=sid&enable_syno_token=yes";

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

        // Convierte /CPAPPDEV/img/stories/{id} → /team-folders/CPAPPDEV/img/stories/{id}
        private string ToDriverPath(string fileStationPath)
        {
            // fileStationPath viene como /CPAPPDEV/img/stories/{id}
            // La Drive API necesita /team-folders/CPAPPDEV/img/stories/{id}
            var relative = fileStationPath.TrimStart('/');
            // Quita el primer segmento (CPAPPDEV) y reemplaza con el base de Drive
            var firstSlash = relative.IndexOf('/');
            var subPath = firstSlash >= 0 ? relative.Substring(firstSlash) : "";
            return DriveTeamFolderBase + subPath;
        }

        // Crea una carpeta usando SYNO.SynologyDrive.Files
        private async Task CreateDriveFolderAsync(string drivePath, string sid)
        {
            var lastSlash = drivePath.LastIndexOf('/');
            var parentPath = drivePath.Substring(0, lastSlash);
            var folderName = drivePath.Substring(lastSlash + 1);

            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrEmpty(_cachedSynoToken))
                req.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

            var body = new
            {
                api = "SYNO.SynologyDrive.Files",
                version = 6,
                method = "create_folder",
                path = parentPath,
                name = folderName
            };

            req.Content = new StringContent(
                JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var resp = await _httpClient.SendAsync(req);
            var raw = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"=== DRIVE CREATE FOLDER === parent: [{parentPath}] | name: [{folderName}] | result: {raw}");
            // Ignoramos errores — la carpeta puede ya existir
        }

        // Crea toda la jerarquía de carpetas usando paths de Drive API
        private async Task EnsureDriveFolderHierarchyAsync(string fileStationPath, string sid)
        {
            var drivePath = ToDriverPath(fileStationPath);
            // drivePath = /team-folders/CPAPPDEV/img/stories/{id}
            // Necesitamos crear: /team-folders/CPAPPDEV/img, /team-folders/CPAPPDEV/img/stories, /team-folders/CPAPPDEV/img/stories/{id}
            // El base /team-folders/CPAPPDEV ya existe, así que empezamos desde el siguiente nivel

            var parts = drivePath.Split('/'); // ["", "team-folders", "CPAPPDEV", "img", "stories", "{id}"]
            // Base seguro = primeros 3 partes = /team-folders/CPAPPDEV
            int baseDepth = 3; // índices 0,1,2 = "", "team-folders", "CPAPPDEV"

            for (int i = baseDepth + 1; i <= parts.Length - 1; i++)
            {
                var folderToCreate = string.Join("/", parts[..i]);
                if (!folderToCreate.StartsWith("/")) folderToCreate = "/" + folderToCreate;
                await CreateDriveFolderAsync(folderToCreate, sid);
            }
        }

        // Upload del archivo usando SYNO.SynologyDrive.Files upload
        private async Task<string> UploadViaDriveAsync(IFormFile file, string fileStationFolder, string fileName, string sid)
        {
            var driveFolderPath = ToDriverPath(fileStationFolder);

            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrEmpty(_cachedSynoToken))
                req.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

            using var content = new MultipartFormDataContent("AaB03x");
            content.Add(new StringContent("SYNO.SynologyDrive.Files"), "api");
            content.Add(new StringContent("6"), "version");
            content.Add(new StringContent("upload"), "method");
            content.Add(new StringContent(driveFolderPath), "path");
            content.Add(new StringContent("true"), "overwrite");
            content.Add(new StringContent(sid), "_sid");

            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", fileName);

            req.Content = content;

            var response = await _httpClient.SendAsync(req);
            var rawBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"=== DRIVE UPLOAD RESPONSE ===\nStatus: {response.StatusCode}\nFolder: {driveFolderPath}\nBody: {rawBody}\n================================");

            var result = JsonSerializer.Deserialize<SynologyBaseResponse>(rawBody);
            if (result == null || !result.Success)
                throw new Exception($"Error al subir archivo a Synology Drive. Code: {result?.Error?.Code}");

            // Retorna el path de FileStation para el sharing link
            return $"{fileStationFolder}/{fileName}";
        }

        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            string sid = await GetSidAsync();

            // 1. Crear carpetas con Drive API (usa /team-folders/CPAPPDEV/...)
            await EnsureDriveFolderHierarchyAsync(destinationFolder, sid);

            // 2. Subir archivo con Drive API
            var fileStationFilePath = await UploadViaDriveAsync(file, destinationFolder, fileName, sid);

            // 3. Generar sharing link con FileStation
            return await GenerateSharingLinkAsync(fileStationFilePath, sid);
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
                    string deleteFileUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Delete&version=2&method=delete" +
                                           $"&path=%5B%22{Uri.EscapeDataString(matchedLink.Path)}%22%5D&recursive=true&_sid={sid}";
                    using var deleteFileRequest = new HttpRequestMessage(HttpMethod.Get, deleteFileUrl);
                    if (!string.IsNullOrEmpty(_cachedSynoToken))
                        deleteFileRequest.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);
                    await _httpClient.SendAsync(deleteFileRequest);

                    string cleanLinkUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=delete" +
                                          $"&id=%5B%22{matchedLink.Id}%22%5D&_sid={sid}";
                    using var cleanLinkRequest = new HttpRequestMessage(HttpMethod.Get, cleanLinkUrl);
                    if (!string.IsNullOrEmpty(_cachedSynoToken))
                        cleanLinkRequest.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);
                    await _httpClient.SendAsync(cleanLinkRequest);
                }
            }
        }

        private async Task<string> GenerateSharingLinkAsync(string filePath, string sid)
        {
            string url = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=create" +
                         $"&path=%22{Uri.EscapeDataString(filePath)}%22&_sid={sid}";

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