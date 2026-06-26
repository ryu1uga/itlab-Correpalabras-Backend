using Microsoft.AspNetCore.Mvc;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : BaseController
    {
        private readonly IHealthCheckService _service;

        public HealthCheckController(IHealthCheckService service)
        {
            _service = service;
        }

        /// <summary>Verifica el estado del servicio</summary>
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        // GET: api/healthcheck
        [HttpGet]
        public async Task<IActionResult> GetHealthCheck()
        {
            var result = await _service.CheckDatabaseConnectionAsync();

            if (result.Success)
            {
                // Usamos SuccessResponse para devolver { success: true, data: { status: "ok" } }
                return SuccessResponse(new { status = result.Status });
            }

            // Usamos ErrorResponse para devolver el error 500 estandarizado
            return ErrorResponse(result.ErrorMessage ?? "Error de conexión", 500);
        }
    }
}