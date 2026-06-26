using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IStoryCategoriesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(StoryCategoryRequest dto);
        Task<string> UpdateAsync(Guid id, StoryCategoryRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}