using LocalRAG.Constants;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminCompanionController : ControllerBase
{
    private readonly ICompanionService _companionService;

    public AdminCompanionController(ICompanionService companionService)
    {
        _companionService = companionService;
    }

    /// <summary>
    /// 행사의 모든 동반자 관계 조회
    /// </summary>
    [HttpGet("conventions/{conventionId}/companions")]
    public async Task<IActionResult> GetAllCompanionRelations(int conventionId)
    {
        var result = await _companionService.GetAllCompanionRelationsAsync(conventionId);
        return Ok(result);
    }

    /// <summary>
    /// 특정 사용자의 동반자 목록 조회
    /// </summary>
    [HttpGet("conventions/{conventionId}/users/{userId}/companions")]
    public async Task<IActionResult> GetCompanions(int conventionId, int userId)
    {
        var result = await _companionService.GetCompanionsAsync(conventionId, userId);
        return Ok(result);
    }

    /// <summary>
    /// 동반자 관계 추가
    /// </summary>
    [HttpPost("conventions/{conventionId}/users/{userId}/companions")]
    public async Task<IActionResult> AddCompanion(
        int conventionId, int userId, [FromBody] AddCompanionRequest request)
    {
        var (success, result, statusCode) = await _companionService.AddCompanionAsync(
            conventionId, userId, request.CompanionUserId, request.RelationType);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// 동반자 관계 삭제
    /// </summary>
    [HttpDelete("companions/{relationId}")]
    public async Task<IActionResult> RemoveCompanion(int relationId)
    {
        var (success, result, statusCode) = await _companionService.RemoveCompanionAsync(relationId);
        return StatusCode(statusCode, result);
    }
}

public class AddCompanionRequest
{
    public int CompanionUserId { get; set; }
    public string RelationType { get; set; } = string.Empty;
}
