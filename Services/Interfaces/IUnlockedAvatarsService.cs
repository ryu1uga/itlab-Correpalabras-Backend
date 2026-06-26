using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IUnlockedAvatarsService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetByProfileAsync(Guid profileId);
        Task<string> CreateAsync(UnlockedAvatarRequest dto);
        Task<string> UpdateAsync(Guid id, UnlockedAvatarRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}