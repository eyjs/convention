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
            MapsTo = request.MapsTo,
            Deadline = request.Deadline,
            OrderNum = request.OrderNum,
            ConfigJson = request.ConfigJson,
            IsActive = request.IsActive,
            ActionCategory = request.ActionCategory,
            TargetLocation = request.TargetLocation,
            BehaviorType = request.BehaviorType,
            TargetId = request.BehaviorType == ActionBehaviorType.FormBuilder ? request.TargetId : null,
            TargetModuleId = request.BehaviorType == ActionBehaviorType.ModuleLink ? request.TargetModuleId : null,
            CreatedAt = DateTime.UtcNow
        };
        _context.ConventionActions.Add(action);
        await _context.SaveChangesAsync();
        return Ok(new { id = action.Id });
    }

    [HttpPut("actions/{id}")]
    public async Task<ActionResult> UpdateAction(int id, [FromBody] ConventionActionDto request)
    {
        var action = await _context.ConventionActions.FindAsync(id);
        if (action == null) return NotFound();

        action.Title = request.Title;
        action.MapsTo = request.MapsTo;
        action.Deadline = request.Deadline;
        action.OrderNum = request.OrderNum;
        action.ConfigJson = request.ConfigJson;
        action.IsActive = request.IsActive;
        action.ActionCategory = request.ActionCategory;
        action.TargetLocation = request.TargetLocation;
        action.BehaviorType = request.BehaviorType;
        action.TargetId = request.BehaviorType == ActionBehaviorType.FormBuilder ? request.TargetId : null;
        action.TargetModuleId = request.BehaviorType == ActionBehaviorType.ModuleLink ? request.TargetModuleId : null;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok();
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