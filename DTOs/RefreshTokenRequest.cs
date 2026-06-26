using System.ComponentModel.DataAnnotations;

namespace CorrePalabras.DTOs
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "El refresh token es requerido.")]
        public string RefreshToken { get; set; } = "";
    }
}
