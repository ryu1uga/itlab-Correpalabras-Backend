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

        // Caché de sesión para no pedir login en cada subida
        private string? _cachedSid;
        private DateTime _sidExpiration = DateTime.MinValue;

        public SynologyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Se leen las variables de entorno. Nota: asegúrate de que SYNOLOGY_BASE_URL 
            // incluya el sufijo "/drive" si tu red interna requiere ese ruteo.
            _synologyBaseUrl = Environment.GetEnvironmentVariable("SYNOLOGY_BASE_URL") ?? "http://localhost:5000";
            _username = Environment.GetEnvironmentVariable("SYNOLOGY_USERNAME") ?? "";
            _password = Environment.GetEnvironmentVariable("SYNOLOGY_PASSWORD") ?? "";
        }

        /// <summary>
        /// Obtiene y mantiene activo el token de sesión (SID)
        /// </summary>
        private async Task<string> GetSidAsync()
        {
            if (!string.IsNullOrEmpty(_cachedSid) && DateTime.UtcNow < _sidExpiration)
                return _cachedSid;

            string url = $"{_synologyBaseUrl.TrimEnd('/')}/webapi/auth.cgi?api=SYNO.API.Auth&version=3&method=login" +
                         $"&account={Uri.EscapeDataString(_username)}&passwd={Uri.EscapeDataString(_password)}" +
                         $"&session=FileStation&format=sid";

            var rawResponse = await _httpClient.GetStringAsync(url);
            var response = JsonSerializer.Deserialize<SynologyLoginResponse>(rawResponse);

            if (response != null && response.Success && response.Data != null)
            {
                _cachedSid = response.Data.Sid;
                // El SID suele expirar en Synology, lo guardamos por unas horas
                _sidExpiration = DateTime.UtcNow.AddHours(6); 
                return _cachedSid;
            }

            throw new Exception($"Error de autenticación Synology. Código: {response?.Error?.Code}. Raw: {rawResponse}");
        }

        /// <summary>
        /// Sube la imagen y retorna el link compartido de File Station
        /// </summary>
        public async Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("El archivo está vacío o es nulo.");

            string sid = await GetSidAsync();
            string endpoint = $"{_synologyBaseUrl.TrimEnd('/')}/webapi/entry.cgi";

            // 1. SUBIR ARCHIVO 
            using var uploadReq = new HttpRequestMessage(HttpMethod.Post, endpoint);
            using var content = new MultipartFormDataContent();

            // Parámetros obligatorios escapados con comillas dobles para evitar el Error 101
            content.Add(new StringContent("SYNO.FileStation.Upload"), "\"api\"");
            content.Add(new StringContent("2"), "\"version\"");
            content.Add(new StringContent("upload"), "\"method\"");
            content.Add(new StringContent(sid), "\"_sid\"");
            content.Add(new StringContent("true"), "\"overwrite\"");
            content.Add(new StringContent("true"), "\"create_parents\""); // <- Crea el folder GUID por ti
            content.Add(new StringContent(destinationFolder), "\"path\"");

            // Procesar el IFormFile como Stream (Eficiente en memoria)
            using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            
            content.Add(fileContent, "\"file\"", $"\"{fileName}\"");
            uploadReq.Content = content;

            var uploadResp = await _httpClient.SendAsync(uploadReq);
            var uploadBody = await uploadResp.Content.ReadAsStringAsync();
            var uploadResult = JsonSerializer.Deserialize<SynologyBaseResponse>(uploadBody);

            if (uploadResult == null || !uploadResult.Success)
                throw new Exception($"Error al subir archivo a Synology FileStation. Code: {uploadResult?.Error?.Code}");

            // 2. GENERAR LINK COMPARTIDO
            string fullPath = $"{destinationFolder.TrimEnd('/')}/{fileName}";
            return await GenerateSharingLinkAsync(fullPath, sid);
        }

        /// <summary>
        /// Genera el enlace público (Shared Link) del archivo subido
        /// </summary>
        private async Task<string> GenerateSharingLinkAsync(string filePath, string sid)
        {
            string url = $"{_synologyBaseUrl.TrimEnd('/')}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=create" +
                         $"&path=%22{Uri.EscapeDataString(filePath)}%22&_sid={sid}";

            var response = await _httpClient.GetFromJsonAsync<SynologySharingResponse>(url);

            if (response != null && response.Success && response.Data?.Links?.Length > 0)
                return response.Data.Links[0].Url;

            throw new Exception("No se pudo generar el enlace compartido en Synology.");
        }

        /// <summary>
        /// Elimina físicamente el archivo del NAS y también elimina su Link Compartido
        /// </summary>
        public async Task DeleteBySharingUrlAsync(string sharingUrl)
        {
            if (string.IsNullOrEmpty(sharingUrl)) return;

            string sid = await GetSidAsync();
            
            // 1. Obtener la lista de enlaces compartidos para buscar a cuál pertenece la URL que nos pasaron
            string listUrl = $"{_synologyBaseUrl.TrimEnd('/')}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=list&_sid={sid}";
            var listResponse = await _httpClient.GetFromJsonAsync<SynologySharingResponse>(listUrl);

            if (listResponse != null && listResponse.Success && listResponse.Data?.Links != null)
            {
                // Encontrar el enlace
                var matchedLink = listResponse.Data.Links.FirstOrDefault(l => l.Url == sharingUrl);
                
                if (matchedLink != null)
                {
                    // 2. Borrar el archivo real en el NAS
                    // El path en la API de eliminación es un arreglo en JSON ["/ruta/al/archivo"] por eso el %5B%22...%22%5D
                    string deleteFileUrl = $"{_synologyBaseUrl.TrimEnd('/')}/webapi/entry.cgi?api=SYNO.FileStation.Delete&version=2&method=delete" +
                                           $"&path=%5B%22{Uri.EscapeDataString(matchedLink.Path)}%22%5D&recursive=true&_sid={sid}";
                    await _httpClient.GetAsync(deleteFileUrl);

                    // 3. Limpiar/Borrar el enlace compartido de la base de datos de Synology
                    string cleanLinkUrl = $"{_synologyBaseUrl.TrimEnd('/')}/webapi/entry.cgi?api=SYNO.FileStation.Sharing&version=3&method=delete" +
                                          $"&id=%5B%22{matchedLink.Id}%22%5D&_sid={sid}";
                    await _httpClient.GetAsync(cleanLinkUrl);
                }
            }
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