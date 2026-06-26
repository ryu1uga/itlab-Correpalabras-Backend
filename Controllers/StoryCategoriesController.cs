using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StoryCategoriesController : BaseController
    {
        private readonly IStoryCategoriesService _service;

        public StoryCategoriesController(IStoryCategoriesService service) => _service = service;

        /// <summary>Obtiene todas las categorías de cuentos</summary>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] Guid userId)
        {
            var data = await _service.GetAllAsync();
            return SuccessResponse(data);
        }

        /// <summary>Obtiene una categoría de cuento por ID</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, [FromHeader] Guid userId)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFoundResponse("Relación cuento-categoría no encontrado.");
            
            return SuccessResponse(data);
        }

        /// <summary>Asigna una categoría a un cuento</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StoryCategoryRequest dto, [FromHeader] Guid userId)
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

        /// <summary>Actualiza una categoría de cuento</summary>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StoryCategoryRequest dto, [FromHeader] Guid userId)
        {
            try 
            { 
                var result = await _service.UpdateAsync(id, dto);
                return SuccessResponse(result); 
            }
            catch (KeyNotFoundException) { return NotFoundResponse("Relación cuento-categoría no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }

        /// <summary>Elimina una categoría de cuento</summary>
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
            catch (KeyNotFoundException) { return NotFoundResponse("Relación cuento-categoría no encontrado."); }
            catch (Exception ex) { return ErrorResponse(ex.Message); }
        }
    }
}