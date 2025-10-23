using System.Collections.Generic;
using System.Threading.Tasks;
using LocalRAG.Models.DTOs;

namespace LocalRAG.Interfaces
{
    public interface INoticeCategoryService
    {
        Task<List<NoticeCategoryDto>> GetCategoriesAsync(int conventionId);
        Task<NoticeCategoryDto> GetCategoryAsync(int id);
        Task<NoticeCategoryDto> CreateCategoryAsync(CreateNoticeCategoryDto dto);
        Task<NoticeCategoryDto> UpdateCategoryAsync(int id, UpdateNoticeCategoryDto dto);
        Task DeleteCategoryAsync(int id);
    }
}
