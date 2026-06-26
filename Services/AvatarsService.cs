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
    public class AvatarsService : IAvatarsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISynologyService _synologyService;

        public AvatarsService(ApplicationDbContext context, ISynologyService synologyService)
        {
            _context = context;
            _synologyService = synologyService;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            var avatars = await _context.Avatars.OrderBy(a => a.Id).ToListAsync();
            return avatars.Select(a => (object)new { a.Id, a.StoryId, AvatarUrl = $"/api/avatars/{a.Id}/image" });
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            var avatar = await _context.Avatars.Where(a => a.Id == id).FirstOrDefaultAsync();
            if (avatar == null) return null;
            return new { avatar.Id, avatar.StoryId, AvatarUrl = $"/api/avatars/{avatar.Id}/image" };
        }

        public async Task<(byte[] Bytes, string ContentType)> GetImageAsync(Guid id)
        {
            var avatar = await _context.Avatars.FindAsync(id);
            if (avatar == null) throw new KeyNotFoundException("Avatar no encontrado.");
            return await _synologyService.DownloadBySharingUrlAsync(avatar.AvatarUrl);
        }

        public async Task<string> CreateAsync(IFormFile avatarImage, Guid? storyId)
        {
            if (avatarImage == null || avatarImage.Length == 0)
                throw new ArgumentException("Archivo vacío.");

            var avatarId = Guid.NewGuid();

            // El servicio central se encarga de todo el flujo del NAS
            string folderPath = $"/CPAPPDEV/img/avatars";
            string fileExtension = Path.GetExtension(avatarImage.FileName);
            string fileName = $"{avatarId}_avatar{fileExtension}";
            var imageUrl = await _synologyService.UploadAndShareAsync(avatarImage, folderPath, fileName);

            var newAvatar = new Avatar { Id = avatarId, StoryId = storyId, AvatarUrl = imageUrl };
            _context.Avatars.Add(newAvatar);
            await _context.SaveChangesAsync();
            return "Avatar creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, IFormFile? avatarImage, Guid? storyId)
        {
            var avatar = await _context.Avatars.FindAsync(id);
            if (avatar == null) throw new KeyNotFoundException("Avatar no encontrado.");

            if (avatarImage != null && avatarImage.Length > 0)
            {
                // Eliminamos la imagen previa en el NAS pasándole su URL guardada
                string folderPath = $"/CPAPPDEV/img/avatars";
                await _synologyService.DeleteBySharingUrlAsync(avatar.AvatarUrl);
                string fileExtension = Path.GetExtension(avatarImage.FileName);
                string fileName = $"{id}_avatar{fileExtension}";
                // Subimos la nueva
                avatar.AvatarUrl = await _synologyService.UploadAndShareAsync(avatarImage, folderPath, fileName);
            }

            avatar.StoryId = storyId;
            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var avatar = await _context.Avatars.FindAsync(id);
            if (avatar == null) throw new KeyNotFoundException("Avatar no encontrado.");

            await _synologyService.DeleteBySharingUrlAsync(avatar.AvatarUrl);

            _context.Avatars.Remove(avatar);
            await _context.SaveChangesAsync();
            return "Avatar eliminado correctamente.";
        }
    }
}