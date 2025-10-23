using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Models;
using LocalRAG.Models.DTOs;

namespace LocalRAG.Services.Convention
{
    public class NoticeCategoryService : INoticeCategoryService
    {
        private readonly ConventionDbContext _context;

        public NoticeCategoryService(ConventionDbContext context)
        {
            _context = context;
        }

        public async Task<List<NoticeCategoryDto>> GetCategoriesAsync(int conventionId)
        {
            return await _context.NoticeCategories
                .Where(c => c.ConventionId == conventionId && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new NoticeCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<NoticeCategoryDto> GetCategoryAsync(int id)
        {
            var category = await _context.NoticeCategories
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new NoticeCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            return category;
        }

        public async Task<NoticeCategoryDto> CreateCategoryAsync(CreateNoticeCategoryDto dto)
        {
            var category = new NoticeCategory
            {
                Name = dto.Name,
                ConventionId = dto.ConventionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.NoticeCategories.Add(category);
            await _context.SaveChangesAsync();

            return new NoticeCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<NoticeCategoryDto> UpdateCategoryAsync(int id, UpdateNoticeCategoryDto dto)
        {
            var category = await _context.NoticeCategories.FindAsync(id);

            if (category == null || category.IsDeleted)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            category.Name = dto.Name;
            await _context.SaveChangesAsync();

            return new NoticeCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.NoticeCategories.FindAsync(id);

            if (category == null || category.IsDeleted)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            category.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
