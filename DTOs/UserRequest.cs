using System.ComponentModel.DataAnnotations;

namespace CorrePalabras.DTOs
{
    public class UserRequest
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 255 caracteres.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El email debe tener un formato válido.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "La contraseña debe contener mayúsculas, minúsculas y números.")]
        public string? Password { get; set; } = "";

        public int UserType { get; set; } = 0;
    }
}
