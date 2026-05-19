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
    public class PageContentsController : BaseController
    {
        private readonly IPageContentsService _service;

        public PageContentsController(IPageContentsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetPageContents([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPageContent(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Contenido de página no encontrado.");
            
            return SuccessResponse(data);
        }

        [HttpGet("ByStory/{storyId}")]
        public async Task<IActionResult> GetByStory(Guid storyId, [FromHeader(Name = "UserId")] Guid userId, [FromQuery] Guid? languageId = null)
        {
            var data = await _service.GetByStoryAsync(storyId, languageId);
            
            // Verificamos si la lista tiene elementos
            if (data == null || !((IEnumerable<object>)data).Any()) 
                return NotFoundResponse("Contenido de página no encontrado.");
            
            return SuccessResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PageContentDTO dto, [FromHeader(Name = "UserId")] Guid userId)
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PageContentDTO dto, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result);
            } 
            catch (KeyNotFoundException) 
            {
                return NotFoundResponse("Contenido de página no encontrado.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

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
                return NotFoundResponse("Contenido de página no encontrado.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }
    }
}