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
    public class ProfilesService : IProfilesService
    {
        private readonly ApplicationDbContext _context;

        public ProfilesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await (from p in _context.Profiles
                          join a in _context.Avatars on p.AvatarId equals a.Id
                          orderby p.Id
                          select new
                          {
                              p.Id,
                              AvatarUrl = a.AvatarUrl,
                              p.Username,
                              p.Gender,
                              p.BirthDate,
                              p.UserId
                          }).ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await (from p in _context.Profiles
                          join a in _context.Avatars on p.AvatarId equals a.Id
                          where p.Id == id
                          select new
                          {
                              p.Id,
                              AvatarUrl = a.AvatarUrl,
                              p.Username,
                              p.Gender,
                              p.BirthDate,
                              p.UserId
                          }).FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Profiles.CountAsync();
        }

        public async Task<int> GetCountByAgeRangeAsync(int minAge, int maxAge)
        {
            var today = DateTime.Today;
            var profiles = await _context.Profiles.Select(p => p.BirthDate).ToListAsync();

            return profiles.Count(birthDate =>
            {
                int age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;
                return age >= minAge && age <= maxAge;
            });
        }

        public async Task<object> GetGenderStatsAsync()
        {
            var genderCounts = await _context.Profiles
                .GroupBy(p => p.Gender)
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToListAsync();

            return new
            {
                Male = genderCounts.FirstOrDefault(g => g.Gender == "Masculino" || g.Gender == "M")?.Count ?? 0,
                Female = genderCounts.FirstOrDefault(g => g.Gender == "Femenino" || g.Gender == "F")?.Count ?? 0
            };
        }

        public async Task<object?> GetStoriesSummaryAsync(Guid profileId)
        {
            var profile = await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == profileId);
            if (profile == null) return null;

            var summary = await _context.ProfileStories
                .Where(ps => ps.Id == profileId)
                .GroupBy(ps => ps.Id)
                .Select(g => new
                {
                    ReadCount = g.Count(ps => ps.IsRead == true),
                    DownloadedCount = g.Count(ps => ps.IsDownloaded == true)
                })
                .FirstOrDefaultAsync();

            return new
            {
                profile.Id,
                profile.Username,
                ReadStories = summary?.ReadCount ?? 0,
                DownloadedStories = summary?.DownloadedCount ?? 0
            };
        }

        public async Task<string> CreateAsync(ProfileDTO profileDTO, Guid userId)
        {
            var profile = new Profile
            {
                Id = Guid.NewGuid(),
                AvatarId = profileDTO.AvatarId,
                Username = profileDTO.Username,
                Gender = profileDTO.Gender,
                BirthDate = profileDTO.BirthDate,
                UserId = userId
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            return "Perfil creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, ProfileDTO dto)
        {
            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null) throw new KeyNotFoundException();

            profile.AvatarId = dto.AvatarId;
            profile.Username = dto.Username;
            profile.Gender = dto.Gender;
            profile.BirthDate = dto.BirthDate;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var profile = await _context.Profiles
                .Include(p => p.UnlockedBadges)
                .Include(p => p.UnlockedAvatars)
                .Include(p => p.ProfileStories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null) throw new KeyNotFoundException();

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();
            return "Perfil eliminado correctamente.";
        }
    }
}