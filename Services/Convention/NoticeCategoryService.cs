using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.DTOs.NoticeModels;
using LocalRAG.Repositories;

namespace LocalRAG.Services.Convention
{
    public class NoticeCategoryService : INoticeCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NoticeCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<NoticeCategoryDto>> GetCategoriesAsync(int conventionId)
        {
            return await _unitOfWork.NoticeCategories.Query
                .Where(c => c.ConventionId == conventionId && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new NoticeCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    NoticeCount = c.Notices.Count(n => !n.IsDeleted)
                })
                .ToListAsync();
        }

        public async Task<NoticeCategoryDto> GetCategoryAsync(int id)
        {
            var category = await _unitOfWork.NoticeCategories.Query
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new NoticeCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    NoticeCount = c.Notices.Count(n => !n.IsDeleted)
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
            var category = new Entities.NoticeCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder,
                ConventionId = dto.ConventionId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.NoticeCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new NoticeCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                NoticeCount = 0
            };
        }

        public async Task<NoticeCategoryDto> UpdateCategoryAsync(int id, UpdateNoticeCategoryDto dto)
        {
            var category = await _unitOfWork.NoticeCategories.Query
                .Include(c => c.Notices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null || category.IsDeleted)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.DisplayOrder = dto.DisplayOrder;
            await _unitOfWork.SaveChangesAsync();

            return new NoticeCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                NoticeCount = category.Notices.Count(n => !n.IsDeleted)
            };
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.NoticeCategories.GetByIdAsync(id);

            if (category == null || category.IsDeleted)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            category.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
