using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StoriesController : BaseController
    {
        private readonly IStoriesService _service;
        public StoriesController(IStoriesService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, [FromHeader] Guid userId)
        {
            var data = await _service.GetByIdAsync(id, userId);
            if (data == null) return NotFoundResponse("Cuento no encontrado.");
            
            return SuccessResponse(data);
        }

        [HttpGet("ByCategory")]
        public async Task<IActionResult> GetByCategory([FromHeader] Guid userId, [FromQuery] Guid categoryId, [FromQuery] string? orderedBy = null)
        {
            var data = await _service.GetByCategoryAsync(categoryId, orderedBy);
            if (data == null || !((IEnumerable<object>)data).Any()) 
                return NotFoundResponse("No se encontraron cuentos para esta categoría.");

            return SuccessResponse(data);
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandom([FromHeader] Guid userId)
        {
            var data = await _service.GetRandomStoryAsync();
            if (data == null) return NotFoundResponse("No se encontraron cuentos INV.");
            
            return SuccessResponse(data);
        }

        [HttpGet("mostRead")]
        public async Task<IActionResult> GetMostRead([FromHeader] Guid userId)
        {
            var data = await _service.GetMostReadAsync();
            return SuccessResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] StoryDTO dto, [FromForm] IFormFile thumbnail, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.CreateAsync(dto, thumbnail);
                return SuccessResponse(result); 
            }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] StoryDTO dto, [FromForm] IFormFile? thumbnail, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.UpdateAsync(id, dto, thumbnail);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Cuento no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Cuento no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }
    }
}