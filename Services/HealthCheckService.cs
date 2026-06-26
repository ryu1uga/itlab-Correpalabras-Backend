using Microsoft.EntityFrameworkCore;
using CorrePalabras.Data;
using CorrePalabras.Services.Interfaces;
using System;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ApplicationDbContext _context;

        public HealthCheckService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Status, string? ErrorMessage)> CheckDatabaseConnectionAsync()
        {
            try
            {
                // Verifica la conexión a la base de datos
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                return (true, "ok", null);
            }
            catch (Exception ex)
            {
                return (false, "error", ex.Message);
            }
        }
    }
}