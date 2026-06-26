using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface ILanguagesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetMostDemandedAsync();
        Task<IEnumerable<object>> GetMostDemandedByAgeRangeAsync(int minAge, int maxAge);
        Task<IEnumerable<object>> GetMostDemandedByGenderAsync(string gender);
        Task<string> CreateAsync(LanguageRequest dto);
        Task<string> UpdateAsync(Guid id, LanguageRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}