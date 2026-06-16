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
        private readonly ISynologyService _synologyService;

        public PagesService(ApplicationDbContext context, ISynologyService synologyService)
        {
            _context = context;
            _synologyService = synologyService;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            var pages = await _context.Pages
                .OrderBy(p => p.Id)
                .Select(p => new { p.Id, p.StoryId, p.PageOrder })
                .ToListAsync();
            return pages.Select(p => (object)new { p.Id, p.StoryId, p.PageOrder, ImageUrl = $"/api/pages/{p.Id}/image" });
        }

        public async Task<PageDTO?> GetByIdAsync(Guid id)
        {
            var p = await _context.Pages
                .Where(p => p.Id == id)
                .Select(p => new { p.Id, p.StoryId, p.PageOrder })
                .FirstOrDefaultAsync();
            if (p == null) return null;
            return new PageDTO { Id = p.Id, StoryId = p.StoryId, PageOrder = p.PageOrder, ImageUrl = $"/api/pages/{p.Id}/image" };
        }

        public async Task<(byte[] Bytes, string ContentType)> GetImageAsync(Guid id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) throw new KeyNotFoundException("Página no encontrada.");
            return await _synologyService.DownloadBySharingUrlAsync(page.ImageUrl);
        }

        public async Task<PageDTO> CreateAsync(PageDTO pageDTO, IFormFile? imageFile)
        {
            var page = new CorrePalabras.Models.Common.Page
            {
                Id = Guid.NewGuid(),
                StoryId = pageDTO.StoryId,
                PageOrder = pageDTO.PageOrder,
                ImageUrl = pageDTO.ImageUrl
            };

            if (imageFile != null && imageFile.Length > 0)
            {
                string folderPath = $"/CPAPPDEV/img/stories/{pageDTO.StoryId}/pages";
                string fileExtension = Path.GetExtension(imageFile.FileName);
                string fileName = $"{pageDTO.StoryId}_page{pageDTO.PageOrder}{fileExtension}";
                page.ImageUrl = await _synologyService.UploadAndShareAsync(imageFile, folderPath, fileName);
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
                string folderPath = $"/CPAPPDEV/img/stories/{pageDTO.StoryId}/pages";
                await _synologyService.DeleteBySharingUrlAsync(page.ImageUrl);
                string fileExtension = Path.GetExtension(imageFile.FileName);
                string fileName = $"{pageDTO.StoryId}_page{pageDTO.PageOrder}{fileExtension}";
                page.ImageUrl = await _synologyService.UploadAndShareAsync(imageFile, folderPath, fileName);
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
                await _synologyService.DeleteBySharingUrlAsync(page.ImageUrl);
            }

            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();
            return "Página eliminada correctamente.";
        }
    }
}