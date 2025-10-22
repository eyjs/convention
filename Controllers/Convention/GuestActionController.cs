using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.DTOs.Action;

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
            .OrderBy(a => a.Deadline) // 가장 가까운 순
            .Take(3)
            .Select(a => new
            {
                a.Id,
                a.ActionType,
                a.Title,
                a.Deadline,
                a.MapsTo,
                a.IsRequired,
                IconClass = a.IconClass ?? a.Template.IconClass,
                Category = a.Category ?? a.Template.Category
            })
            .ToListAsync();

        return Ok(actions);
    }

    /// <summary>
    /// 더보기 메뉴용: 모든 활성 액션 조회 (마감기한 여부 무관)
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult> GetAllActions(int conventionId)
    {
        var actions = await _context.ConventionActions
            .Include(a => a.Template)
            .Where(a => a.ConventionId == conventionId && a.IsActive)
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
                IconClass = a.IconClass ?? a.Template.IconClass,
                Category = a.Category ?? a.Template.Category
            })
            .ToListAsync();

        return Ok(actions);
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
            IconClass = action.IconClass ?? action.Template?.IconClass,
            Category = action.Category ?? action.Template?.Category
        });
    }
}
