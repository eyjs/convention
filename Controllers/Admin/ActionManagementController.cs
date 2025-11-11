using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using Microsoft.AspNetCore.Authorization;
using LocalRAG.Entities.Action;
using LocalRAG.DTOs.ActionModels;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin/action-management")]
[Authorize(Roles = "Admin")]
public class ActionManagementController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ActionManagementController> _logger;

    public ActionManagementController(
        ConventionDbContext context,
        ILogger<ActionManagementController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("convention/{conventionId}")]
    public async Task<ActionResult<List<ConventionActionDto>>> GetActionsForConvention(int conventionId)
    {
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId)
            .OrderBy(a => a.OrderNum)
            .Select(a => new ConventionActionDto
            {
                Id = a.Id,
                ConventionId = a.ConventionId,
                Title = a.Title,
                Deadline = a.Deadline,
                MapsTo = a.MapsTo,
                ConfigJson = a.ConfigJson,
                IsActive = a.IsActive,
                OrderNum = a.OrderNum,
                ActionCategory = a.ActionCategory,
                TargetLocation = a.TargetLocation,
                BehaviorType = a.BehaviorType.ToString(),
                TargetId = a.TargetId,
                TargetModuleId = a.TargetModuleId
            })
            .ToListAsync();
        return Ok(actions);
    }
    
    [HttpPost("actions")]
    public async Task<ActionResult> CreateAction([FromBody] ConventionActionDto request)
    {
        var action = new ConventionAction
        {
            ConventionId = request.ConventionId,
            Title = request.Title,
            MapsTo = request.MapsTo ?? string.Empty,
            Deadline = request.Deadline,
            OrderNum = request.OrderNum,
            ConfigJson = request.ConfigJson,
            IsActive = request.IsActive,
            ActionCategory = request.ActionCategory,
            TargetLocation = request.TargetLocation,
            BehaviorType = Enum.Parse<BehaviorType>(request.BehaviorType),
            TargetId = request.BehaviorType == BehaviorType.FormBuilder.ToString() ? request.TargetId : null,
            TargetModuleId = request.BehaviorType == BehaviorType.ModuleLink.ToString() ? request.TargetModuleId : null,
            CreatedAt = DateTime.UtcNow
        };

        // BehaviorType이 ModuleLink인 경우 MapsTo 필드에 대한 유효성 검사 및 정규화
        if (action.BehaviorType == BehaviorType.ModuleLink)
        {
            action.MapsTo = action.MapsTo.Trim(); // 앞뒤 공백 제거

            // /feature/ 접두어 강제
            if (!action.MapsTo.StartsWith("/feature/", StringComparison.OrdinalIgnoreCase))
            {
                action.MapsTo = "/feature/" + action.MapsTo.TrimStart('/');
            }
            // 중복 슬래시 제거 (예: /feature//path -> /feature/path)
            action.MapsTo = System.Text.RegularExpressions.Regex.Replace(action.MapsTo, "(?<!:)/{2,}", "/");

            // 유효성 검사: /feature/로 시작해야 함
            if (!action.MapsTo.StartsWith("/feature/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("ModuleLink의 MapsTo는 '/feature/'로 시작해야 합니다.");
            }
        }
        _context.ConventionActions.Add(action);
        await _context.SaveChangesAsync();
        return Ok(new { id = action.Id });
    }

    [HttpPut("actions/{id}")]
    public async Task<ActionResult> UpdateAction(int id, [FromBody] ConventionActionDto request)
    {
        _logger.LogInformation("UpdateAction called for ID: {Id}", id);
        _logger.LogInformation("Request DTO: {@Request}", request);

        var action = await _context.ConventionActions.FindAsync(id);
        if (action == null)
        {
            _logger.LogWarning("Action with ID {Id} not found.", id);
            return NotFound();
        }
        _logger.LogInformation("Existing Action: {@Action}", action);

        action.Title = request.Title;
        action.MapsTo = request.MapsTo ?? string.Empty;
        action.Deadline = request.Deadline;
        action.OrderNum = request.OrderNum;
        action.ConfigJson = request.ConfigJson;
        action.IsActive = request.IsActive;
        action.ActionCategory = request.ActionCategory;
        action.TargetLocation = request.TargetLocation;
        action.BehaviorType = Enum.Parse<BehaviorType>(request.BehaviorType);
        action.TargetId = request.BehaviorType == BehaviorType.FormBuilder.ToString() ? request.TargetId : null;
        action.TargetModuleId = request.BehaviorType == BehaviorType.ModuleLink.ToString() ? request.TargetModuleId : null;
        action.UpdatedAt = DateTime.UtcNow;

        // BehaviorType이 ModuleLink인 경우 MapsTo 필드에 대한 유효성 검사 및 정규화
        if (action.BehaviorType == BehaviorType.ModuleLink)
        {
            action.MapsTo = action.MapsTo.Trim(); // 앞뒤 공백 제거

            // /feature/ 접두어 강제
            if (!action.MapsTo.StartsWith("/feature/", StringComparison.OrdinalIgnoreCase))
            {
                action.MapsTo = "/feature/" + action.MapsTo.TrimStart('/');
            }
            // 중복 슬래시 제거 (예: /feature//path -> /feature/path)
            action.MapsTo = System.Text.RegularExpressions.Regex.Replace(action.MapsTo, "(?<!:)/{2,}", "/");

            // 유효성 검사: /feature/로 시작해야 함
            if (!action.MapsTo.StartsWith("/feature/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("ModuleLink의 MapsTo는 '/feature/'로 시작해야 합니다.");
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Action with ID {Id} updated successfully.", id);
            return Ok();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating action with ID {Id}. Request: {@Request}", id, request);
            return StatusCode(500, new { message = "액션 업데이트 중 데이터베이스 오류가 발생했습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating action with ID {Id}. Request: {@Request}", id, request);
            return StatusCode(500, new { message = "액션 업데이트 중 알 수 없는 오류가 발생했습니다." });
        }
    }
    
    [HttpPut("actions/{id}/toggle")]
    public async Task<ActionResult> ToggleAction(int id)
    {
        var action = await _context.ConventionActions.FindAsync(id);
        if (action == null) return NotFound();
        action.IsActive = !action.IsActive;
        await _context.SaveChangesAsync();
        return Ok(new { isActive = action.IsActive });
    }

    [HttpDelete("actions/{id}")]
    public async Task<ActionResult> DeleteAction(int id)
    {
        var action = await _context.ConventionActions.FindAsync(id);
        if (action == null) return NotFound();
        _context.ConventionActions.Remove(action);
        await _context.SaveChangesAsync();
        return Ok();
    }
}