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
    public class LanguagesController : BaseController
    {
        private readonly ILanguagesService _service;

        public LanguagesController(ILanguagesService service)
        {
            _service = service;
        }

        /// <summary>Obtiene todos los idiomas</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetLanguages([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene un idioma por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLanguage(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Idioma no encontrado.");
            
            return SuccessResponse(data);
        }

        /// <summary>Top 5 idiomas más demandados</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("mostDemanded")]
        public async Task<IActionResult> GetMostDemanded([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostDemandedAsync();
            return SuccessResponse(data);
        }

        /// <summary>Top 5 idiomas más demandados por rango de edad</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("mostDemandedByAgeRange")]
        public async Task<IActionResult> GetByAge([FromQuery] int minAge, [FromQuery] int maxAge, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostDemandedByAgeRangeAsync(minAge, maxAge);
            return SuccessResponse(data);
        }

        /// <summary>Top 5 idiomas más demandados por género</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("mostDemandedByGender")]
        public async Task<IActionResult> GetByGender([FromQuery] string gender, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostDemandedByGenderAsync(gender);
            return SuccessResponse(data);
        }

        /// <summary>Crea un nuevo idioma</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> CreateLanguage([FromBody] LanguageDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.CreateAsync(dto);
            return SuccessResponse(result);
        }

        /// <summary>Actualiza un idioma</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
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

        /// <summary>Elimina un idioma</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
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