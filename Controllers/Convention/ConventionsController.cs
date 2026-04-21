using LocalRAG.Interfaces;
using LocalRAG.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LocalRAG.DTOs.ConventionModels;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/conventions")]
[Authorize]
public class ConventionsController : ControllerBase
{
    private readonly IConventionCrudService _conventionCrudService;
    private readonly IAttributeCategoryService _attributeCategoryService;

    public ConventionsController(
        IConventionCrudService conventionCrudService,
        IAttributeCategoryService attributeCategoryService)
    {
        _conventionCrudService = conventionCrudService;
        _attributeCategoryService = attributeCategoryService;
    }

    // GET: api/conventions
    [HttpGet]
    public async Task<IActionResult> GetConventions([FromQuery] bool includeDeleted = false)
    {
        var conventions = await _conventionCrudService.GetConventionsAsync(includeDeleted);
        return Ok(conventions);
    }

    [HttpGet("my-conventions")]
    public async Task<IActionResult> GetUserConventions()
    {
        var userId = User.GetUserIdOrNull();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated or user ID is invalid." });
        }

        var conventions = await _conventionCrudService.GetMyConventionsAsync(userId.Value);
        return Ok(conventions);
    }

    // GET: api/conventions/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConvention(int id)
    {
        var result = await _conventionCrudService.GetConventionAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        return Ok(result);
    }

    // POST: api/conventions
    [HttpPost]
    public async Task<IActionResult> CreateConvention([FromBody] CreateConventionRequest request)
    {
        var userId = User.GetUserId().ToString();
        var result = await _conventionCrudService.CreateConventionAsync(request, userId);

        // CreatedAtAction 응답을 위해 Id를 추출
        var idProp = result.GetType().GetProperty("Id");
        var id = idProp?.GetValue(result);

        return CreatedAtAction(nameof(GetConvention), new { id }, result);
    }

    // PUT: api/conventions/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConvention(int id, [FromBody] UpdateConventionRequest request)
    {
        var result = await _conventionCrudService.UpdateConventionAsync(id, request);
        if (result == null)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        return Ok(result);
    }

    // DELETE: api/conventions/5 (Soft Delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteConvention(int id)
    {
        var success = await _conventionCrudService.SoftDeleteConventionAsync(id);
        if (!success)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        return Ok(new { message = "행사가 삭제되었습니다." });
    }

    // POST: api/conventions/5/complete
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteConvention(int id)
    {
        var result = await _conventionCrudService.ToggleCompleteAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        return Ok(result);
    }

    // POST: api/conventions/5/restore
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreConvention(int id)
    {
        var success = await _conventionCrudService.RestoreConventionAsync(id);
        if (!success)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        return Ok(new { message = "행사가 복원되었습니다." });
    }

    // GET: api/conventions/{conventionId}/my-attributes/grouped
    [HttpGet("{conventionId}/my-attributes/grouped")]
    public async Task<IActionResult> GetMyGroupedAttributes(int conventionId)
    {
        var userId = User.GetUserIdOrNull();
        if (userId == null)
            return Unauthorized(new { message = "인증이 필요합니다." });

        var result = await _attributeCategoryService.GetGroupedAttributesAsync(conventionId, userId.Value);
        return Ok(result);
    }
}
