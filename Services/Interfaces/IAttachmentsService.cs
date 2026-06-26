using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IAttachmentsService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<(byte[] Bytes, string ContentType)> GetImageAsync(Guid id);
        Task<string> CreateAsync(IFormFile? file, AttachmentRequest dto);
        Task<string> UpdateAsync(Guid id, IFormFile? file, AttachmentRequest dto);
        Task<string> DeleteAsync(Guid id);
    }
}