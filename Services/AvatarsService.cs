using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CorrePalabras.Data;
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
    public class AvatarsService : IAvatarsService
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public AvatarsService(ApplicationDbContext context)
        {
            _context = context;
            var account = new Account(
                Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.Avatars
                .OrderBy(a => a.Id)
                .Select(a => new { a.Id, a.StoryId, a.AvatarUrl })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.Avatars
                .Where(a => a.Id == id)
                .Select(a => new { a.Id, a.StoryId, a.AvatarUrl })
                .FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(IFormFile avatarImage, Guid storyId)
        {
            if (avatarImage == null || avatarImage.Length == 0)
                throw new ArgumentException("No se seleccionó una imagen o el archivo está vacío.");

            var imageUrl = await UploadToCloudinary(avatarImage);

            var newAvatar = new Avatar
            {
                Id = Guid.NewGuid(),
                StoryId = storyId,
                AvatarUrl = imageUrl
            };

            _context.Avatars.Add(newAvatar);
            await _context.SaveChangesAsync();
            return "Avatar creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, IFormFile? avatarImage, Guid storyId)
        {
            var avatar = await _context.Avatars.FindAsync(id);
            if (avatar == null) throw new KeyNotFoundException("Avatar no encontrado.");

            if (avatarImage != null && avatarImage.Length > 0)
            {
                avatar.AvatarUrl = await UploadToCloudinary(avatarImage);
            }

            avatar.StoryId = storyId;
            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var avatar = await _context.Avatars.FindAsync(id);
            if (avatar == null) throw new KeyNotFoundException("Avatar no encontrado.");

            if (!string.IsNullOrEmpty(avatar.AvatarUrl))
            {
                await DeleteFromCloudinary(avatar.AvatarUrl);
            }

            _context.Avatars.Remove(avatar);
            await _context.SaveChangesAsync();
            return "Avatar eliminado correctamente.";
        }

        private async Task<string> UploadToCloudinary(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "corre_palabras_avatars"
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null) throw new Exception(result.Error.Message);
            return result.SecureUrl.ToString();
        }

        private async Task DeleteFromCloudinary(string url)
        {
            var uri = new Uri(url);
            var publicId = string.Join("/", uri.AbsolutePath.Split('/').Skip(5));
            var dotIndex = publicId.IndexOf('.');
            if (dotIndex >= 0) publicId = publicId.Substring(0, dotIndex);

            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            if (result.Error != null) throw new Exception("No se pudo eliminar la imagen de Cloudinary.");
        }
    }
}