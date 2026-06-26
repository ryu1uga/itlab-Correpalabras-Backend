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
    public class UnlockedBadgesService : IUnlockedBadgesService
    {
        private readonly ApplicationDbContext _context;

        public UnlockedBadgesService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.UnlockedBadges
                .OrderBy(ub => ub.Id)
                .Select(ub => new { ub.Id, ub.ProfileId, ub.BadgeId })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.UnlockedBadges
                .Where(ub => ub.Id == id)
                .Select(ub => new { ub.Id, ub.ProfileId, ub.BadgeId })
                .FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(UnlockedBadgeRequest dto)
        {
            var entity = new UnlockedBadge
            {
                Id = Guid.NewGuid(),
                ProfileId = dto.ProfileId,
                BadgeId = dto.BadgeId
            };
            _context.UnlockedBadges.Add(entity);
            await _context.SaveChangesAsync();
            return "Insignia desbloqueada creada correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, UnlockedBadgeRequest dto)
        {
            var entity = await _context.UnlockedBadges.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            entity.ProfileId = dto.ProfileId;
            entity.BadgeId = dto.BadgeId;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var entity = await _context.UnlockedBadges.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            _context.UnlockedBadges.Remove(entity);
            await _context.SaveChangesAsync();
            return "Insignia desbloqueada eliminada correctamente.";
        }
    }
}