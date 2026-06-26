    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
        using CorrePalabras.Models.Common;
    using CorrePalabras.Services.Interfaces;
    using System;
    using System.Threading.Tasks;
using CorrePalabras.DTOs;

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

            /// <summary>Obtiene todos los attachments</summary>
            [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
            [HttpGet]
            public async Task<IActionResult> GetAttachments([FromHeader(Name = "UserId")] Guid userId)
            {
                var data = await _service.GetAllAsync();
                return SuccessResponse(data);
            }

            /// <summary>Obtiene un attachment por ID</summary>
            [ProducesResponseType(typeof(ApiResponse<object>), 200)]
            [ProducesResponseType(typeof(ApiResponse<string>), 404)]
            [HttpGet("{id}")]
            public async Task<IActionResult> GetAttachment(Guid id, [FromHeader(Name = "UserId")] Guid userId)
            {
                var data = await _service.GetByIdAsync(id);
                if (data == null) return NotFoundResponse("Archivo no encontrado.");

                return SuccessResponse(data);
            }

            /// <summary>Descarga la imagen de un attachment</summary>
            [ProducesResponseType(typeof(FileContentResult), 200)]
            [ProducesResponseType(404)]
            [HttpGet("{id}/image")]
            [AllowAnonymous]
            public async Task<IActionResult> GetAttachmentImage(Guid id)
            {
                try
                {
                    var (bytes, contentType) = await _service.GetImageAsync(id);
                    return File(bytes, contentType);
                }
                catch (KeyNotFoundException) { return NotFound(); }
                catch (Exception ex) { return StatusCode(500, ex.Message); }
            }

            /// <summary>Crea un nuevo attachment</summary>
            [ProducesResponseType(typeof(ApiResponse<string>), 200)]
            [ProducesResponseType(typeof(ApiResponse<string>), 400)]
            [HttpPost]
            public async Task<IActionResult> CreateAttachment([FromForm] IFormFile? file, [FromForm] AttachmentRequest dto, [FromHeader] Guid userId)
            {
                try {
                    var result = await _service.CreateAsync(file, dto);
                    return SuccessResponse(result);
                } catch (Exception ex) {
                    return ErrorResponse(ex.Message);
                }
            }

            /// <summary>Actualiza un attachment</summary>
            [ProducesResponseType(typeof(ApiResponse<string>), 200)]
            [ProducesResponseType(typeof(ApiResponse<string>), 404)]
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateAttachment(Guid id, [FromForm] IFormFile? file, [FromForm] AttachmentRequest dto, [FromHeader] Guid userId)
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

            /// <summary>Elimina un attachment</summary>
            [ProducesResponseType(typeof(ApiResponse<string>), 200)]
            [ProducesResponseType(typeof(ApiResponse<string>), 404)]
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