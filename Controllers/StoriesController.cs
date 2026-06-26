using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : BaseController
    {
        private readonly IStoriesService _service;
        public StoriesController(IStoriesService service) => _service = service;

        /// <summary>Obtiene todos los cuentos</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromHeader] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene un cuento por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id, [FromHeader] Guid userId)
        {
            var data = await _service.GetByIdAsync(id, userId);
            if (data == null) return NotFoundResponse("Cuento no encontrado.");

            return SuccessResponse(data);
        }

        /// <summary>Descarga la imagen de portada de un cuento</summary>
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImage(Guid id)
        {
            try
            {
                var (bytes, contentType) = await _service.GetImageAsync(id);
                return File(bytes, contentType);
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        /// <summary>Obtiene cuentos por categoría</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("ByCategory")]
        [Authorize]
        public async Task<IActionResult> GetByCategory([FromHeader] Guid userId, [FromQuery] Guid categoryId, [FromQuery] string? orderedBy = null)
        {
            var data = await _service.GetByCategoryAsync(categoryId, orderedBy);
            if (data == null || !((IEnumerable<object>)data).Any()) 
                return NotFoundResponse("No se encontraron cuentos para esta categoría.");

            return SuccessResponse(data);
        }

        /// <summary>Obtiene un cuento aleatorio (no requiere JWT)</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("random")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRandom()
        {
            var data = await _service.GetRandomStoryAsync();
            if (data == null) return NotFoundResponse("No se encontraron cuentos.");
            return SuccessResponse(data);
        }

        /// <summary>Top 5 cuentos más leídos</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet("mostRead")]
        [Authorize]
        public async Task<IActionResult> GetMostRead([FromHeader] Guid userId)
        {
            var data = await _service.GetMostReadAsync();
            return SuccessResponse(data);
        }

        /// <summary>Crea un nuevo cuento</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] StoryRequest dto, [FromForm] IFormFile thumbnail, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.CreateAsync(dto, thumbnail);
                return SuccessResponse(result); 
            }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Actualiza un cuento</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromForm] StoryRequest dto, [FromForm] IFormFile? thumbnail, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.UpdateAsync(id, dto, thumbnail);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Cuento no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Elimina un cuento</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpDelete("{id}")]
        [Authorize]
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