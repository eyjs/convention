using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LocalRAG.Entities.Action;
using LocalRAG.DTOs.ActionModels;
using LocalRAG.Repositories;
using System.Text.RegularExpressions;
using LocalRAG.Constants;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin/action-management")]
[Authorize(Roles = Roles.Admin)]
public class ActionManagementController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActionManagementController> _logger;

    public ActionManagementController(
        IUnitOfWork unitOfWork,
        ILogger<ActionManagementController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("convention/{conventionId}")]
    public async Task<ActionResult<List<ConventionActionDto>>> GetActionsForConvention(int conventionId)
    {
        var actions = await _unitOfWork.ConventionActions.Query
            .Where(a => a.ConventionId == conventionId)
            .OrderBy(a => a.OrderNum)
            .Select(a => new ConventionActionDto
            {
                Id = a.Id,
                ConventionId = a.ConventionId,
                Title = a.Title,
                Deadline = a.Deadline,
                MapsTo = a.MapsTo,
                IsActive = a.IsActive,
                OrderNum = a.OrderNum,
                ActionCategory = a.ActionCategory,
                TargetLocation = a.TargetLocation,
                BehaviorType = a.BehaviorType.ToString(),
                TargetId = a.TargetId,
                TargetModuleId = a.TargetModuleId,
                ConfigJson = a.ConfigJson
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
            IsActive = request.IsActive,
            ActionCategory = request.ActionCategory,
            TargetLocation = request.TargetLocation,
            BehaviorType = Enum.Parse<BehaviorType>(request.BehaviorType),
            TargetId = request.BehaviorType == BehaviorType.FormBuilder.ToString() ? request.TargetId : null,
            TargetModuleId = request.BehaviorType == BehaviorType.ModuleLink.ToString() ? request.TargetModuleId : null,
            ConfigJson = request.ConfigJson,
            CreatedAt = DateTime.UtcNow
        };

        if (action.BehaviorType == BehaviorType.ModuleLink)
        {
            action.MapsTo = action.MapsTo.Trim();

            if (!action.MapsTo.StartsWith("/feature/", StringComparison.OrdinalIgnoreCase))
            {
                action.MapsTo = "/feature/" + action.MapsTo.TrimStart('/');
            }
            action.MapsTo = Regex.Replace(action.MapsTo, "(?<!:)/{2,}", "/");

            if (!action.MapsTo.StartsWith("/feature/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("ModuleLink의 MapsTo는 '/feature/'로 시작해야 합니다.");
            }
        }
        await _unitOfWork.ConventionActions.AddAsync(action);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new { id = action.Id });
    }

    [HttpPut("actions/{id}")]
    public async Task<ActionResult> UpdateAction(int id, [FromBody] ConventionActionDto request)
    {
        _logger.LogInformation("UpdateAction called for ID: {Id}", id);
        _logger.LogInformation("Request DTO: {@Request}", request);

        var action = await _unitOfWork.ConventionActions.GetByIdAsync(id);
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
        action.IsActive = request.IsActive;
        action.ActionCategory = request.ActionCategory;
        action.TargetLocation = request.TargetLocation;
        action.BehaviorType = Enum.Parse<BehaviorType>(request.BehaviorType);
        action.TargetId = request.BehaviorType == BehaviorType.FormBuilder.ToString() ? request.TargetId : null;
        action.TargetModuleId = request.BehaviorType == BehaviorType.ModuleLink.ToString() ? request.TargetModuleId : null;
        action.ConfigJson = request.ConfigJson;
        action.UpdatedAt = DateTime.UtcNow;

        if (action.BehaviorType == BehaviorType.ModuleLink)
        {
            action.MapsTo = action.MapsTo.Trim();

            if (!action.MapsTo.StartsWith("/feature/", StringComparison.OrdinalIgnoreCase))
            {
                action.MapsTo = "/feature/" + action.MapsTo.TrimStart('/');
            }
            action.MapsTo = Regex.Replace(action.MapsTo, "(?<!:)/{2,}", "/");

            if (!action.MapsTo.StartsWith("/feature/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("ModuleLink의 MapsTo는 '/feature/'로 시작해야 합니다.");
            }
        }

        try
        {
            _unitOfWork.ConventionActions.Update(action);
            await _unitOfWork.SaveChangesAsync();
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
        var action = await _unitOfWork.ConventionActions.GetByIdAsync(id);
        if (action == null) return NotFound();
        action.IsActive = !action.IsActive;
        _unitOfWork.ConventionActions.Update(action);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new { isActive = action.IsActive });
    }

    [HttpDelete("actions/{id}")]
    public async Task<ActionResult> DeleteAction(int id)
    {
        var action = await _unitOfWork.ConventionActions.GetByIdAsync(id);
        if (action == null) return NotFound();
        _unitOfWork.ConventionActions.Remove(action);
        await _unitOfWork.SaveChangesAsync();
        return Ok();
    }
}
