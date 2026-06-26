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
    public class CategoriesController : BaseController
    {
        private readonly ICategoriesService _service;

        public CategoriesController(ICategoriesService service)
        {
            _service = service;
        }

        /// <summary>Obtiene categorías visibles</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetCategories([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync(isAdmin: false);
            return SuccessResponse(data);
        }

        /// <summary>Obtiene todas las categorías incluyendo ocultas</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("admin")]
        public async Task<IActionResult> GetCategoriesAdmin([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync(isAdmin: true);
            return SuccessResponse(data);
        }

        /// <summary>Obtiene una categoría por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Categoría no encontrada.");
            
            return SuccessResponse(data);
        }

        /// <summary>Top 5 categorías más visitadas</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("mostVisited")]
        public async Task<IActionResult> GetMostVisitedCategories([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostVisitedAsync();
            return SuccessResponse(data);
        }

        /// <summary>Top 5 categorías más visitadas por rango de edad</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("mostVisitedByAgeRange")]
        public async Task<IActionResult> GetMostVisitedByAge([FromQuery] int minAge, [FromQuery] int maxAge, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostVisitedByAgeRangeAsync(minAge, maxAge);
            return SuccessResponse(data);
        }

        /// <summary>Top 5 categorías más visitadas por género</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("mostVisitedByGender")]
        public async Task<IActionResult> GetMostVisitedByGender([FromQuery] string gender, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostVisitedByGenderAsync(gender);
            return SuccessResponse(data);
        }

        /// <summary>Crea una nueva categoría</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.CreateAsync(dto);
            return SuccessResponse(result);
        }

        /// <summary>Actualiza una categoría</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Categoría no encontrada.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        /// <summary>Elimina una categoría</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Categoría no encontrada.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }
    }
}