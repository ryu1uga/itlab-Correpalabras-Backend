using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IStoryLanguagesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(StoryLanguageDTO dto);
        Task<string> UpdateAsync(Guid id, StoryLanguageDTO dto);
        Task<string> DeleteAsync(Guid id);
    }
}