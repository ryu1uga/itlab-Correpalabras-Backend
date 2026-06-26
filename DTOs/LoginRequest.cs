using System.ComponentModel.DataAnnotations;

namespace CorrePalabras.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El email debe tener un formato válido.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = "";
    }
}
