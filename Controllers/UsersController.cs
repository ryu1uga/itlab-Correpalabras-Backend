using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IUsersService _service;
        public UsersController(IUsersService service) => _service = service;

        /// <summary>Obtiene todos los usuarios</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene un usuario por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Usuario no encontrado.");
            
            return SuccessResponse(data);
        }

        /// <summary>Obtiene los perfiles de un usuario</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [Authorize]
        [HttpGet("{id}/profiles")]
        public async Task<IActionResult> GetProfiles(Guid id)
        {
            var data = await _service.GetUserProfilesAsync(id);
            if (data == null) return NotFoundResponse("Usuario no encontrado.");
            
            return SuccessResponse(data);
        }

        /// <summary>Login de usuario</summary>
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            try 
            { 
                var result = await _service.LoginAsync(dto, false);
                return SuccessResponse(result); 
            }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Login de administrador</summary>
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost("loginAdmin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginRequestDTO dto)
        {
            try 
            { 
                var result = await _service.LoginAsync(dto, true);
                return SuccessResponse(result); 
            }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }
        
        /// <summary>Renueva el access token usando el refresh token</summary>
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO dto)
        {
            try
            {
                var result = await _service.RefreshTokenAsync(dto);
                return SuccessResponse(result);
            }
            catch (Exception ex) { return ErrorResponse(ex.Message, 401); }
        }

        /// <summary>Cierra la sesión del usuario</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return ErrorResponse("Token inválido.", 401);

                var result = await _service.LogoutAsync(userId);
                return SuccessResponse(result);
            }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Envía código de verificación al email</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost("verifyemail")]
        public async Task<IActionResult> VerifyEmail([FromBody] EmailDTO dto)
        {
            try 
            { 
                var result = await _service.GenerateVerificationCodeAsync(dto.Email);
                return SuccessResponse(result); 
            }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Restablece la contraseña con código de verificación</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] ResetPasswordDTO dto)
        {
            try 
            { 
                var result = await _service.ResetPasswordAsync(dto);
                return SuccessResponse(result); 
            }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Crea un nuevo usuario</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDTO dto)
        {
            try 
            { 
                var result = await _service.CreateAsync(dto);
                return SuccessResponse(result); 
            }
            catch (InvalidOperationException ex) { return ErrorResponse(ex.Message, 409); } // 409 Conflict
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Cambia el rol de un usuario (solo admin)</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [Authorize]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] int userType)
        {
            var callerType = User.FindFirstValue("userType");
            if (callerType != "1")
                return StatusCode(403, new { message = "Solo un administrador puede cambiar roles." });

            try
            {
                var result = await _service.UpdateRoleAsync(id, userType);
                return SuccessResponse(result);
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Usuario no encontrado."); }
            catch (Exception ex)         { return ErrorResponse(ex.Message); }
        }

        /// <summary>Elimina un usuario con verificación por email</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] EmailVerificationDTO dto)
        {
            try 
            { 
                var result = await _service.DeleteAsync(dto);
                return SuccessResponse(result); 
            }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }
    }
}