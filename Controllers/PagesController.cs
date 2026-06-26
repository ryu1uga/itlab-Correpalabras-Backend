using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PagesController : BaseController
    {
        private readonly IPagesService _service;

        public PagesController(IPagesService service)
        {
            _service = service;
        }

        /// <summary>Obtiene todas las páginas</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetPages([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene una página por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPage(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Página no encontrada.");

            return SuccessResponse(data);
        }

        /// <summary>Descarga la imagen de una página</summary>
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPageImage(Guid id)
        {
            try
            {
                var (bytes, contentType) = await _service.GetImageAsync(id);
                return File(bytes, contentType);
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        /// <summary>Crea una nueva página</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> CreatePage([FromForm] PageDTO pageDTO, [FromForm] IFormFile? imageFile, [FromHeader(Name = "UserId")] Guid userId)
        {
            if (pageDTO == null || pageDTO.PageOrder <= 0) 
                return ErrorResponse("Datos de página no válidos.");

            try 
            {
                var result = await _service.CreateAsync(pageDTO, imageFile);
                return SuccessResponse(result);
            } 
            catch (Exception ex) 
            {
                return ErrorResponse(ex.Message, 500);
            }
        }

        /// <summary>Actualiza una página</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePage(Guid id, [FromForm] PageDTO pageDTO, [FromForm] IFormFile? imageFile, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.UpdateAsync(id, pageDTO, imageFile);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Página no encontrada.");
            } 
            catch (Exception ex) 
            {
                return ErrorResponse(ex.Message);
            }
        }

        /// <summary>Elimina una página</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Página no encontrada.");
            } 
            catch (Exception ex) 
            {
                return ErrorResponse(ex.Message);
            }
        }
    }
}