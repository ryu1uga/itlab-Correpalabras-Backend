using CorrePalabras.Data;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorrePalabras.Services
{
    public class StoryLanguagesService : IStoryLanguagesService
    {
        private readonly ApplicationDbContext _context;

        public StoryLanguagesService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.StoryLanguages
                .OrderBy(sl => sl.Id)
                .Select(sl => new { sl.Id, sl.StoryId, sl.LanguageId })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.StoryLanguages
                .Where(sl => sl.Id == id)
                .Select(sl => new { sl.Id, sl.StoryId, sl.LanguageId })
                .FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(StoryLanguageDTO dto)
        {
            var entity = new StoryLanguage
            {
                Id = Guid.NewGuid(),
                StoryId = dto.StoryId,
                LanguageId = dto.LanguageId
            };
            _context.StoryLanguages.Add(entity);
            await _context.SaveChangesAsync();
            return "Relación cuento-idioma creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, StoryLanguageDTO dto)
        {
            var entity = await _context.StoryLanguages.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            entity.StoryId = dto.StoryId;
            entity.LanguageId = dto.LanguageId;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var entity = await _context.StoryLanguages.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            _context.StoryLanguages.Remove(entity);
            await _context.SaveChangesAsync();
            return "Relación cuento-idioma eliminado correctamente.";
        }
    }
}