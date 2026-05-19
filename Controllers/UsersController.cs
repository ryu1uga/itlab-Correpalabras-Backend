using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IUsersService _service;
        public UsersController(IUsersService service) => _service = service;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Usuario no encontrado.");
            
            return SuccessResponse(data);
        }

        [Authorize]
        [HttpGet("{id}/profiles")]
        public async Task<IActionResult> GetProfiles(Guid id)
        {
            var data = await _service.GetUserProfilesAsync(id);
            if (data == null) return NotFoundResponse("Usuario no encontrado.");
            
            return SuccessResponse(data);
        }

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