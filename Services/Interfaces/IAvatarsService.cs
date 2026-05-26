using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IAvatarsService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(IFormFile avatarImage, Guid storyId);
        Task<string> UpdateAsync(Guid id, IFormFile? avatarImage, Guid storyId);
        Task<string> DeleteAsync(Guid id);
    }
}