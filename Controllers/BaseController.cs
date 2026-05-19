using Microsoft.AspNetCore.Mvc;
using CorrePalabras.Models.Common;
using System.Security.Claims;

namespace CorrePalabras.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IActionResult SuccessResponse<T>(T data) 
            => Ok(ApiResponse<T>(true, data));

        protected IActionResult ErrorResponse(string message, int statusCode = 400) 
            => StatusCode(statusCode, ApiResponse<string>(false, message));

        protected IActionResult UnauthorizedResponse() 
            => Unauthorized(ApiResponse<string>(false, "El token ingresado no es válido."));

        protected IActionResult NotFoundResponse(string message = "Recurso no encontrado.") 
            => NotFound(ApiResponse<string>(false, message));
        
        // Extraer userId del token JWT (seguro, no del header)
        protected Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Token inválido o usuario no identificado.");
            return userId;
        }
            
        // Helper privado para instanciar
        private ApiResponse<T> ApiResponse<T>(bool success, T data) => new ApiResponse<T>(success, data);
    }
}