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
    public class LanguagesService : ILanguagesService
    {
        private readonly ApplicationDbContext _context;

        public LanguagesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.Languages
                .OrderBy(l => l.Id)
                .Select(l => new { l.Id, l.Name })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            var language = await _context.Languages.FirstOrDefaultAsync(l => l.Id == id);
            if (language == null) return null;

            language.Counter++;
            await _context.SaveChangesAsync();

            return new { language.Id, language.Name, language.Counter };
        }

        public async Task<IEnumerable<object>> GetMostDemandedAsync()
        {
            return await _context.Languages
                .OrderByDescending(l => l.Counter)
                .Take(5)
                .Select(l => new { LanguageId = l.Id, LanguageName = l.Name, Requests = l.Counter })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetMostDemandedByAgeRangeAsync(int minAge, int maxAge)
        {
            var today = DateTime.UtcNow.Date;
            var minBirthDate = today.AddYears(-maxAge - 1).AddDays(1);
            var maxBirthDate = today.AddYears(-minAge);

            return await _context.ProfileStories
                .Where(ps => ps.Profile.BirthDate >= minBirthDate && ps.Profile.BirthDate <= maxBirthDate)
                .Select(ps => ps.StoryLanguage.Language)
                .Distinct()
                .OrderByDescending(l => l.Counter)
                .Take(5)
                .Select(l => new { LanguageId = l.Id, LanguageName = l.Name, Requests = l.Counter })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetMostDemandedByGenderAsync(string gender)
        {
            return await _context.ProfileStories
                .Where(ps => ps.Profile.Gender.ToLower() == gender.ToLower())
                .Select(ps => ps.StoryLanguage.Language)
                .Distinct()
                .OrderByDescending(l => l.Counter)
                .Take(5)
                .Select(l => new { LanguageId = l.Id, LanguageName = l.Name, Requests = l.Counter })
                .ToListAsync();
        }

        public async Task<string> CreateAsync(LanguageDTO languageDTO)
        {
            var language = new Language
            {
                Id = Guid.NewGuid(),
                Name = languageDTO.Name
            };
            _context.Languages.Add(language);
            await _context.SaveChangesAsync();
            return "Idioma creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, LanguageDTO languageDTO)
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null) throw new KeyNotFoundException();

            language.Name = languageDTO.Name;
            _context.Languages.Update(language);
            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null) throw new KeyNotFoundException();

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();
            return "Idioma eliminado correctamente.";
        }
    }
}