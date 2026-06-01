using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
        private string? _cachedSynoToken; // 👈 nuevo: guardamos el synotoken
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

            // 👇 Cambiado: version=3 en lugar de version=6
            string url = $"{_synologyBaseUrl}/webapi/auth.cgi?api=SYNO.API.Auth&version=3&method=login&account={Uri.EscapeDataString(_username)}&passwd={Uri.EscapeDataString(_password)}&session=FileStation&format=sid&enable_syno_token=yes";

            var rawResponse = await _httpClient.GetStringAsync(url);
            Console.WriteLine($"=== AUTH BODY: {rawResponse} ===");

            var response = System.Text.Json.JsonSerializer.Deserialize<SynologyLoginResponse>(rawResponse);

            if (response != null && response.Success && response.Data != null)
            {
                _cachedSid = response.Data.Sid;
                _cachedSynoToken = response.Data.SynoToken; // 👈 nuevo: guardamos el token
                _sidExpiration = DateTime.UtcNow.AddHours(6);
                return _cachedSid;
            }

            // 👇 Mejorado: incluye el código de error en el mensaje
            throw new Exception($"Error de autenticación Synology. Código: {response?.Error?.Code}. Raw: {rawResponse}");
        }

        private async Task EnsureFolderHierarchyAsync(string fullFolderPath, string sid)
        {
            var pathsToTry = new[]
            {
                fullFolderPath,
                "/team-folders" + fullFolderPath
            };

            foreach (var path in pathsToTry)
            {
                var parts = path.Trim('/').Split('/');
                string parentPath = "/" + string.Join("/", parts[..^1]);
                string folderName = parts[^1];

                string url = $"{_synologyBaseUrl}/webapi/entry.cgi";
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent("SYNO.FileStation.CreateFolder"), "api");
                content.Add(new StringContent("2"), "version");
                content.Add(new StringContent("create"), "method");
                content.Add(new StringContent(parentPath), "folder_path");
                content.Add(new StringContent(folderName), "name");
                content.Add(new StringContent("true"), "force_parent");
                content.Add(new StringContent(sid), "_sid");

                requestMessage.Content = content;

                // 👇 nuevo: agregamos el X-SYNO-TOKEN header
                if (!string.IsNullOrEmpty(_cachedSynoToken))
                    requestMessage.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

                var response = await _httpClient.SendAsync(requestMessage);
                var raw = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"=== CREATE FOLDER === path: [{path}] | parent: [{parentPath}] | name: [{folderName}] | result: {raw}");
            }
        }

        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            string sid = await GetSidAsync();

            await EnsureFolderHierarchyAsync(destinationFolder, sid);

            string url = $"{_synologyBaseUrl}/webapi/entry.cgi";

            // 👇 Cambiado: usamos HttpRequestMessage para poder agregar el header X-SYNO-TOKEN
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

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

            // 👇 nuevo: agregamos el X-SYNO-TOKEN header
            if (!string.IsNullOrEmpty(_cachedSynoToken))
                requestMessage.Headers.Add("X-SYNO-TOKEN", _cachedSynoToken);

            var response = await _httpClient.SendAsync(requestMessage);
            var rawBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"=== SYNOLOGY UPLOAD RESPONSE ===\nStatus: {response.StatusCode}\nFolder: {destinationFolder}\nBody: {rawBody}\n================================");

            var result = System.Text.Json.JsonSerializer.Deserialize<SynologyBaseResponse>(rawBody);
            if (result == null || !result.Success)
                throw new Exception($"Error al subir archivo a Synology. Code: {result?.Error?.Code}");

            return await GenerateSharingLinkAsync($"{destinationFolder}/{fileName}", sid);
        }

        public async Task DeleteBySharingUrlAsync(string sharingUrl)
        {
            if (string.IsNullOrEmpty(sharingUrl)) return;

            string sid = await GetSidAsync();
            string listUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=list&_sid={sid}";

            // 👇 Usamos HttpRequestMessage para agregar el header X-SYNO-TOKEN
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

            // 👇 Usamos HttpRequestMessage para agregar el header X-SYNO-TOKEN
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
                [JsonPropertyName("synotoken")] public string SynoToken { get; set; } = string.Empty; // 👈 nuevo
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