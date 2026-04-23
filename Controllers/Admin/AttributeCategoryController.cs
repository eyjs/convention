using LocalRAG.Constants;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AttributeCategoryController : ControllerBase
{
    private readonly IAttributeCategoryService _attributeCategoryService;

    public AttributeCategoryController(IAttributeCategoryService attributeCategoryService)
    {
        _attributeCategoryService = attributeCategoryService;
    }

    // GET api/admin/conventions/{conventionId}/attribute-categories
    [HttpGet("conventions/{conventionId}/attribute-categories")]
    public async Task<IActionResult> GetCategories(int conventionId)
    {
        var categories = await _attributeCategoryService.GetCategoriesAsync(conventionId);
        return Ok(categories);
    }

    // POST api/admin/conventions/{conventionId}/attribute-categories
    [HttpPost("conventions/{conventionId}/attribute-categories")]
    public async Task<IActionResult> CreateCategory(int conventionId, [FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "카테고리 이름은 필수입니다." });

        var category = await _attributeCategoryService.CreateCategoryAsync(conventionId, request.Name, request.Icon, request.AttributeKeys ?? new List<string>(), request.OrderNum);
        return Ok(category);
    }

    // PUT api/admin/attribute-categories/{categoryId}
    [HttpPut("attribute-categories/{categoryId}")]
    public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] UpdateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "카테고리 이름은 필수입니다." });

        try
        {
            var category = await _attributeCategoryService.UpdateCategoryAsync(
                categoryId,
                request.Name,
                request.Icon,
                request.AttributeKeys ?? new List<string>(),
                request.OrderNum
            );
            return Ok(category);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE api/admin/attribute-categories/{categoryId}
    [HttpDelete("attribute-categories/{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        var deleted = await _attributeCategoryService.DeleteCategoryAsync(categoryId);
        if (!deleted)
            return NotFound(new { message = "카테고리를 찾을 수 없습니다." });

        return Ok(new { message = "카테고리가 삭제되었습니다." });
    }

    // GET api/admin/conventions/{conventionId}/attribute-keys
    [HttpGet("conventions/{conventionId}/attribute-keys")]
    public async Task<IActionResult> GetAttributeKeys(int conventionId)
    {
        var keys = await _attributeCategoryService.GetAttributeKeysAsync(conventionId);
        return Ok(keys);
    }

    // PUT api/admin/conventions/{conventionId}/attribute-categories/reorder
    [HttpPut("conventions/{conventionId}/attribute-categories/reorder")]
    public async Task<IActionResult> ReorderCategories(int conventionId, [FromBody] ReorderCategoriesRequest request)
    {
        if (request.CategoryIds == null || request.CategoryIds.Count == 0)
            return BadRequest(new { message = "정렬할 카테고리 ID 목록이 필요합니다." });

        await _attributeCategoryService.ReorderCategoriesAsync(request.CategoryIds);
        return Ok(new { message = "카테고리 순서가 변경되었습니다." });
    }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public List<string>? AttributeKeys { get; set; }
    public int? OrderNum { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public List<string>? AttributeKeys { get; set; }
    public int? OrderNum { get; set; }
}

public class ReorderCategoriesRequest
{
    public List<int> CategoryIds { get; set; } = new();
}
