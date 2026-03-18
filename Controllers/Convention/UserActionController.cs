using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.DTOs.ActionModels;
using LocalRAG.Extensions;
using LocalRAG.Interfaces;
using System.Text.Json;

namespace LocalRAG.Controllers.Convention;

/// <summary>
/// User용 ConventionAction API
/// </summary>
[ApiController]
[Route("api/conventions/{conventionId}/actions")]
public class UserActionController : ControllerBase
{
    private readonly IUserActionService _userActionService;
    private readonly ILogger<UserActionController> _logger;

    public UserActionController(
        IUserActionService userActionService,
        ILogger<UserActionController> logger)
    {
        _userActionService = userActionService;
        _logger = logger;
    }

    /// <summary>
    /// 메인 화면용: 마감기한이 있는 액션 중 기한이 짧은 3개 조회
    /// </summary>
    [HttpGet("urgent")]
    public async Task<ActionResult> GetUrgentActions(int conventionId)
    {
        var actions = await _userActionService.GetUrgentActionsAsync(conventionId);
        return Ok(actions);
    }

    /// <summary>
    /// 더보기 메뉴용: 모든 활성 액션 조회 (마감기한 여부 무관)
    /// 필터링 지원: targetLocation (콤마 구분), actionCategory, isActive
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult> GetAllActions(
        int conventionId,
        [FromQuery] string? targetLocation = null,
        [FromQuery] string? actionCategory = null,
        [FromQuery] bool? isActive = null)
    {
        var actions = await _userActionService.GetAllActionsAsync(conventionId, targetLocation, actionCategory, isActive);
        return Ok(actions);
    }

    [Authorize]
    [HttpGet("statuses")]
    public async Task<ActionResult> GetActionStatuses(int conventionId)
    {
        var userIdNullable = User.GetUserIdOrNull();
        if (userIdNullable == null)
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        var userId = userIdNullable.Value;

        var statuses = await _userActionService.GetUserActionStatusesAsync(conventionId, userId);
        return Ok(statuses);
    }

    [Authorize]
    [HttpGet("checklist")]
    public async Task<ActionResult> GetUserChecklist(int conventionId)
    {
        var userIdNullable = User.GetUserIdOrNull();
        if (userIdNullable == null)
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        var userId = userIdNullable.Value;

        try
        {
            var checklist = await _userActionService.GetUserChecklistAsync(conventionId, userId);
            return Ok(checklist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 체크리스트 조회 중 오류 발생. ConventionId={ConventionId}, UserId={UserId}", conventionId, userId);
            return StatusCode(500, new { message = "체크리스트 조회 중 오류가 발생했습니다." });
        }
    }

    [HttpGet("{actionId}")]
    public async Task<ActionResult> GetActionDetail(int conventionId, int actionId)
    {
        var result = await _userActionService.GetActionDetailAsync(conventionId, actionId);
        if (result == null)
            return NotFound(new { message = "액션을 찾을 수 없습니다." });

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{actionId:int}/complete")]
    public async Task<IActionResult> CompleteAction(int conventionId, int actionId, [FromBody] ActionResponseDto? responseDto = null)
    {
        var userIdNullable = User.GetUserIdOrNull();
        if (userIdNullable == null)
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        var userId = userIdNullable.Value;

        var result = await _userActionService.CompleteActionAsync(conventionId, actionId, userId, responseDto?.ResponseDataJson);
        if (result == null)
            return NotFound(new { message = "액션을 찾을 수 없습니다." });

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{actionId:int}/toggle")]
    public async Task<IActionResult> ToggleAction(int conventionId, int actionId, [FromBody] ToggleActionDto dto)
    {
        var userIdNullable = User.GetUserIdOrNull();
        if (userIdNullable == null)
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        var userId = userIdNullable.Value;

        var result = await _userActionService.ToggleActionAsync(conventionId, actionId, userId, dto.IsComplete);
        if (result == null)
            return NotFound(new { message = "액션을 찾을 수 없습니다." });

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{actionId}/submit")]
    public async Task<IActionResult> SubmitGenericActionData(int conventionId, int actionId, [FromBody] JsonElement payload)
    {
        var userIdNullable = User.GetUserIdOrNull();
        if (userIdNullable == null)
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        var userId = userIdNullable.Value;

        var (result, error, notFound) = await _userActionService.SubmitActionAsync(conventionId, actionId, userId, payload);

        if (notFound)
            return NotFound(new { message = error });
        if (error != null)
            return BadRequest(new { message = error });

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{actionId}/submission")]
    public async Task<IActionResult> GetMySubmission(int conventionId, int actionId)
    {
        var userIdNullable = User.GetUserIdOrNull();
        if (userIdNullable == null)
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        var userId = userIdNullable.Value;

        var result = await _userActionService.GetUserSubmissionAsync(actionId, userId);
        if (result == null)
            return NotFound(new { message = "제출 데이터가 없습니다." });

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{actionId}/submissions/all")]
    public async Task<IActionResult> GetAllSubmissions(int conventionId, int actionId)
    {
        var (result, error, notFound) = await _userActionService.GetAllSubmissionsAsync(conventionId, actionId);

        if (notFound)
            return NotFound(new { message = error });
        if (error != null)
            return BadRequest(new { message = error });

        return Ok(result);
    }

    [Authorize]
    [HttpGet("checklist-status")]
    public async Task<IActionResult> GetChecklistStatus(int conventionId)
    {
        var userIdNullable = User.GetUserIdOrNull();
        if (userIdNullable == null)
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        var userId = userIdNullable.Value;

        var result = await _userActionService.GetChecklistStatusAsync(conventionId, userId);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("menu")]
    public async Task<IActionResult> GetMenuActions(int conventionId)
    {
        var actions = await _userActionService.GetMenuActionsAsync(conventionId);
        return Ok(actions);
    }
}
