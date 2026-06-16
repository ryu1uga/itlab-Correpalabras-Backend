using Microsoft.AspNetCore.Http;
using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IStoriesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id, Guid userId);
        Task<IEnumerable<object>> GetByCategoryAsync(Guid categoryId, string? orderedBy);
        Task<object?> GetRandomStoryAsync();
        Task<IEnumerable<object>> GetMostReadAsync();
        Task<IEnumerable<object>> GetMostReadByAgeRangeAsync(int minAge, int maxAge);
        Task<IEnumerable<object>> GetMostReadByGenderAsync(string gender);
        Task<(byte[] Bytes, string ContentType)> GetImageAsync(Guid id);
        Task<StoryDTO> CreateAsync(StoryDTO storyDTO, IFormFile thumbnail);
        Task<string> UpdateAsync(Guid id, StoryDTO storyDTO, IFormFile? thumbnail);
        Task<string> DeleteAsync(Guid id);
    }
}