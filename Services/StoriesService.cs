using CorrePalabras.Data;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorrePalabras.Services
{
    public class StoriesService : IStoriesService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISynologyService _synologyService;

        public StoriesService(ApplicationDbContext context, ISynologyService synologyService)
        {
            _context = context;
            _synologyService = synologyService;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            var spanish = await _context.Languages.FirstOrDefaultAsync(l => l.Name.ToLower() == "español");

            return await _context.Stories
                .OrderBy(s => s.Id)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Thumbnail,
                    StoryCategories = s.StoryCategories
                        .Where(sc => sc.Category.Code != "INV")
                        .Select(sc => new { sc.CategoryId }).ToList(),
                    TotalWords = (spanish != null && s.Pages.SelectMany(p => p.PageContents).Any(pc => pc.LanguageId == spanish.Id))
                        ? s.Pages.SelectMany(p => p.PageContents).Where(pc => pc.LanguageId == spanish.Id).Sum(pc => pc.CountWords)
                        : s.Pages.SelectMany(p => p.PageContents).GroupBy(pc => pc.LanguageId).Select(g => g.Sum(pc => pc.CountWords)).FirstOrDefault()
                }).ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id, Guid userId)
        {
            var story = await _context.Stories
                .Include(s => s.StoryCategories).ThenInclude(sc => sc.Category)
                .Include(s => s.StoryLanguages)
                .Include(s => s.Pages).ThenInclude(p => p.PageContents)
                .Include(s => s.Attachments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (story == null) return null;

            if (userId != Guid.Parse("7267d866-4506-4983-8143-1dbe25216700"))
            {
                story.Counter++;
                await _context.SaveChangesAsync();
            }

            return new
            {
                story.Id,
                story.Author,
                story.Illustrator,
                story.Title,
                story.CountPages,
                story.Thumbnail,
                story.UpdatedAt,
                StoryCategories = story.StoryCategories.Where(sc => sc.Category.Code != "INV").Select(sc => new { sc.Id, sc.CategoryId }).ToList(),
                StoryLanguages = story.StoryLanguages.Select(sl => new { sl.Id, sl.LanguageId }).ToList(),
                Pages = story.Pages.Select(p => new {
                    p.Id, p.PageOrder, p.ImageUrl,
                    PageContents = p.PageContents.Select(pc => new { pc.Id, pc.PageId, pc.LanguageId, pc.CountWords, pc.Content }).ToList()
                }).ToList(),
                Attachments = story.Attachments.Select(a => new { a.Id, a.ImageUrl, a.TypeImage, a.Position, a.OrderAttachments, a.LanguageId }).ToList()
            };
        }

        public async Task<IEnumerable<object>> GetByCategoryAsync(Guid categoryId, string? orderedBy)
        {
            var query = _context.Stories.Where(s => s.StoryCategories.Any(c => c.CategoryId == categoryId));

            query = orderedBy?.ToLower() switch
            {
                "title" => query.OrderBy(s => s.Title),
                "updatedat" => query.OrderByDescending(s => s.UpdatedAt),
                "counter" => query.OrderByDescending(s => s.Counter),
                _ => query.OrderBy(s => s.Pages.SelectMany(p => p.PageContents).Sum(pc => pc.CountWords))
            };

            return await query.Select(s => new {
                s.Id, s.Author, s.Illustrator, s.Title, s.CountPages, s.Thumbnail, s.UpdatedAt,
                StoryCategories = s.StoryCategories.Select(sc => new { sc.CategoryId }).ToList(),
                StoryLanguages = s.StoryLanguages.Select(sl => new { sl.LanguageId }).ToList(),
                Pages = s.Pages.Select(p => new {
                    p.Id, p.PageOrder, p.ImageUrl,
                    PageContents = p.PageContents.Select(pc => new { pc.Id, pc.PageId, pc.LanguageId, pc.CountWords, pc.Content }).ToList()
                }).ToList(),
                Attachments = s.Attachments.Select(a => new { a.Id, a.ImageUrl, a.TypeImage, a.Position, a.OrderAttachments }).ToList()
            }).ToListAsync();
        }

        public async Task<object?> GetRandomStoryAsync()
        {
            var query = _context.Stories.Where(s => s.StoryCategories.Any(sc => sc.Category.Code == "INV"));
            int count = await query.CountAsync();
            if (count == 0) return null;

            int index = new Random().Next(0, count);
            return await query.Skip(index).Take(1).Select(s => new {
                s.Id, s.Title, s.Thumbnail,
                StoryCategories = s.StoryCategories.Select(sc => new { sc.CategoryId }).ToList()
            }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<object>> GetMostReadAsync() => 
            await _context.Stories.OrderByDescending(s => s.Counter).Take(5)
            .Select(s => new { StoryId = s.Id, s.Title, Reads = s.Counter }).ToListAsync();

        public async Task<IEnumerable<object>> GetMostReadByAgeRangeAsync(int minAge, int maxAge)
        {
            var today = DateTime.UtcNow.Date;
            var minBirthDate = today.AddYears(-maxAge - 1).AddDays(1);
            var maxBirthDate = today.AddYears(-minAge);

            return await _context.ProfileStories
                .Where(ps => ps.Profile.BirthDate >= minBirthDate && ps.Profile.BirthDate <= maxBirthDate)
                .Select(ps => ps.StoryLanguage.Story).Distinct().OrderByDescending(s => s.Counter).Take(5)
                .Select(s => new { StoryId = s.Id, s.Title, Reads = s.Counter }).ToListAsync();
        }

        public async Task<IEnumerable<object>> GetMostReadByGenderAsync(string gender) =>
            await _context.ProfileStories
                .Where(ps => ps.Profile.Gender.ToLower() == gender.ToLower())
                .Select(ps => ps.StoryLanguage.Story).Distinct().OrderByDescending(s => s.Counter).Take(5)
                .Select(s => new { StoryId = s.Id, s.Title, Reads = s.Counter }).ToListAsync();

        public async Task<StoryDTO> CreateAsync(StoryDTO dto, IFormFile thumbnail)
        {
            var storyId = Guid.NewGuid();
            string folderPath = $"/CPAPPDEV/img/stories/{storyId}";
            string fileExtension = Path.GetExtension(thumbnail.FileName);
            string fileName = $"{storyId}_thumbnail{fileExtension}";
            var imageUrl = await _synologyService.UploadAndShareAsync(thumbnail, folderPath, fileName);
            var story = new Story {
                Id = storyId, Author = dto.Author, Illustrator = dto.Illustrator,
                Title = dto.Title, CountPages = dto.CountPages, Thumbnail = imageUrl, UpdatedAt = DateTime.UtcNow
            };

            _context.Stories.Add(story);
            var allCat = await _context.Categories.FirstOrDefaultAsync(c => c.Code == "ALL");
            if (allCat != null) _context.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = allCat.Id });
            
            await _context.SaveChangesAsync();
            dto.Id = story.Id; dto.Thumbnail = imageUrl;
            return dto;
        }

        public async Task<string> UpdateAsync(Guid id, StoryDTO dto, IFormFile? thumbnail)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story == null) throw new KeyNotFoundException();

            if (thumbnail != null && thumbnail.Length > 0)
            {
                string folderPath = $"/CPAPPDEV/img/stories/{id}";
                await _synologyService.DeleteBySharingUrlAsync(story.Thumbnail);
                string fileExtension = Path.GetExtension(thumbnail.FileName);
                string fileName = $"{id}_thumbnail{fileExtension}";
                story.Thumbnail = await _synologyService.UploadAndShareAsync(thumbnail, folderPath, fileName);
            }

            story.Author = dto.Author; story.Illustrator = dto.Illustrator;
            story.Title = dto.Title; story.CountPages = dto.CountPages;
            story.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var story = await _context.Stories
                .Include(s => s.Pages).Include(s => s.StoryCategories)
                .Include(s => s.StoryLanguages).Include(s => s.Attachments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (story == null) throw new KeyNotFoundException();

            if (!string.IsNullOrEmpty(story.Thumbnail)) 
            {
                await _synologyService.DeleteBySharingUrlAsync(story.Thumbnail);
            }

            foreach (var p in story.Pages.Where(p => !string.IsNullOrEmpty(p.ImageUrl))) 
            {
                await _synologyService.DeleteBySharingUrlAsync(p.ImageUrl);
            }

            foreach (var a in story.Attachments.Where(a => !string.IsNullOrEmpty(a.ImageUrl))) 
            {
                await _synologyService.DeleteBySharingUrlAsync(a.ImageUrl);
            }

            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
            return "Cuento eliminado correctamente.";
        }
    }
}