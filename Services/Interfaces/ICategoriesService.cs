using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface ICategoriesService
    {
        Task<IEnumerable<object>> GetAllAsync(bool isAdmin);
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetMostVisitedAsync();
        Task<IEnumerable<object>> GetMostVisitedByAgeRangeAsync(int minAge, int maxAge);
        Task<IEnumerable<object>> GetMostVisitedByGenderAsync(string gender);
        Task<string> CreateAsync(CategoryDTO categoryDTO);
        Task<string> UpdateAsync(Guid id, CategoryDTO categoryDTO);
        Task<string> DeleteAsync(Guid id);
    }
}