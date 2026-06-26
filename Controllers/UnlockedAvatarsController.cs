using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnlockedAvatarsController : BaseController
    {
        private readonly IUnlockedAvatarsService _service;

        public UnlockedAvatarsController(IUnlockedAvatarsService service) => _service = service;

        /// <summary>Obtiene todos los avatares desbloqueados</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene un avatar desbloqueado por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Avatar desbloqueado no encontrado.");
            
            return SuccessResponse(data);
        }

        /// <summary>Obtiene avatares desbloqueados por perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("Profile/{profileId}")]
        public async Task<IActionResult> GetByProfile(Guid profileId, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByProfileAsync(profileId);
            return SuccessResponse(data);
        }

        /// <summary>Desbloquea un avatar para un perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UnlockedAvatarRequest dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        /// <summary>Actualiza un avatar desbloqueado</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UnlockedAvatarRequest dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            { 
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Avatar desbloqueado no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Elimina un avatar desbloqueado</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            { 
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Avatar desbloqueado no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }
    }
}