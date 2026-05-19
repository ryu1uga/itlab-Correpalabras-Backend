using Microsoft.AspNetCore.Http;
using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IAttachmentsService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(IFormFile? file, AttachmentDTO attachmentDTO);
        Task<string> UpdateAsync(Guid id, IFormFile? file, AttachmentDTO attachmentDTO);
        Task<string> DeleteAsync(Guid id);
    }
}