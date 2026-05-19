using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.DTOs.Common;
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

        [HttpGet]
        public async Task<IActionResult> GetBadges([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBadge(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Insignia no encontrada.");

            return SuccessResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBadge([FromBody] BadgeDTO badgeDTO, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.CreateAsync(badgeDTO);
            return SuccessResponse(result);
        }

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