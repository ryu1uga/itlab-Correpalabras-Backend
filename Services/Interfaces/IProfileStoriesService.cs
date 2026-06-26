using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IProfileStoriesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(ProfileStoryRequest dto);
        Task<string> UpdateAsync(Guid id, ProfileStoryRequest dto);
        Task<string> UpdateDownloadedAsync(Guid id, bool isDownloaded);
        Task<string> UpdateReadAsync(Guid id, bool isRead);
        Task<string> DeleteAsync(Guid id);
    }
}