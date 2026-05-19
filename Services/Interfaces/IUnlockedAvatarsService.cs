using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IUnlockedAvatarsService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetByProfileAsync(Guid profileId);
        Task<string> CreateAsync(UnlockedAvatarDTO dto);
        Task<string> UpdateAsync(Guid id, UnlockedAvatarDTO dto);
        Task<string> DeleteAsync(Guid id);
    }
}