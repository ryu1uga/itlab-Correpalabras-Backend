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
    public class UnlockedAvatarsService : IUnlockedAvatarsService
    {
        private readonly ApplicationDbContext _context;

        public UnlockedAvatarsService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.UnlockedAvatars
                .OrderBy(ua => ua.Id)
                .Select(ua => new { ua.Id, ua.ProfileId, ua.AvatarId })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.UnlockedAvatars
                .Where(ua => ua.Id == id)
                .Select(ua => new { ua.Id, ua.ProfileId, ua.AvatarId })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<object>> GetByProfileAsync(Guid profileId)
        {
            return await _context.UnlockedAvatars
                .Where(ua => ua.ProfileId == profileId)
                .OrderBy(ua => ua.Id)
                .Select(ua => new { ua.Id, ua.ProfileId, ua.AvatarId })
                .ToListAsync();
        }

        public async Task<string> CreateAsync(UnlockedAvatarRequest dto)
        {
            var entity = new UnlockedAvatar
            {
                Id = Guid.NewGuid(),
                ProfileId = dto.ProfileId,
                AvatarId = dto.AvatarId
            };
            _context.UnlockedAvatars.Add(entity);
            await _context.SaveChangesAsync();
            return "Avatar desbloqueado creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, UnlockedAvatarRequest dto)
        {
            var entity = await _context.UnlockedAvatars.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            entity.ProfileId = dto.ProfileId;
            entity.AvatarId = dto.AvatarId;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var entity = await _context.UnlockedAvatars.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            _context.UnlockedAvatars.Remove(entity);
            await _context.SaveChangesAsync();
            return "Avatar desbloqueado eliminado correctamente.";
        }
    }
}