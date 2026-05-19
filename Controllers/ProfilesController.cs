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
    public class ProfilesController : BaseController
    {
        private readonly IProfilesService _service;

        public ProfilesController(IProfilesService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfiles([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Perfil no encontrado.");
            
            return SuccessResponse(data);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromHeader(Name = "UserId")] Guid userId)
        {
            var total = await _service.GetTotalCountAsync();
            return SuccessResponse(total);
        }

        [HttpGet("countByAgeRange")]
        public async Task<IActionResult> GetCountByAge([FromQuery] int minAge, [FromQuery] int maxAge, [FromHeader(Name = "UserId")] Guid userId)
        {
            var count = await _service.GetCountByAgeRangeAsync(minAge, maxAge);
            return SuccessResponse(count);
        }

        [HttpGet("countByGender")]
        public async Task<IActionResult> GetGenderCount([FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.GetGenderStatsAsync();
            return SuccessResponse(result);
        }

        [HttpGet("{id}/storiesSummary")]
        public async Task<IActionResult> GetStoriesSummary(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.GetStoriesSummaryAsync(id);
            if (result == null) return NotFoundResponse("Perfil no encontrado.");
            
            return SuccessResponse(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProfileDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.CreateAsync(dto, userId);
            return SuccessResponse(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProfileDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Perfil no encontrado.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Perfil no encontrado.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }
    }
}