using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.Interfaces;
using LocalRAG.Models.DTOs;

namespace LocalRAG.Controllers.Convention
{
    [ApiController]
    [Route("api/conventions/{conventionId}/notice-categories")]
    public class UserNoticeCategoriesController : ControllerBase
    {
        private readonly INoticeCategoryService _categoryService;

        public UserNoticeCategoriesController(INoticeCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<NoticeCategoryDto>>> GetCategories(int conventionId)
        {
            var categories = await _categoryService.GetCategoriesAsync(conventionId);
            return Ok(categories);
        }
    }
}
