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
    public class LanguagesController : BaseController
    {
        private readonly ILanguagesService _service;

        public LanguagesController(ILanguagesService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetLanguages([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLanguage(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Idioma no encontrado.");
            
            return SuccessResponse(data);
        }

        [HttpGet("mostDemanded")]
        public async Task<IActionResult> GetMostDemanded([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostDemandedAsync();
            return SuccessResponse(data);
        }

        [HttpGet("mostDemandedByAgeRange")]
        public async Task<IActionResult> GetByAge([FromQuery] int minAge, [FromQuery] int maxAge, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostDemandedByAgeRangeAsync(minAge, maxAge);
            return SuccessResponse(data);
        }

        [HttpGet("mostDemandedByGender")]
        public async Task<IActionResult> GetByGender([FromQuery] string gender, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostDemandedByGenderAsync(gender);
            return SuccessResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLanguage([FromBody] LanguageDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.CreateAsync(dto);
            return SuccessResponse(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLanguage(Guid id, [FromBody] LanguageDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Idioma no encontrado.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLanguage(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Idioma no encontrado.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }
    }
}