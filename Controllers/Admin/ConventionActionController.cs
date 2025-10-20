using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Models;
using LocalRAG.DTOs.Action;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/conventions/{conventionId}/actions")]
public class ConventionActionController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ConventionActionController> _logger;

    public ConventionActionController(
        ConventionDbContext context,
        ILogger<ConventionActionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 특정 행사의 액션 목록 조회
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConventionAction>>> GetActions(int conventionId)
    {
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId)
            .OrderBy(a => a.OrderNum)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();

        return Ok(actions);
    }

    /// <summary>
    /// 액션 상세 조회
    /// </summary>
    [HttpGet("{actionId}")]
    public async Task<ActionResult<ConventionAction>> GetAction(int conventionId, int actionId)
    {
        var action = await _context.ConventionActions
            .FirstOrDefaultAsync(a => a.Id == actionId && a.ConventionId == conventionId);

        if (action == null)
            return NotFound();

        return Ok(action);
    }

    /// <summary>
    /// 새 액션 생성
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConventionAction>> CreateAction(
        int conventionId,
        [FromBody] ConventionActionDto dto)
    {
        // 행사 존재 확인
        var conventionExists = await _context.Conventions.AnyAsync(c => c.Id == conventionId);
        if (!conventionExists)
            return NotFound("Convention not found");

        // 중복 ActionType 확인
        var isDuplicate = await _context.ConventionActions
            .AnyAsync(a => a.ConventionId == conventionId && a.ActionType == dto.ActionType);
        
        if (isDuplicate)
            return BadRequest($"ActionType '{dto.ActionType}' already exists for this convention");

        var action = new ConventionAction
        {
            ConventionId = conventionId,
            ActionType = dto.ActionType,
            Title = dto.Title,
            Deadline = dto.Deadline,
            MapsTo = dto.MapsTo,
            ConfigJson = dto.ConfigJson,
            IsActive = dto.IsActive,
            OrderNum = dto.OrderNum,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ConventionActions.Add(action);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created action {ActionType} for convention {ConventionId}",
            action.ActionType,
            conventionId);

        return CreatedAtAction(
            nameof(GetAction),
            new { conventionId, actionId = action.Id },
            action);
    }

    /// <summary>
    /// 액션 수정
    /// </summary>
    [HttpPut("{actionId}")]
    public async Task<ActionResult> UpdateAction(
        int conventionId,
        int actionId,
        [FromBody] ConventionActionDto dto)
    {
        var action = await _context.ConventionActions
            .FirstOrDefaultAsync(a => a.Id == actionId && a.ConventionId == conventionId);

        if (action == null)
            return NotFound();

        // ActionType 변경 시 중복 확인
        if (action.ActionType != dto.ActionType)
        {
            var isDuplicate = await _context.ConventionActions
                .AnyAsync(a => a.ConventionId == conventionId 
                    && a.ActionType == dto.ActionType 
                    && a.Id != actionId);
            
            if (isDuplicate)
                return BadRequest($"ActionType '{dto.ActionType}' already exists for this convention");
        }

        action.ActionType = dto.ActionType;
        action.Title = dto.Title;
        action.Deadline = dto.Deadline;
        action.MapsTo = dto.MapsTo;
        action.ConfigJson = dto.ConfigJson;
        action.IsActive = dto.IsActive;
        action.OrderNum = dto.OrderNum;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated action {ActionId}", actionId);

        return NoContent();
    }

    /// <summary>
    /// 액션 삭제
    /// </summary>
    [HttpDelete("{actionId}")]
    public async Task<ActionResult> DeleteAction(int conventionId, int actionId)
    {
        var action = await _context.ConventionActions
            .Include(a => a.GuestActionStatuses)
            .FirstOrDefaultAsync(a => a.Id == actionId && a.ConventionId == conventionId);

        if (action == null)
            return NotFound();

        // 연관된 GuestActionStatus도 함께 삭제됨 (Cascade)
        _context.ConventionActions.Remove(action);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted action {ActionId} with {StatusCount} guest statuses",
            actionId,
            action.GuestActionStatuses.Count);

        return NoContent();
    }

    /// <summary>
    /// 액션 활성/비활성 토글
    /// </summary>
    [HttpPatch("{actionId}/toggle")]
    public async Task<ActionResult> ToggleAction(int conventionId, int actionId)
    {
        var action = await _context.ConventionActions
            .FirstOrDefaultAsync(a => a.Id == actionId && a.ConventionId == conventionId);

        if (action == null)
            return NotFound();

        action.IsActive = !action.IsActive;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { isActive = action.IsActive });
    }
}
