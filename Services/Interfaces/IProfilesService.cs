using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IProfilesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<int> GetTotalCountAsync();
        Task<int> GetCountByAgeRangeAsync(int minAge, int maxAge);
        Task<object> GetGenderStatsAsync();
        Task<object?> GetStoriesSummaryAsync(Guid profileId);
        Task<string> CreateAsync(ProfileRequest dto, Guid userId);
        Task<string> UpdateAsync(Guid id, ProfileRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}