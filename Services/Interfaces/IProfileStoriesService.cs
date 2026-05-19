using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IProfileStoriesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(ProfileStoryDTO dto);
        Task<string> UpdateAsync(Guid id, ProfileStoryDTO dto);
        Task<string> UpdateDownloadedAsync(Guid id, bool isDownloaded);
        Task<string> UpdateReadAsync(Guid id, bool isRead);
        Task<string> DeleteAsync(Guid id);
    }
}