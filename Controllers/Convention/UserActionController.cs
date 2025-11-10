using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities;
using LocalRAG.Entities.Action;
using System.Security.Claims;
using System.Text.Json;
using LocalRAG.DTOs.ActionModels;
using LocalRAG.Interfaces;

namespace LocalRAG.Controllers.Convention;

/// <summary>
/// User용 ConventionAction API
/// </summary>
[ApiController]
[Route("api/conventions/{conventionId}/actions")]
public class UserActionController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<UserActionController> _logger;
    private readonly IActionOrchestrationService _orchestrationService;

    public UserActionController(
        ConventionDbContext context,
        ILogger<UserActionController> logger,
        IActionOrchestrationService orchestrationService)
    {
        _context = context;
        _logger = logger;
        _orchestrationService = orchestrationService;
    }

    /// <summary>
    /// 메인 화면용: 마감기한이 있는 액션 중 기한이 짧은 3개 조회
    /// </summary>
    [HttpGet("urgent")]
    public async Task<ActionResult> GetUrgentActions(int conventionId)
    {
        var now = DateTime.UtcNow;

        var actions = await _context.ConventionActions
            .Include(a => a.Template)
            .Where(a => a.ConventionId == conventionId &&
                       a.IsActive &&
                       a.Deadline.HasValue &&
                       a.Deadline.Value > now) // 미래 마감기한만
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Deadline,
                a.MapsTo,
                a.IsRequired,
                a.ActionCategory,
                a.TargetLocation,
                a.ConfigJson,
                a.BehaviorType,
                a.TargetId,
                IconClass = a.IconClass ?? (a.Template == null ? null : a.Template.IconClass),
                Category = a.Category ?? (a.Template == null ? null : a.Template.Category)
            })
            .ToListAsync();

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
        var query = _context.ConventionActions
            .Include(a => a.Template)
            .Where(a => a.ConventionId == conventionId);

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }
        else
        {
            query = query.Where(a => a.IsActive);
        }

        if (!string.IsNullOrEmpty(targetLocation))
        {
            var locations = targetLocation.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(l => l.Trim())
                                         .ToList();
            if (locations.Any())
            {
                query = query.Where(a => a.TargetLocation != null && locations.Contains(a.TargetLocation));
            }
        }

        if (!string.IsNullOrEmpty(actionCategory))
        {
            query = query.Where(a => a.ActionCategory == actionCategory);
        }

        var actions = await query
            .OrderBy(a => a.Category)
            .ThenBy(a => a.OrderNum)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Deadline,
                a.MapsTo,
                a.IsRequired,
                a.ActionCategory,
                a.TargetLocation,
                a.ConfigJson,
                a.BehaviorType,
                a.TargetId,
                IconClass = a.IconClass ?? (a.Template == null ? null : a.Template.IconClass),
                Category = a.Category ?? (a.Template == null ? null : a.Template.Category)
            })
            .ToListAsync();

        return Ok(actions);
    }

    [Authorize]
    [HttpGet("statuses")]
    public async Task<ActionResult> GetActionStatuses(int conventionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        var statuses = await _context.UserActionStatuses
            .Where(s => s.UserId == userId && s.ConventionAction != null && s.ConventionAction.ConventionId == conventionId)
            .ToListAsync();

        return Ok(statuses);
    }

    [Authorize]
    [HttpGet("checklist")]
    public async Task<ActionResult> GetUserChecklist(int conventionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        try
        {
            var checklist = await _orchestrationService.GetUserActionsAsync(conventionId, userId);
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
        var action = await _context.ConventionActions
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.ConventionId == conventionId &&
                                    a.Id == actionId &&
                                    a.IsActive);

        if (action == null)
            return NotFound(new { message = "액션을 찾을 수 없습니다." });

        return Ok(new
        {
            action.Id,
            action.Title,
            action.Deadline,
            action.MapsTo,
            action.ConfigJson,
            action.IsRequired,
            action.ActionCategory,
            action.TargetLocation,
            action.BehaviorType,
            action.TargetId,
            IconClass = action.IconClass ?? action.Template?.IconClass,
            Category = action.Category ?? action.Template?.Category
        });
    }

    [Authorize]
    [HttpPost("{actionId:int}/complete")]
    public async Task<IActionResult> CompleteAction(int conventionId, int actionId, [FromBody] ActionResponseDto? responseDto = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        var action = await _context.ConventionActions.FirstOrDefaultAsync(a => a.Id == actionId && a.ConventionId == conventionId);
        if (action == null)
        {
            return NotFound(new { message = "액션을 찾을 수 없습니다." });
        }

        var status = await _context.UserActionStatuses.FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);
        if (status == null)
        {
            status = new UserActionStatus
            {
                UserId = userId,
                ConventionActionId = action.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserActionStatuses.Add(status);
        }

        status.IsComplete = true;
        status.CompletedAt = DateTime.UtcNow;
        status.ResponseDataJson = responseDto?.ResponseDataJson;
        status.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "액션이 완료되었습니다." });
    }

    [Authorize]
    [HttpPost("{actionId:int}/toggle")]
    public async Task<IActionResult> ToggleAction(int conventionId, int actionId, [FromBody] ToggleActionDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        var action = await _context.ConventionActions.FirstOrDefaultAsync(a => a.Id == actionId && a.ConventionId == conventionId);
        if (action == null)
        {
            return NotFound(new { message = "액션을 찾을 수 없습니다." });
        }

        var status = await _context.UserActionStatuses.FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);
        if (status == null)
        {
            status = new UserActionStatus
            {
                UserId = userId,
                ConventionActionId = action.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserActionStatuses.Add(status);
        }

        status.IsComplete = dto.IsComplete;
        status.CompletedAt = dto.IsComplete ? DateTime.UtcNow : null;
        status.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = dto.IsComplete ? "액션이 완료되었습니다." : "완료가 취소되었습니다.",
            isComplete = status.IsComplete
        });
    }

    [Authorize]
    [HttpPost("{actionId}/submit")]
    public async Task<IActionResult> SubmitGenericActionData(int conventionId, int actionId, [FromBody] JsonElement payload)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        var action = await _context.ConventionActions
            .FirstOrDefaultAsync(a => a.Id == actionId && a.ConventionId == conventionId);

        if (action == null)
        {
            return NotFound(new { message = "액션을 찾을 수 없습니다." });
        }

        if (action.BehaviorType != ActionBehaviorType.FormBuilder)
        {
            return BadRequest(new { message = "이 액션은 FormBuilder 타입이 아닙니다." });
        }

        var submission = await _context.ActionSubmissions
            .FirstOrDefaultAsync(s => s.ConventionActionId == actionId && s.UserId == userId);

        if (submission != null)
        {
            submission.SubmissionDataJson = payload.ToString();
            submission.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            submission = new ActionSubmission
            {
                ConventionActionId = actionId,
                UserId = userId,
                SubmissionDataJson = payload.ToString(),
                SubmittedAt = DateTime.UtcNow
            };
            _context.ActionSubmissions.Add(submission);
        }

        var status = await _context.UserActionStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == actionId);

        if (status == null)
        {
            status = new UserActionStatus
            {
                UserId = userId,
                ConventionActionId = actionId,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserActionStatuses.Add(status);
        }

        status.IsComplete = true;
        status.CompletedAt = DateTime.UtcNow;
        status.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "제출이 완료되었습니다." });
    }

    [Authorize]
    [HttpGet("{actionId}/submission")]
    public async Task<IActionResult> GetMySubmission(int conventionId, int actionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        var submission = await _context.ActionSubmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ConventionActionId == actionId && s.UserId == userId);

        if (submission == null)
        {
            return NotFound(new { message = "제출 데이터가 없습니다." });
        }

        var jsonData = JsonDocument.Parse(submission.SubmissionDataJson);
        return Ok(jsonData.RootElement);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{actionId}/submissions/all")]
    public async Task<IActionResult> GetAllSubmissions(int conventionId, int actionId)
    {
        var action = await _context.ConventionActions
            .FirstOrDefaultAsync(a => a.Id == actionId && a.ConventionId == conventionId);

        if (action == null)
        {
            return NotFound(new { message = "액션을 찾을 수 없습니다." });
        }

        if (action.BehaviorType != ActionBehaviorType.FormBuilder)
        {
            return BadRequest(new { message = "이 액션은 FormBuilder 타입이 아닙니다." });
        }

        var submissionsRaw = await _context.ActionSubmissions
            .Include(s => s.User)
            .Where(s => s.ConventionActionId == actionId)
            .ToListAsync();

        var submissions = submissionsRaw.Select(s => new
        {
            s.Id,
            s.UserId,
            UserName = s.User.Name,
            UserEmail = s.User.Email,
            SubmissionData = JsonDocument.Parse(s.SubmissionDataJson).RootElement,
            s.SubmittedAt,
            s.UpdatedAt
        }).ToList();

        return Ok(submissions);
    }

    [Authorize]
    [HttpGet("checklist-status")]
    public async Task<IActionResult> GetChecklistStatus(int conventionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId &&
                       a.IsActive &&
                       a.Deadline.HasValue)
            .OrderBy(a => a.Deadline)
            .ThenBy(a => a.OrderNum)
            .ToListAsync();

        if (actions.Count == 0)
            return Ok(new { totalItems = 0, completedItems = 0, progressPercentage = 0, items = new List<object>() });

        var statuses = await _context.UserActionStatuses
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var statusDict = statuses.ToDictionary(s => s.ConventionActionId, s => s);

        var items = new List<object>();
        int completedCount = 0;

        foreach (var action in actions)
        {
            var status = statusDict.GetValueOrDefault(action.Id);
            bool isComplete = status?.IsComplete ?? false;

            if (isComplete)
                completedCount++;

            items.Add(new
            {
                actionId = action.Id,
                title = action.Title,
                isComplete = isComplete,
                deadline = action.Deadline,
                navigateTo = action.MapsTo,
                orderNum = action.OrderNum,
                behaviorType = action.BehaviorType,
                targetId = action.TargetId
            });
        }

        DateTime? overallDeadline = actions
            .Where(a => {
                var status = statusDict.GetValueOrDefault(a.Id);
                return !(status?.IsComplete ?? false);
            })
            .OrderBy(a => a.Deadline)
            .FirstOrDefault()?.Deadline;

        int totalItems = actions.Count;
        int progressPercentage = totalItems > 0 ? (completedCount * 100 / totalItems) : 0;

        return Ok(new
        {
            totalItems = totalItems,
            completedItems = completedCount,
            progressPercentage = progressPercentage,
            overallDeadline = overallDeadline,
            items = items
        });
    }

    [Authorize]
    [HttpGet("menu")]
    public async Task<IActionResult> GetMenuActions(int conventionId)
    {
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId &&
                       a.IsActive &&
                       a.ActionCategory == "MENU")
            .OrderBy(a => a.OrderNum)
            .ThenBy(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.MapsTo,
                a.OrderNum,
                a.BehaviorType,
                a.TargetId,
                a.ConfigJson
            })
            .ToListAsync();

        return Ok(actions);
    }
}
