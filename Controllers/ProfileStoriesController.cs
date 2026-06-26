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
    public class ProfileStoriesController : BaseController
    {
        private readonly IProfileStoriesService _service;

        public ProfileStoriesController(IProfileStoriesService service) => _service = service;

        /// <summary>Obtiene todas las historias de perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene una historia de perfil por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, [FromHeader] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Relación perfil-cuento no encontrado.");
            
            return SuccessResponse(data);
        }

        /// <summary>Registra una historia en un perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProfileStoryDTO dto, [FromHeader] Guid userId)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        /// <summary>Actualiza una historia de perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProfileStoryDTO dto, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Relación perfil-cuento no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Marca una historia como descargada</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}/downloaded")]
        public async Task<IActionResult> UpdateDownloaded(Guid id, [FromBody] bool isDownloaded, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.UpdateDownloadedAsync(id, isDownloaded);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Relación perfil-cuento no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Marca una historia como leída</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}/read")]
        public async Task<IActionResult> UpdateRead(Guid id, [FromBody] bool isRead, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.UpdateReadAsync(id, isRead);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Relación perfil-cuento no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Elimina una historia de perfil</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Relación perfil-cuento no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }
    }
}