using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IHealthCheckService
    {
        Task<(bool Success, string Status, string? ErrorMessage)> CheckDatabaseConnectionAsync();
    }
}