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
    public class AttachmentsController : BaseController
    {
        private readonly IAttachmentsService _service;

        public AttachmentsController(IAttachmentsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAttachments([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttachment(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Archivo no encontrado.");

            return SuccessResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAttachment([FromForm] IFormFile? file, [FromForm] AttachmentDTO dto, [FromHeader] Guid userId)
        {
            try {
                var result = await _service.CreateAsync(file, dto);
                return SuccessResponse(result);
            } catch (Exception ex) {
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttachment(Guid id, [FromForm] IFormFile? file, [FromForm] AttachmentDTO dto, [FromHeader] Guid userId)
        {
            try {
                var result = await _service.UpdateAsync(id, file, dto);
                return SuccessResponse(result);
            } catch (KeyNotFoundException) {
                return NotFoundResponse("Archivo no encontrado.");
            } catch (Exception ex) {
                return ErrorResponse(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(Guid id, [FromHeader] Guid userId)
        {
            try {
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result);
            } catch (KeyNotFoundException) {
                return NotFoundResponse("Archivo no encontrado.");
            } catch (Exception ex) {
                return ErrorResponse(ex.Message);
            }
        }
    }
}