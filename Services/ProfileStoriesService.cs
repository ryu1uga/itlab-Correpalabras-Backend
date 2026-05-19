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
    public class ProfileStoriesService : IProfileStoriesService
    {
        private readonly ApplicationDbContext _context;

        public ProfileStoriesService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.ProfileStories
                .OrderBy(ps => ps.Id)
                .Select(ps => new { ps.Id, ps.StoryLanguageId, ps.ProfileId, ps.IsDownloaded, ps.IsRead, ps.StartTime, ps.EndTime })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.ProfileStories
                .Where(ps => ps.Id == id)
                .Select(ps => new { ps.Id, ps.StoryLanguageId, ps.ProfileId, ps.IsDownloaded, ps.IsRead, ps.StartTime, ps.EndTime })
                .FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(ProfileStoryDTO dto)
        {
            var entity = new ProfileStory
            {
                Id = Guid.NewGuid(),
                StoryLanguageId = dto.StoryLanguageId,
                ProfileId = dto.ProfileId,
                IsDownloaded = dto.IsDownloaded,
                IsRead = dto.IsRead,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };
            _context.ProfileStories.Add(entity);
            await _context.SaveChangesAsync();
            return "Relación perfil-cuento creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, ProfileStoryDTO dto)
        {
            var entity = await _context.ProfileStories.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();
            entity.StoryLanguageId = dto.StoryLanguageId;
            entity.ProfileId = dto.ProfileId;
            entity.IsDownloaded = dto.IsDownloaded;
            entity.IsRead = dto.IsRead;
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> UpdateDownloadedAsync(Guid id, bool isDownloaded)
        {
            var entity = await _context.ProfileStories.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();
            entity.IsDownloaded = isDownloaded;
            await _context.SaveChangesAsync();
            return $"Campo 'IsDownloaded' actualizado a {isDownloaded}.";
        }

        public async Task<string> UpdateReadAsync(Guid id, bool isRead)
        {
            var entity = await _context.ProfileStories.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();
            entity.IsRead = isRead;
            await _context.SaveChangesAsync();
            return $"Campo 'IsRead' actualizado a {isRead}.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var entity = await _context.ProfileStories.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();
            _context.ProfileStories.Remove(entity);
            await _context.SaveChangesAsync();
            return "Relación perfil-cuento eliminado correctamente.";
        }
    }
}