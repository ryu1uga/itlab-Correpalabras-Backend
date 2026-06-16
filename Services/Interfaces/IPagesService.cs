using Microsoft.AspNetCore.Http;
using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IPagesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<PageDTO?> GetByIdAsync(Guid id);
        Task<(byte[] Bytes, string ContentType)> GetImageAsync(Guid id);
        Task<PageDTO> CreateAsync(PageDTO pageDTO, IFormFile? imageFile);
        Task<string> UpdateAsync(Guid id, PageDTO pageDTO, IFormFile? imageFile);
        Task<string> DeleteAsync(Guid id);
    }
}