using CorrePalabras.Models.Common;
using System.Net;
using System.Text.Json;

namespace CorrePalabras.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<string>(false, "Ocurrió un error interno del servidor.");

            switch (exception)
            {
                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new ApiResponse<string>(false, "Recurso no encontrado.");
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new ApiResponse<string>(false, exception.Message);
                    break;

                case InvalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new ApiResponse<string>(false, exception.Message);
                    break;

                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new ApiResponse<string>(false, exception.Message);
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    // En producción, no expongas el mensaje de excepción
                    response = new ApiResponse<string>(false, 
                        "Ocurrió un error interno del servidor. Por favor, intenta más tarde.");
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
