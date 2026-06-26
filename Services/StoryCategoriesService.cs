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
    public class StoryCategoriesService : IStoryCategoriesService
    {
        private readonly ApplicationDbContext _context;

        public StoryCategoriesService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.StoryCategories
                .OrderBy(sc => sc.Id)
                .Select(sc => new { sc.Id, sc.StoryId, sc.CategoryId })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.StoryCategories
                .Where(sc => sc.Id == id)
                .Select(sc => new { sc.Id, sc.StoryId, sc.CategoryId })
                .FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(StoryCategoryRequest dto)
        {
            var entity = new StoryCategory
            {
                Id = Guid.NewGuid(),
                StoryId = dto.StoryId,
                CategoryId = dto.CategoryId
            };
            _context.StoryCategories.Add(entity);
            await _context.SaveChangesAsync();
            return "Relación cuento-categoría creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, StoryCategoryRequest dto)
        {
            var entity = await _context.StoryCategories.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            entity.StoryId = dto.StoryId;
            entity.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var entity = await _context.StoryCategories.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            _context.StoryCategories.Remove(entity);
            await _context.SaveChangesAsync();
            return "Relación cuento-categoría eliminado correctamente.";
        }
    }
}