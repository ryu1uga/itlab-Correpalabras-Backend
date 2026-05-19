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
    public class BadgesService : IBadgesService
    {
        private readonly ApplicationDbContext _context;

        public BadgesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.Badges
                .OrderBy(b => b.Id)
                .Select(b => new { b.Id, b.Name, b.BadgeUrl })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.Badges
                .Where(b => b.Id == id)
                .Select(b => new { b.Id, b.Name, b.BadgeUrl })
                .FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(BadgeDTO badgeDTO)
        {
            var badge = new Badge
            {
                Id = Guid.NewGuid(),
                Name = badgeDTO.Name,
                BadgeUrl = badgeDTO.BadgeUrl
            };

            _context.Badges.Add(badge);
            await _context.SaveChangesAsync();
            return "Insignia creada correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, BadgeDTO badgeDTO)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) throw new KeyNotFoundException("Insignia no encontrada.");

            badge.Name = badgeDTO.Name;
            badge.BadgeUrl = badgeDTO.BadgeUrl;

            _context.Badges.Update(badge);
            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) throw new KeyNotFoundException("Insignia no encontrada.");

            _context.Badges.Remove(badge);
            await _context.SaveChangesAsync();
            return "Insignia eliminada correctamente.";
        }
    }
}