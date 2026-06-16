using CorrePalabras.Data;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorrePalabras.Services
{
    public class AttachmentsService : IAttachmentsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISynologyService _synologyService;

        public AttachmentsService(ApplicationDbContext context, ISynologyService synologyService)
        {
            _context = context;
            _synologyService = synologyService;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            var attachments = await _context.Attachments
                .OrderBy(a => a.Id)
                .Select(a => new { a.Id, a.StoryId, a.LanguageId, a.TypeImage, a.Position, a.OrderAttachments })
                .ToListAsync();
            return attachments.Select(a => (object)new
            {
                a.Id, a.StoryId, a.LanguageId,
                ImageUrl = $"/api/attachments/{a.Id}/image",
                a.TypeImage, a.Position, a.OrderAttachments
            });
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            var a = await _context.Attachments
                .Where(a => a.Id == id)
                .Select(a => new { a.Id, a.StoryId, a.LanguageId, a.TypeImage, a.Position, a.OrderAttachments })
                .FirstOrDefaultAsync();
            if (a == null) return null;
            return new { a.Id, a.StoryId, a.LanguageId, ImageUrl = $"/api/attachments/{a.Id}/image", a.TypeImage, a.Position, a.OrderAttachments };
        }

        public async Task<(byte[] Bytes, string ContentType)> GetImageAsync(Guid id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null) throw new KeyNotFoundException("Archivo no encontrado.");
            return await _synologyService.DownloadBySharingUrlAsync(attachment.ImageUrl);
        }

        public async Task<string> CreateAsync(IFormFile? file, AttachmentDTO attachmentDTO)
        {
            string? imageUrl = null;

            if (file != null && file.Length > 0)
            {
                string folderPath = $"/CPAPPDEV/img/stories/{attachmentDTO.StoryId}";
                string fileExtension = Path.GetExtension(file.FileName);
                string fileName = $"{attachmentDTO.StoryId}_attachment{fileExtension}";
                imageUrl = await _synologyService.UploadAndShareAsync(file, folderPath, fileName);
            }

            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                StoryId = attachmentDTO.StoryId,
                LanguageId = attachmentDTO.LanguageId,
                ImageUrl = imageUrl,
                TypeImage = attachmentDTO.TypeImage,
                Position = attachmentDTO.Position,
                OrderAttachments = attachmentDTO.OrderAttachments
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return "Archivo creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, IFormFile? file, AttachmentDTO attachmentDTO)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null) throw new KeyNotFoundException("Archivo no encontrado.");

            if (file != null && file.Length > 0)
            {
                string folderPath = $"/CPAPPDEV/img/stories/{attachmentDTO.StoryId}";
                await _synologyService.DeleteBySharingUrlAsync(attachment.ImageUrl);
                string fileExtension = Path.GetExtension(file.FileName);
                string fileName = $"{attachmentDTO.StoryId}_attachment{fileExtension}";
                attachment.ImageUrl = await _synologyService.UploadAndShareAsync(file, folderPath, fileName);
            }

            attachment.StoryId = attachmentDTO.StoryId;
            attachment.LanguageId = attachmentDTO.LanguageId;
            attachment.TypeImage = attachmentDTO.TypeImage;
            attachment.Position = attachmentDTO.Position;
            attachment.OrderAttachments = attachmentDTO.OrderAttachments;

            _context.Attachments.Update(attachment);
            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null) throw new KeyNotFoundException("Archivo no encontrado.");

            if (!string.IsNullOrEmpty(attachment.ImageUrl))
            {
                await _synologyService.DeleteBySharingUrlAsync(attachment.ImageUrl);
            }

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return "Archivo eliminado correctamente.";
        }
    }
}