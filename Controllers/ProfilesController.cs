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
    public class ProfilesController : BaseController
    {
        private readonly IProfilesService _service;

        public ProfilesController(IProfilesService service)
        {
            _service = service;
        }

        /// <summary>Obtiene todos los perfiles</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetProfiles([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene un perfil por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Perfil no encontrado.");
            
            return SuccessResponse(data);
        }

        /// <summary>Obtiene el total de perfiles</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromHeader(Name = "UserId")] Guid userId)
        {
            var total = await _service.GetTotalCountAsync();
            return SuccessResponse(total);
        }

        /// <summary>Obtiene cantidad de perfiles por rango de edad</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [HttpGet("countByAgeRange")]
        public async Task<IActionResult> GetCountByAge([FromQuery] int minAge, [FromQuery] int maxAge, [FromHeader(Name = "UserId")] Guid userId)
        {
            var count = await _service.GetCountByAgeRangeAsync(minAge, maxAge);
            return SuccessResponse(count);
        }

        /// <summary>Obtiene distribución de perfiles por género</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [HttpGet("countByGender")]
        public async Task<IActionResult> GetGenderCount([FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.GetGenderStatsAsync();
            return SuccessResponse(result);
        }

        /// <summary>Obtiene resumen de historias de un perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}/storiesSummary")]
        public async Task<IActionResult> GetStoriesSummary(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.GetStoriesSummaryAsync(id);
            if (result == null) return NotFoundResponse("Perfil no encontrado.");
            
            return SuccessResponse(result);
        }

        /// <summary>Crea un nuevo perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProfileRequest dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.CreateAsync(dto, userId);
            return SuccessResponse(result);
        }

        /// <summary>Actualiza un perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProfileRequest dto, [FromHeader(Name = "UserId")] Guid userId)
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

        /// <summary>Elimina un perfil</summary>
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