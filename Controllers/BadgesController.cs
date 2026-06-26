using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BadgesController : BaseController
    {
        private readonly IBadgesService _service;

        public BadgesController(IBadgesService service)
        {
            _service = service;
        }

        /// <summary>Obtiene todas las insignias</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetBadges([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene una insignia por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBadge(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Insignia no encontrada.");

            return SuccessResponse(data);
        }

        /// <summary>Crea una nueva insignia</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> CreateBadge([FromBody] BadgeDTO badgeDTO, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.CreateAsync(badgeDTO);
            return SuccessResponse(result);
        }

        /// <summary>Actualiza una insignia</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBadge(Guid id, [FromBody] BadgeDTO badgeDTO, [FromHeader(Name = "UserId")] Guid userId)
        {
            try
            {
                var result = await _service.UpdateAsync(id, badgeDTO);
                return SuccessResponse(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundResponse("Insignia no encontrada.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        /// <summary>Elimina una insignia</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBadge(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundResponse("Insignia no encontrada.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }
    }
}