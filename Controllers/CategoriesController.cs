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
    public class CategoriesController : BaseController
    {
        private readonly ICategoriesService _service;

        public CategoriesController(ICategoriesService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync(isAdmin: false);
            return SuccessResponse(data);
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetCategoriesAdmin([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync(isAdmin: true);
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Categoría no encontrada.");
            
            return SuccessResponse(data);
        }

        [HttpGet("mostVisited")]
        public async Task<IActionResult> GetMostVisitedCategories([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostVisitedAsync();
            return SuccessResponse(data);
        }

        [HttpGet("mostVisitedByAgeRange")]
        public async Task<IActionResult> GetMostVisitedByAge([FromQuery] int minAge, [FromQuery] int maxAge, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostVisitedByAgeRangeAsync(minAge, maxAge);
            return SuccessResponse(data);
        }

        [HttpGet("mostVisitedByGender")]
        public async Task<IActionResult> GetMostVisitedByGender([FromQuery] string gender, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetMostVisitedByGenderAsync(gender);
            return SuccessResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            var result = await _service.CreateAsync(dto);
            return SuccessResponse(result);
        }

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