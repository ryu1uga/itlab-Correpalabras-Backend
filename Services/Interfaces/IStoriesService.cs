using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

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
        Task<StoryRequest> CreateAsync(StoryRequest dto, IFormFile thumbnail);
        Task<string> UpdateAsync(Guid id, StoryRequest dto, IFormFile? thumbnail);
        Task<string> DeleteAsync(Guid id);
    }
}