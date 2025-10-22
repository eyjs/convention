using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.DTOs.Action;
using Microsoft.AspNetCore.Authorization;
using LocalRAG.Models;

namespace LocalRAG.Controllers.Admin;

/// <summary>
/// 액션 관리 대시보드 컨트롤러
/// </summary>
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

    /// <summary>
    /// 행사별 액션 관리 대시보드 데이터 조회
    /// </summary>
    [HttpGet("convention/{conventionId}")]
    public async Task<ActionResult<ActionManagementDto>> GetActionManagement(int conventionId)
    {
        var convention = await _context.Conventions.FindAsync(conventionId);
        if (convention == null)
            return NotFound("Convention not found");

        var totalGuests = await _context.Guests
            .CountAsync(g => g.ConventionId == conventionId);

        // 현재 행사의 액션 목록
        var actions = await _context.ConventionActions
            .Include(a => a.Template)
            .Include(a => a.GuestActionStatuses)
            .Where(a => a.ConventionId == conventionId)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.OrderNum)
            .Select(a => new ConventionActionDetailDto
            {
                Id = a.Id,
                ConventionId = a.ConventionId,
                ActionType = a.ActionType,
                Title = a.Title,
                Deadline = a.Deadline,
                MapsTo = a.MapsTo,
                ConfigJson = a.ConfigJson,
                IsActive = a.IsActive,
                OrderNum = a.OrderNum,
                IsRequired = a.IsRequired,
                IconClass = a.IconClass ?? (a.Template != null ? a.Template.IconClass : null),
                Category = a.Category ?? (a.Template != null ? a.Template.Category : null),
                TemplateName = a.Template != null ? a.Template.TemplateName : null,
                TemplateType = a.Template != null ? a.Template.TemplateType : null,
                CompletedCount = a.GuestActionStatuses.Count(s => s.IsComplete),
                TotalGuestCount = totalGuests
            })
            .ToListAsync();

        // 사용 가능한 템플릿 목록
        var usedTemplateIds = actions
            .Where(a => a.Id.HasValue)
            .Select(a => a.Id!.Value)
            .ToList();

        var availableTemplates = await _context.ActionTemplates
            .Where(t => t.IsActive && !_context.ConventionActions
                .Any(ca => ca.ConventionId == conventionId && ca.TemplateId == t.Id))
            .OrderBy(t => t.Category)
            .ThenBy(t => t.OrderNum)
            .Select(t => new ActionTemplateSummaryDto
            {
                Id = t.Id,
                TemplateType = t.TemplateType,
                TemplateName = t.TemplateName,
                Description = t.Description,
                Category = t.Category,
                IconClass = t.IconClass,
                IsActive = t.IsActive
            })
            .ToListAsync();

        var result = new ActionManagementDto
        {
            ConventionId = conventionId,
            ConventionName = convention.Title,
            Actions = actions,
            AvailableTemplates = availableTemplates
        };

        return Ok(result);
    }

    /// <summary>
    /// 액션 순서 일괄 변경
    /// </summary>
    [HttpPost("convention/{conventionId}/reorder")]
    public async Task<ActionResult> ReorderActions(
        int conventionId, 
        [FromBody] List<ActionReorderDto> reorderRequests)
    {
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId)
            .ToListAsync();

        foreach (var request in reorderRequests)
        {
            var action = actions.FirstOrDefault(a => a.Id == request.ActionId);
            if (action != null)
            {
                action.OrderNum = request.NewOrder;
                action.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Reordered {Count} actions for convention {ConventionId}", 
            reorderRequests.Count, conventionId);

        return NoContent();
    }

    /// <summary>
    /// 여러 템플릿을 한번에 행사에 적용
    /// </summary>
    [HttpPost("convention/{conventionId}/apply-templates")]
    public async Task<ActionResult> ApplyMultipleTemplates(
        int conventionId,
        [FromBody] List<int> templateIds)
    {
        var convention = await _context.Conventions.FindAsync(conventionId);
        if (convention == null)
            return NotFound("Convention not found");

        var templates = await _context.ActionTemplates
            .Where(t => templateIds.Contains(t.Id))
            .ToListAsync();

        var createdActions = new List<ConventionAction>();
        var errors = new List<string>();

        foreach (var template in templates)
        {
            // 이미 존재하는지 확인
            var exists = await _context.ConventionActions
                .AnyAsync(a => a.ConventionId == conventionId && 
                         (a.TemplateId == template.Id || a.ActionType == template.TemplateType));

            if (exists)
            {
                errors.Add($"템플릿 '{template.TemplateName}'은 이미 적용되어 있습니다.");
                continue;
            }

            var action = new ConventionAction
            {
                ConventionId = conventionId,
                TemplateId = template.Id,
                ActionType = template.TemplateType,
                Title = template.TemplateName,
                MapsTo = template.DefaultRoute,
                ConfigJson = template.DefaultConfigJson,
                IconClass = template.IconClass,
                Category = template.Category,
                IsActive = true,
                IsRequired = false,
                OrderNum = 0, // 나중에 정렬 필요
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ConventionActions.Add(action);
            createdActions.Add(action);
        }

        if (createdActions.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Applied {Count} templates to convention {ConventionId}", 
                createdActions.Count, conventionId);
        }

        return Ok(new 
        { 
            Created = createdActions.Count,
            Errors = errors
        });
    }

    /// <summary>
    /// 액션 일괄 활성/비활성화
    /// </summary>
    [HttpPost("convention/{conventionId}/toggle-bulk")]
    public async Task<ActionResult> ToggleBulkActions(
        int conventionId,
        [FromBody] BulkToggleDto request)
    {
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId && request.ActionIds.Contains(a.Id))
            .ToListAsync();

        foreach (var action in actions)
        {
            action.IsActive = request.IsActive;
            action.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Toggled {Count} actions to {Status} for convention {ConventionId}", 
            actions.Count, request.IsActive ? "active" : "inactive", conventionId);

        return Ok(new { UpdatedCount = actions.Count });
    }

    // ============================================
    // CRUD 메서드 (ConventionActionController에서 통합)
    // ============================================

    /// <summary>
    /// 액션 생성
    /// </summary>
    [HttpPost("actions")]
    public async Task<ActionResult> CreateAction([FromBody] ConventionActionDto request)
    {
        try
        {
            // 중복 체크 (같은 행사에서 같은 ActionType)
            var exists = await _context.ConventionActions
                .AnyAsync(a => a.ConventionId == request.ConventionId && a.ActionType == request.ActionType);

            if (exists)
                return BadRequest(new { message = "이미 동일한 액션 타입이 존재합니다." });

            var action = new ConventionAction
            {
                ConventionId = request.ConventionId,
                ActionType = request.ActionType,
                Title = request.Title,
                MapsTo = request.MapsTo,
                Deadline = request.Deadline == DateTime.MinValue ? null : request.Deadline,
                OrderNum = request.OrderNum,
                ConfigJson = request.ConfigJson,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ConventionActions.Add(action);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created action {ActionType} for convention {ConventionId}", 
                action.ActionType, action.ConventionId);

            return Ok(new { id = action.Id, message = "액션이 생성되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create action");
            return StatusCode(500, new { message = "액션 생성에 실패했습니다.", details = ex.Message });
        }
    }

    /// <summary>
    /// 액션 수정
    /// </summary>
    [HttpPut("actions/{id}")]
    public async Task<ActionResult> UpdateAction(int id, [FromBody] ConventionActionDto request)
    {
        try
        {
            var action = await _context.ConventionActions.FindAsync(id);
            if (action == null)
                return NotFound(new { message = "액션을 찾을 수 없습니다." });

            // 중복 체크 (자기 자신 제외)
            var exists = await _context.ConventionActions
                .AnyAsync(a => a.Id != id && 
                         a.ConventionId == request.ConventionId && 
                         a.ActionType == request.ActionType);

            if (exists)
                return BadRequest(new { message = "이미 동일한 액션 타입이 존재합니다." });

            action.ActionType = request.ActionType;
            action.Title = request.Title;
            action.MapsTo = request.MapsTo;
            action.Deadline = request.Deadline == DateTime.MinValue ? null : request.Deadline;
            action.OrderNum = request.OrderNum;
            action.ConfigJson = request.ConfigJson;
            action.IsActive = request.IsActive;
            action.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated action {ActionId}", id);

            return Ok(new { message = "액션이 수정되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update action {ActionId}", id);
            return StatusCode(500, new { message = "액션 수정에 실패했습니다.", details = ex.Message });
        }
    }

    /// <summary>
    /// 단일 액션 활성/비활성 토글
    /// </summary>
    [HttpPut("actions/{id}/toggle")]
    public async Task<ActionResult> ToggleAction(int id)
    {
        try
        {
            var action = await _context.ConventionActions.FindAsync(id);
            if (action == null)
                return NotFound(new { message = "액션을 찾을 수 없습니다." });

            action.IsActive = !action.IsActive;
            action.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled action {ActionId} to {IsActive}", id, action.IsActive);

            return Ok(new { isActive = action.IsActive, message = "상태가 변경되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle action {ActionId}", id);
            return StatusCode(500, new { message = "상태 변경에 실패했습니다.", details = ex.Message });
        }
    }

    /// <summary>
    /// 액션 삭제
    /// </summary>
    [HttpDelete("actions/{id}")]
    public async Task<ActionResult> DeleteAction(int id)
    {
        try
        {
            var action = await _context.ConventionActions
                .Include(a => a.GuestActionStatuses)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (action == null)
                return NotFound(new { message = "액션을 찾을 수 없습니다." });

            // 관련된 GuestActionStatus도 함께 삭제
            _context.GuestActionStatuses.RemoveRange(action.GuestActionStatuses);
            _context.ConventionActions.Remove(action);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted action {ActionId}", id);

            return Ok(new { message = "액션이 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete action {ActionId}", id);
            return StatusCode(500, new { message = "액션 삭제에 실패했습니다.", details = ex.Message });
        }
    }
}

public class ActionReorderDto
{
    public int ActionId { get; set; }
    public int NewOrder { get; set; }
}

public class BulkToggleDto
{
    public List<int> ActionIds { get; set; } = new();
    public bool IsActive { get; set; }
}