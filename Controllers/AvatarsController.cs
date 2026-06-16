using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AvatarsController : BaseController
    {
        private readonly IAvatarsService _service;

        public AvatarsController(IAvatarsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvatars([FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAvatar(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Avatar no encontrado.");

            return SuccessResponse(data);
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvatarImage(Guid id)
        {
            try
            {
                var (bytes, contentType) = await _service.GetImageAsync(id);
                return File(bytes, contentType);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostAvatar([FromForm] IFormFile avatarImage, [FromForm] Guid? storyId, [FromHeader(Name = "UserId")] Guid userId)
        {
            try 
            {
                var result = await _service.CreateAsync(avatarImage, storyId);
                return SuccessResponse(result);
            } 
            catch (Exception ex) 
            {
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAvatar(Guid id, [FromForm] IFormFile? avatarImage, [FromForm] Guid? storyId, [FromHeader(Name = "UserId")] Guid userId)
        {
            try {
                var result = await _service.UpdateAsync(id, avatarImage, storyId);
                return SuccessResponse(result);
            } catch (KeyNotFoundException) {
                return NotFoundResponse("Avatar no encontrado.");
            } catch (Exception ex) {
                return ErrorResponse(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvatar(Guid id, [FromHeader(Name = "UserId")] Guid userId)
        {
            try {
                var result = await _service.DeleteAsync(id);
                return SuccessResponse(result);
            } catch (KeyNotFoundException) {
                return NotFoundResponse("Avatar no encontrado.");
            } catch (Exception ex) {
                return ErrorResponse(ex.Message);
            }
        }
    }
}