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
    public class UnlockedBadgesController : BaseController
    {
        private readonly IUnlockedBadgesService _service;

        public UnlockedBadgesController(IUnlockedBadgesService service) => _service = service;

        /// <summary>Obtiene todas las insignias desbloqueadas</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene una insignia desbloqueada por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, [FromHeader] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Insignia desbloqueada no encontrada.");
            
            return SuccessResponse(data);
        }

        /// <summary>Desbloquea una insignia para un perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> Create([FromHeader] Guid userId, [FromBody] UnlockedBadgeRequest dto)
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

        /// <summary>Actualiza una insignia desbloqueada</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromHeader] Guid userId, [FromBody] UnlockedBadgeRequest dto)
        {
            try 
            { 
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Insignia desbloqueada no encontrada."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Elimina una insignia desbloqueada</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Insignia desbloqueada no encontrada."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }
    }
}