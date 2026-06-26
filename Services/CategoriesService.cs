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
    public class CategoriesService : ICategoriesService
    {
        private readonly ApplicationDbContext _context;

        public CategoriesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAllAsync(bool isAdmin)
        {
            var query = _context.Categories.AsQueryable();
            
            if (!isAdmin)
                query = query.Where(c => c.Code != "INV");

            return await query
                .OrderBy(c => c.CategoryOrder)
                .Select(c => new { c.Id, c.Name, c.Code, c.CategoryOrder })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            category.Counter++;
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<IEnumerable<object>> GetMostVisitedAsync()
        {
            return await _context.Categories
                .Where(c => c.Code != "INV")
                .OrderByDescending(c => c.Counter)
                .Take(5)
                .Select(c => new { CategoryId = c.Id, CategoryName = c.Name, Views = c.Counter })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetMostVisitedByAgeRangeAsync(int minAge, int maxAge)
        {
            var today = DateTime.UtcNow.Date;
            var minBirthDate = today.AddYears(-maxAge - 1).AddDays(1);
            var maxBirthDate = today.AddYears(-minAge);

            return await _context.ProfileStories
                .Where(ps => ps.Profile.BirthDate >= minBirthDate && ps.Profile.BirthDate <= maxBirthDate)
                .SelectMany(ps => ps.StoryLanguage.Story.StoryCategories.Select(sc => sc.Category))
                .Where(c => c.Code != "INV")
                .Distinct()
                .OrderByDescending(c => c.Counter)
                .Take(5)
                .Select(c => new { CategoryId = c.Id, CategoryName = c.Name, Views = c.Counter })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetMostVisitedByGenderAsync(string gender)
        {
            return await _context.ProfileStories
                .Where(ps => ps.Profile.Gender.ToLower() == gender.ToLower())
                .SelectMany(ps => ps.StoryLanguage.Story.StoryCategories.Select(sc => sc.Category))
                .Where(c => c.Code != "INV")
                .Distinct()
                .OrderByDescending(c => c.Counter)
                .Take(5)
                .Select(c => new { CategoryId = c.Id, CategoryName = c.Name, Views = c.Counter })
                .ToListAsync();
        }

        public async Task<string> CreateAsync(CategoryRequest dto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                CategoryOrder = dto.CategoryOrder
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return "Categoría creada correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, CategoryRequest dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new KeyNotFoundException();

            category.Name = dto.Name;
            category.Code = dto.Code;
            category.CategoryOrder = dto.CategoryOrder;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new KeyNotFoundException();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return "Categoría eliminada correctamente.";
        }
    }
}