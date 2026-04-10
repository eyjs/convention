using LocalRAG.Constants;
using LocalRAG.DTOs.SeatingModels;
using LocalRAG.Extensions;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api")]
public class AdminSeatingController : ControllerBase
{
    private readonly ISeatingLayoutService _service;
    private readonly IFileUploadService _fileUploadService;

    public AdminSeatingController(ISeatingLayoutService service, IFileUploadService fileUploadService)
    {
        _service = service;
        _fileUploadService = fileUploadService;
    }

    // ===== 관리자 =====
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/conventions/{conventionId:int}/seating-layouts")]
    public async Task<IActionResult> GetByConvention(int conventionId)
        => Ok(await _service.GetByConventionAsync(conventionId));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/seating-layouts/{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var layout = await _service.GetByIdAsync(id);
        return layout == null ? NotFound() : Ok(layout);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/conventions/{conventionId:int}/seating-layouts")]
    public async Task<IActionResult> Create(int conventionId, [FromBody] CreateSeatingLayoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "이름을 입력해주세요." });
        var created = await _service.CreateAsync(conventionId, request);
        return Ok(created);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("admin/seating-layouts/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSeatingLayoutRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        return updated == null ? NotFound() : Ok(updated);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("admin/seating-layouts/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? Ok(new { message = "삭제되었습니다." }) : NotFound();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/seating-layouts/{id:int}/duplicate")]
    public async Task<IActionResult> Duplicate(int id)
    {
        var copy = await _service.DuplicateAsync(id);
        return copy == null ? NotFound() : Ok(copy);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/seating-layouts/{id:int}/background")]
    public async Task<IActionResult> UploadBackground(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "파일을 선택해주세요." });

        var result = await _fileUploadService.UploadImageAsync(file, "seating");
        var updated = await _service.SetBackgroundAsync(id, result.Url);
        return updated == null ? NotFound() : Ok(updated);
    }

    // ===== 일반 사용자 =====
    [Authorize]
    [HttpGet("seating-layouts/my/{conventionId:int}")]
    public async Task<IActionResult> GetMyLayouts(int conventionId)
    {
        var userId = User.GetUserIdOrNull();
        if (userId == null) return Unauthorized();
        var list = await _service.GetMyLayoutsAsync(conventionId, userId.Value);
        return Ok(list);
    }

    [Authorize]
    [HttpGet("seating-layouts/{id:int}/view")]
    public async Task<IActionResult> ViewLayout(int id)
    {
        var layout = await _service.GetByIdAsync(id);
        return layout == null ? NotFound() : Ok(layout);
    }
}
