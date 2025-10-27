using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities;

using System.Security.Claims;
using LocalRAG.DTOs.ActionModels;

namespace LocalRAG.Controllers.Convention;

/// <summary>
/// Guest용 ConventionAction API
/// </summary>
[ApiController]
[Route("api/conventions/{conventionId}/actions")]
public class GuestActionController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<GuestActionController> _logger;

    public GuestActionController(
        ConventionDbContext context,
        ILogger<GuestActionController> logger)
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
                a.ActionType,
                a.Title,
                a.Deadline,
                a.MapsTo,
                a.IsRequired,
                a.ActionCategory,
                a.TargetLocation,
                a.ConfigJson,
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
                a.ActionType,
                a.Title,
                a.Deadline,
                a.MapsTo,
                a.IsRequired,
                a.ActionCategory,
                a.TargetLocation,
                a.ConfigJson,
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
        var guestIdClaim = User.FindFirst("GuestId");
        if (guestIdClaim == null || !int.TryParse(guestIdClaim.Value, out int guestId))
        {
            return Unauthorized("게스트 정보를 확인할 수 없습니다.");
        }

        var statuses = await _context.GuestActionStatuses
            .Where(s => s.GuestId == guestId && s.ConventionAction != null && s.ConventionAction.ConventionId == conventionId)
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
            action.ActionType,
            action.Title,
            action.Deadline,
            action.MapsTo,
            action.ConfigJson,
            action.IsRequired,
            action.ActionCategory,
            action.TargetLocation,
            IconClass = action.IconClass ?? action.Template?.IconClass,
            Category = action.Category ?? action.Template?.Category
        });
    }

    [Authorize]
    [HttpPost("{actionType}/complete")]
    public async Task<IActionResult> CompleteAction(int conventionId, string actionType, [FromBody] ActionResponseDto responseDto)
    {
        var guestIdClaim = User.FindFirst("GuestId");
        if (guestIdClaim == null || !int.TryParse(guestIdClaim.Value, out int guestId))
        {
            return Unauthorized("게스트 정보를 확인할 수 없습니다.");
        }

        var action = await _context.ConventionActions.FirstOrDefaultAsync(a => a.ConventionId == conventionId && a.ActionType == actionType);
        if (action == null)
        {
            return NotFound(new { message = "액션을 찾을 수 없습니다." });
        }

        var status = await _context.GuestActionStatuses.FirstOrDefaultAsync(s => s.GuestId == guestId && s.ConventionActionId == action.Id);
        if (status == null)
        {
            status = new GuestActionStatus
            {
                GuestId = guestId,
                ConventionActionId = action.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.GuestActionStatuses.Add(status);
        }

        status.IsComplete = true;
        status.CompletedAt = DateTime.UtcNow;
        status.ResponseDataJson = responseDto.ResponseDataJson;
        status.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "액션이 완료되었습니다." });
    }
}
