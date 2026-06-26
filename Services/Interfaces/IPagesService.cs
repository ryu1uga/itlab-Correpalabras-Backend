using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IPagesService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<PageRequest?> GetByIdAsync(Guid id);
        Task<(byte[] Bytes, string ContentType)> GetImageAsync(Guid id);
        Task<PageRequest> CreateAsync(PageRequest dto, IFormFile? imageFile);
        Task<string> UpdateAsync(Guid id, PageRequest dto, IFormFile? imageFile);
        Task<string> DeleteAsync(Guid id);
    }
}