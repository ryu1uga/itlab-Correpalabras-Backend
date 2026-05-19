using System.ComponentModel.DataAnnotations;

namespace CorrePalabras.DTOs.Common
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El email debe tener un formato válido.")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El código es requerido.")]
        public int Code { get; set; }
        
        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", 
            ErrorMessage = "La contraseña debe contener mayúsculas, minúsculas y números.")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La confirmación de contraseña es requerida.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
