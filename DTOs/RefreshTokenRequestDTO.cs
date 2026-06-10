using System.ComponentModel.DataAnnotations;

namespace CorrePalabras.DTOs.Common
{
    public class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "El refresh token es requerido.")]
        public string RefreshToken { get; set; } = "";
    }
}
