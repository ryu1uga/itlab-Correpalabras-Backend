using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IUnlockedBadgesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(UnlockedBadgeDTO dto);
        Task<string> UpdateAsync(Guid id, UnlockedBadgeDTO dto);
        Task<string> DeleteAsync(Guid id);
    }
}