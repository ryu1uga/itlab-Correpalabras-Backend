using CorrePalabras.Data;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

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
            if (string.IsNullOrEmpty(attachment.ImageUrl)) throw new KeyNotFoundException("Este archivo no tiene imagen.");
            return await _synologyService.DownloadBySharingUrlAsync(attachment.ImageUrl);
        }

        public async Task<string> CreateAsync(IFormFile? file, AttachmentRequest dto)
        {
            string? imageUrl = null;

            if (file != null && file.Length > 0)
            {
                string folderPath = $"/CPAPPDEV/img/stories/{dto.StoryId}";
                string fileExtension = Path.GetExtension(file.FileName);
                string fileName = $"{dto.StoryId}_attachment{fileExtension}";
                imageUrl = await _synologyService.UploadAndShareAsync(file, folderPath, fileName);
            }

            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                StoryId = dto.StoryId,
                LanguageId = dto.LanguageId,
                ImageUrl = imageUrl,
                TypeImage = dto.TypeImage,
                Position = dto.Position,
                OrderAttachments = dto.OrderAttachments
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return "Archivo creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, IFormFile? file, AttachmentRequest dto)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null) throw new KeyNotFoundException("Archivo no encontrado.");

            if (file != null && file.Length > 0)
            {
                string folderPath = $"/CPAPPDEV/img/stories/{dto.StoryId}";
                await _synologyService.DeleteBySharingUrlAsync(attachment.ImageUrl);
                string fileExtension = Path.GetExtension(file.FileName);
                string fileName = $"{dto.StoryId}_attachment{fileExtension}";
                attachment.ImageUrl = await _synologyService.UploadAndShareAsync(file, folderPath, fileName);
            }

            attachment.StoryId = dto.StoryId;
            attachment.LanguageId = dto.LanguageId;
            attachment.TypeImage = dto.TypeImage;
            attachment.Position = dto.Position;
            attachment.OrderAttachments = dto.OrderAttachments;

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