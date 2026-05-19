using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IPageContentsService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetByStoryAsync(Guid storyId, Guid? languageId);
        Task<string> CreateAsync(PageContentDTO dto);
        Task<string> UpdateAsync(Guid id, PageContentDTO dto);
        Task<string> DeleteAsync(Guid id);
    }
}