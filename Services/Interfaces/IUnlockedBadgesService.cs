using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IUnlockedBadgesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(UnlockedBadgeRequest dto);
        Task<string> UpdateAsync(Guid id, UnlockedBadgeRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}