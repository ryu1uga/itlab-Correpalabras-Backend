using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<string> CreateAsync(ProfileDTO profileDTO, Guid userId);
        Task<string> UpdateAsync(Guid id, ProfileDTO profileDTO);
        Task<string> DeleteAsync(Guid id);
    }
}