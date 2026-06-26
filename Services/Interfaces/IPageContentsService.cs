using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IPageContentsService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetByStoryAsync(Guid storyId, Guid? languageId);
        Task<string> CreateAsync(PageContentRequest dto);
        Task<string> UpdateAsync(Guid id, PageContentRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}