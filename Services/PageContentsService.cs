using CorrePalabras.Data;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services
{
    public class PageContentsService : IPageContentsService
    {
        private readonly ApplicationDbContext _context;

        public PageContentsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.PageContents
                .OrderBy(pc => pc.Id)
                .Select(pc => new { pc.Id, pc.PageId, pc.LanguageId, pc.CountWords, pc.Content })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.PageContents
                .Where(pc => pc.Id == id)
                .Select(pc => new { pc.Id, pc.PageId, pc.LanguageId, pc.CountWords, pc.Content })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<object>> GetByStoryAsync(Guid storyId, Guid? languageId)
        {
            var query = _context.PageContents.AsQueryable()
                .Where(pc => pc.Page.StoryId == storyId);

            if (languageId.HasValue)
            {
                query = query.Where(pc => pc.LanguageId == languageId.Value);
            }

            return await query
                .OrderBy(pc => pc.Id)
                .Select(pc => new { pc.Id, pc.PageId, pc.LanguageId, pc.CountWords, pc.Content })
                .ToListAsync();
        }

        public async Task<string> CreateAsync(PageContentRequest dto)
        {
            var pageContent = new PageContent
            {
                Id = Guid.NewGuid(),
                PageId = dto.PageId,
                LanguageId = dto.LanguageId,
                CountWords = dto.CountWords,
                Content = dto.Content
            };

            _context.PageContents.Add(pageContent);
            await _context.SaveChangesAsync();
            return "Contenido de página creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, PageContentRequest dto)
        {
            var pageContent = await _context.PageContents.FindAsync(id);
            if (pageContent == null) throw new KeyNotFoundException();

            pageContent.PageId = dto.PageId;
            pageContent.LanguageId = dto.LanguageId;
            pageContent.CountWords = dto.CountWords;
            pageContent.Content = dto.Content;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var pageContent = await _context.PageContents.FindAsync(id);
            if (pageContent == null) throw new KeyNotFoundException();

            _context.PageContents.Remove(pageContent);
            await _context.SaveChangesAsync();
            return "Contenido de página eliminado correctamente.";
        }
    }
}