using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IHealthCheckService
    {
        Task<(bool Success, string Status, string? ErrorMessage)> CheckDatabaseConnectionAsync();
    }
}