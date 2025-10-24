using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.Interfaces;
using LocalRAG.DTOs.NoticeModels;

namespace LocalRAG.Controllers.Convention
{
    [ApiController]
    [Route("api/admin/conventions/{conventionId}/notice-categories")]
    public class NoticeCategoriesController : ControllerBase
    {
        private readonly INoticeCategoryService _categoryService;
    
        public NoticeCategoriesController(INoticeCategoryService categoryService)
        {
            _categoryService = categoryService;
        }
    
        [HttpGet]
        public async Task<ActionResult<List<NoticeCategoryDto>>> GetCategories(int conventionId)
        {
            var categories = await _categoryService.GetCategoriesAsync(conventionId);
            return Ok(categories);
        }
    
        [HttpGet("{id}")]
        public async Task<ActionResult<NoticeCategoryDto>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryAsync(id);
                return Ok(category);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NoticeCategoryDto>> CreateCategory(int conventionId, [FromBody] CreateNoticeCategoryDto dto)
        {
            if (conventionId != dto.ConventionId)
            {
                return BadRequest("Convention ID mismatch.");
            }
            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategory), new { conventionId = conventionId, id = category.Id }, category);
        }
    
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NoticeCategoryDto>> UpdateCategory(int id, [FromBody] UpdateNoticeCategoryDto dto)
        { 
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, dto);
                return Ok(category);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }}
