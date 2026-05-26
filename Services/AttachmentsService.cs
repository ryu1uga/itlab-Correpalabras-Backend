using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        private readonly Cloudinary _cloudinary;

        public AttachmentsService(ApplicationDbContext context)
        {
            _context = context;

            // Configuración de Cloudinary desde variables de entorno
            var account = new Account(
                Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.Attachments
                .OrderBy(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.StoryId,
                    a.LanguageId,
                    a.ImageUrl,
                    a.TypeImage,
                    a.Position,
                    a.OrderAttachments
                }).ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.Attachments
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.StoryId,
                    a.LanguageId,
                    a.ImageUrl,
                    a.TypeImage,
                    a.Position,
                    a.OrderAttachments
                }).FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(IFormFile? file, AttachmentDTO attachmentDTO)
        {
            string? imageUrl = null;

            if (file != null && file.Length > 0)
            {
                imageUrl = await UploadImageAsync(file);
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
                attachment.ImageUrl = await UploadImageAsync(file);
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
                await DeleteImageAsync(attachment.ImageUrl);
            }

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return "Archivo eliminado correctamente.";
        }

        // Métodos privados auxiliares para Cloudinary
        private async Task<string> UploadImageAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "corre_palabras_attachments"
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null) throw new Exception(result.Error.Message);
            return result.SecureUrl.ToString();
        }

        private async Task DeleteImageAsync(string imageUrl)
        {
            var uri = new Uri(imageUrl);
            var publicId = string.Join("/", uri.AbsolutePath.Split('/').Skip(5));
            var dotIndex = publicId.IndexOf('.');
            if (dotIndex >= 0) publicId = publicId.Substring(0, dotIndex);

            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            if (result.Error != null) throw new Exception("No se pudo eliminar la imagen de Cloudinary.");
        }
    }
}