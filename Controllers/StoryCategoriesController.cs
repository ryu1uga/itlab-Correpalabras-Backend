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
    public class StoryCategoriesController : BaseController
    {
        private readonly IStoryCategoriesService _service;

        public StoryCategoriesController(IStoryCategoriesService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, [FromHeader] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Relación cuento-categoría no encontrado.");
            
            return SuccessResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StoryCategoryDTO dto, [FromHeader] Guid userId)
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
        public async Task<IActionResult> Update(Guid id, [FromBody] StoryCategoryDTO dto, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Relación cuento-categoría no encontrado."); }
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
            catch (KeyNotFoundException) { return NotFoundResponse("Relación cuento-categoría no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }
    }
}