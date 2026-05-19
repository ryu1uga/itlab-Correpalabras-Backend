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
    public class PagesService : IPagesService
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public PagesService(ApplicationDbContext context)
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
            return await _context.Pages
                .OrderBy(p => p.Id)
                .Select(p => new { p.Id, p.StoryId, p.PageOrder, p.ImageUrl })
                .ToListAsync();
        }

        public async Task<PageDTO?> GetByIdAsync(Guid id)
        {
            return await _context.Pages
                .Where(p => p.Id == id)
                .Select(p => new PageDTO { Id = p.Id, StoryId = p.StoryId, PageOrder = p.PageOrder, ImageUrl = p.ImageUrl })
                .FirstOrDefaultAsync();
        }

        public async Task<PageDTO> CreateAsync(PageDTO pageDTO, IFormFile? imageFile)
        {
            var page = new CorrePalabras.Models.Common.Page
            {
                Id = Guid.NewGuid(),
                StoryId = pageDTO.StoryId,
                PageOrder = pageDTO.PageOrder,
                ImageUrl = pageDTO.ImageUrl // URL por defecto si existe
            };

            if (imageFile != null && imageFile.Length > 0)
            {
                page.ImageUrl = await UploadToCloudinary(imageFile);
            }

            _context.Pages.Add(page);
            await _context.SaveChangesAsync();

            pageDTO.Id = page.Id;
            pageDTO.ImageUrl = page.ImageUrl;
            return pageDTO;
        }

        public async Task<string> UpdateAsync(Guid id, PageDTO pageDTO, IFormFile? imageFile)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) throw new KeyNotFoundException("Página no encontrada.");

            if (imageFile != null && imageFile.Length > 0)
            {
                page.ImageUrl = await UploadToCloudinary(imageFile);
            }

            page.StoryId = pageDTO.StoryId;
            page.PageOrder = pageDTO.PageOrder;

            _context.Pages.Update(page);
            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var page = await _context.Pages.Include(p => p.PageContents).FirstOrDefaultAsync(p => p.Id == id);
            if (page == null) throw new KeyNotFoundException("Página no encontrada.");

            if (!string.IsNullOrEmpty(page.ImageUrl))
            {
                await DeleteFromCloudinary(page.ImageUrl);
            }

            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();
            return "Página eliminada correctamente.";
        }

        private async Task<string> UploadToCloudinary(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "corre_palabras_pages"
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
            if (result.Error != null) throw new Exception("Error al eliminar imagen de Cloudinary.");
        }
    }
}