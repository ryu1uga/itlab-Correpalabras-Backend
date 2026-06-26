namespace CorrePalabras.DTOs
{
    public class RecoveryCodeRequest
    {
        public string Email { get; set; } = string.Empty;
        public int Code { get; set; }
    }
}
