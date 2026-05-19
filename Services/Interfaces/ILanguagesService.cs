using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface ILanguagesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetMostDemandedAsync();
        Task<IEnumerable<object>> GetMostDemandedByAgeRangeAsync(int minAge, int maxAge);
        Task<IEnumerable<object>> GetMostDemandedByGenderAsync(string gender);
        Task<string> CreateAsync(LanguageDTO languageDTO);
        Task<string> UpdateAsync(Guid id, LanguageDTO languageDTO);
        Task<string> DeleteAsync(Guid id);
    }
}