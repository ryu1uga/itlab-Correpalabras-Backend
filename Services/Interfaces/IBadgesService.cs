using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IBadgesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(BadgeRequest dto);
        Task<string> UpdateAsync(Guid id, BadgeRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}