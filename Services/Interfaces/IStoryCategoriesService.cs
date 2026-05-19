using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IStoryCategoriesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(StoryCategoryDTO dto);
        Task<string> UpdateAsync(Guid id, StoryCategoryDTO dto);
        Task<string> DeleteAsync(Guid id);
    }
}