namespace CorrePalabras.DTOs
{
    public class HealthCheckResponse
    {
        /// <example>OK</example>
        public string Status { get; set; } = "";
        /// <example>CorrePalabras API is running.</example>
        public string Message { get; set; } = "";
    }
}
