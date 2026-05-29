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

                string url = $"{_synologyBaseUrl}/webapi/auth.cgi?api=SYNO.API.Auth&version=3&method=login&account={Uri.EscapeDataString(_username)}&passwd={Uri.EscapeDataString(_password)}&session=FileStation&format=sid";

                // 👇 AGREGA ESTO
                var rawResponse = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"=== SYNOLOGY AUTH RESPONSE ===");
                Console.WriteLine($"URL: {url}");
                Console.WriteLine($"Body: {rawResponse}");
                Console.WriteLine($"==============================");

                var response = System.Text.Json.JsonSerializer.Deserialize<SynologyLoginResponse>(rawResponse);
                // ...
            }

            public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
            {
                string sid = await GetSidAsync();
                string url = $"{_synologyBaseUrl}/webapi/entry.cgi";

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

                var response = await _httpClient.PostAsync(url, content);
                
                // 👇 AGREGA ESTO TEMPORALMENTE
                var rawBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"=== SYNOLOGY RAW RESPONSE ===");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"URL called: {url}");
                Console.WriteLine($"Folder path sent: {destinationFolder}");
                Console.WriteLine($"Body: {rawBody}");
                Console.WriteLine($"=============================");

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
                var listResponse = await _httpClient.GetFromJsonAsync<SynologySharingResponse>(listUrl);
                
                if (listResponse != null && listResponse.Success && listResponse.Data?.Links != null)
                {
                    var matchedLink = listResponse.Data.Links.FirstOrDefault(l => l.Url == sharingUrl);
                    if (matchedLink != null)
                    {
                        string deleteFileUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Delete&version=2&method=delete&path=%5B%22{Uri.EscapeDataString(matchedLink.Path)}%22%5D&recursive=true&_sid={sid}";
                        await _httpClient.GetFromJsonAsync<SynologyBaseResponse>(deleteFileUrl);

                        string cleanLinkUrl = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=delete&id=%5B%22{matchedLink.Id}%22%5D&_sid={sid}";
                        await _httpClient.GetFromJsonAsync<SynologyBaseResponse>(cleanLinkUrl);
                    }
                }
            }

            private async Task<string> GenerateSharingLinkAsync(string filePath, string sid)
            {
                string url = $"{_synologyBaseUrl}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=create&path=%22{Uri.EscapeDataString(filePath)}%22&_sid={sid}";

                var response = await _httpClient.GetFromJsonAsync<SynologySharingResponse>(url);
                if (response != null && response.Success && response.Data?.Links?.Length > 0)
                {
                    return response.Data.Links[0].Url;
                }
                throw new Exception("No se pudo generar el enlace compartido en Synology.");
            }

            #region DTOs Internos
            public class SynologyBaseResponse { [JsonPropertyName("success")] public bool Success { get; set; } [JsonPropertyName("error")] public SynologyError? Error { get; set; } }
            public class SynologyError { [JsonPropertyName("code")] public int Code { get; set; } }
            public class SynologyLoginResponse : SynologyBaseResponse { [JsonPropertyName("data")] public LoginData? Data { get; set; } public class LoginData { [JsonPropertyName("sid")] public string Sid { get; set; } = string.Empty; } }
            public class SynologySharingResponse : SynologyBaseResponse { [JsonPropertyName("data")] public SharingData? Data { get; set; } public class SharingData { [JsonPropertyName("links")] public SharingLink[] Links { get; set; } = Array.Empty<SharingLink>(); } public class SharingLink { [JsonPropertyName("id")] public string Id { get; set; } = string.Empty; [JsonPropertyName("url")] public string Url { get; set; } = string.Empty; [JsonPropertyName("path")] public string Path { get; set; } = string.Empty; } }
            #endregion
        }
    }