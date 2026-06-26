using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IStoryLanguagesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(StoryLanguageRequest dto);
        Task<string> UpdateAsync(Guid id, StoryLanguageRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}