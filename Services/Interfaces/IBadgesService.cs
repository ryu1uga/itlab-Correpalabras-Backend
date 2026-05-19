using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IBadgesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(BadgeDTO badgeDTO);
        Task<string> UpdateAsync(Guid id, BadgeDTO badgeDTO);
        Task<string> DeleteAsync(Guid id);
    }
}