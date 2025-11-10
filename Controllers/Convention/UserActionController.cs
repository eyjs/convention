using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities;
using LocalRAG.Entities.Action;
using System.Security.Claims;
using System.Text.Json;
using LocalRAG.DTOs.ActionModels;

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

    public UserActionController(
        ConventionDbContext context,
        ILogger<UserActionController> logger)
    {
        _context = context;
        _logger = logger;
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
                a.TargetModuleId,
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

        // isActive 필터 (기본값: true - 활성 액션만)
        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }
        else
        {
            query = query.Where(a => a.IsActive); // 기본: 활성만
        }

        // targetLocation 필터 (콤마로 여러 위치 지원)
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

        // actionCategory 필터
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
                a.TargetModuleId,
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

    /// <summary>
    /// 특정 액션의 상세 정보 조회
    /// </summary>
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
            action.TargetModuleId,
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

    /// <summary>
    /// 액션 상태 토글 (완료 ↔ 미완료)
    /// </summary>
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

    /// <summary>
    /// GenericForm 타입 액션의 데이터 제출 (Create/Update)
    /// 마감기한이 있는 경우 자동으로 완료 처리하여 진척도 업데이트
    /// </summary>
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

        // 이 API는 GenericForm 타입만 처리
        if (action.BehaviorType != ActionBehaviorType.GenericForm)
        {
            return BadRequest(new { message = "이 액션은 GenericForm 타입이 아닙니다." });
        }

        // ActionSubmission 조회/생성
        var submission = await _context.ActionSubmissions
            .FirstOrDefaultAsync(s => s.ConventionActionId == actionId && s.UserId == userId);

        if (submission != null)
        {
            // Update
            submission.SubmissionDataJson = payload.ToString();
            submission.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create
            submission = new ActionSubmission
            {
                ConventionActionId = actionId,
                UserId = userId,
                SubmissionDataJson = payload.ToString(),
                SubmittedAt = DateTime.UtcNow
            };
            _context.ActionSubmissions.Add(submission);
        }

        // 중요: 데이터 저장과 별개로, '완료' 상태도 함께 기록
        // 마감기한이 있는 경우 진척도에 반영됨
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

    /// <summary>
    /// GenericForm 타입 액션에 대해 내가 이전에 제출한 데이터 조회 (Read)
    /// </summary>
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

        // JSON 문자열을 객체로 파싱하여 반환
        var jsonData = JsonDocument.Parse(submission.SubmissionDataJson);
        return Ok(jsonData.RootElement);
    }

    /// <summary>
    /// [관리자용] GenericForm 타입 액션의 모든 제출 현황 조회
    /// </summary>
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

        if (action.BehaviorType != ActionBehaviorType.GenericForm)
        {
            return BadRequest(new { message = "이 액션은 GenericForm 타입이 아닙니다." });
        }

        var submissionsRaw = await _context.ActionSubmissions
            .Include(s => s.User)
            .Where(s => s.ConventionActionId == actionId)
            .ToListAsync();

        // JSON 파싱은 메모리에서 수행 (식 트리 제한 우회)
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

    /// <summary>
    /// 체크리스트 상태 조회 (Deadline이 있는 액션들)
    /// </summary>
    [Authorize]
    [HttpGet("checklist-status")]
    public async Task<IActionResult> GetChecklistStatus(int conventionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        // 1. 해당 행사의 활성 액션 중 Deadline이 있는 것만 조회
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId &&
                       a.IsActive &&
                       a.Deadline.HasValue)
            .OrderBy(a => a.Deadline)
            .ThenBy(a => a.OrderNum)
            .ToListAsync();

        if (actions.Count == 0)
            return Ok(new { totalItems = 0, completedItems = 0, progressPercentage = 0, items = new List<object>() });

        // 2. 해당 사용자의 액션 상태 조회
        var statuses = await _context.UserActionStatuses
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var statusDict = statuses.ToDictionary(s => s.ConventionActionId, s => s);

        // 3. 체크리스트 아이템 구축
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
                orderNum = action.OrderNum
            });
        }

        // 4. 가장 가까운 미완료 액션의 마감일 찾기
        DateTime? overallDeadline = actions
            .Where(a => {
                var status = statusDict.GetValueOrDefault(a.Id);
                return !(status?.IsComplete ?? false);
            })
            .OrderBy(a => a.Deadline)
            .FirstOrDefault()?.Deadline;

        // 5. 체크리스트 상태 반환
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

    /// <summary>
    /// 추가 메뉴 액션 조회 (ActionCategory = "MENU")
    /// </summary>
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
                a.OrderNum
            })
            .ToListAsync();

        return Ok(actions);
    }
}
