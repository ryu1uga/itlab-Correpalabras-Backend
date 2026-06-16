using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.DTOs.Common;
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

        [HttpGet]
        public async Task<IActionResult> GetPages([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPage(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Página no encontrada.");

            return SuccessResponse(data);
        }

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