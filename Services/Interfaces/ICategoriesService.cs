using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface ICategoriesService
    {
        Task<IEnumerable<object>> GetAllAsync(bool isAdmin);
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetMostVisitedAsync();
        Task<IEnumerable<object>> GetMostVisitedByAgeRangeAsync(int minAge, int maxAge);
        Task<IEnumerable<object>> GetMostVisitedByGenderAsync(string gender);
        Task<string> CreateAsync(CategoryRequest dto);
        Task<string> UpdateAsync(Guid id, CategoryRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}