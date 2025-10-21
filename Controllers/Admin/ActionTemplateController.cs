using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Models;
using LocalRAG.DTOs.Action;

namespace LocalRAG.Controllers.Admin;

/// <summary>
/// 액션 템플릿 관리 (공통 템플릿)
/// </summary>
[ApiController]
[Route("api/admin/action-templates")]
[Authorize(Roles = "Admin")]
public class ActionTemplateController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ActionTemplateController> _logger;

    public ActionTemplateController(
        ConventionDbContext context,
        ILogger<ActionTemplateController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 모든 액션 템플릿 목록 조회
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActionTemplate>>> GetTemplates([FromQuery] string? category = null)
    {
        var query = _context.ActionTemplates.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);

        var templates = await query
            .OrderBy(t => t.Category)
            .ThenBy(t => t.OrderNum)
            .ToListAsync();

        return Ok(templates);
    }

    /// <summary>
    /// 카테고리별 그룹화된 템플릿 목록
    /// </summary>
    [HttpGet("by-category")]
    public async Task<ActionResult> GetTemplatesByCategory()
    {
        var templates = await _context.ActionTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.OrderNum)
            .ToListAsync();

        var grouped = templates.GroupBy(t => t.Category)
            .Select(g => new
            {
                Category = g.Key,
                Templates = g.ToList()
            });

        return Ok(grouped);
    }

    /// <summary>
    /// 템플릿 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ActionTemplate>> GetTemplate(int id)
    {
        var template = await _context.ActionTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        return Ok(template);
    }

    /// <summary>
    /// 새 템플릿 생성
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ActionTemplate>> CreateTemplate([FromBody] ActionTemplateDto dto)
    {
        // 중복 TemplateType 확인
        var exists = await _context.ActionTemplates
            .AnyAsync(t => t.TemplateType == dto.TemplateType);
        
        if (exists)
            return BadRequest($"TemplateType '{dto.TemplateType}' already exists");

        var template = new ActionTemplate
        {
            TemplateType = dto.TemplateType,
            TemplateName = dto.TemplateName,
            Description = dto.Description,
            Category = dto.Category,
            IconClass = dto.IconClass,
            DefaultRoute = dto.DefaultRoute,
            DefaultConfigJson = dto.DefaultConfigJson,
            RequiredFields = dto.RequiredFields,
            IsActive = dto.IsActive,
            OrderNum = dto.OrderNum,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ActionTemplates.Add(template);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created action template: {TemplateType}", template.TemplateType);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    /// <summary>
    /// 템플릿 수정
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTemplate(int id, [FromBody] ActionTemplateDto dto)
    {
        var template = await _context.ActionTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        template.TemplateName = dto.TemplateName;
        template.Description = dto.Description;
        template.Category = dto.Category;
        template.IconClass = dto.IconClass;
        template.DefaultRoute = dto.DefaultRoute;
        template.DefaultConfigJson = dto.DefaultConfigJson;
        template.RequiredFields = dto.RequiredFields;
        template.IsActive = dto.IsActive;
        template.OrderNum = dto.OrderNum;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated template: {TemplateType}", template.TemplateType);
        return NoContent();
    }

    /// <summary>
    /// 템플릿 삭제
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTemplate(int id)
    {
        var template = await _context.ActionTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        // 사용 중인 ConventionAction이 있는지 확인
        var inUse = await _context.ConventionActions
            .AnyAsync(a => a.TemplateId == id);

        if (inUse)
            return BadRequest("이 템플릿을 사용 중인 액션이 있습니다. 먼저 액션을 삭제하거나 수정하세요.");

        _context.ActionTemplates.Remove(template);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted template: {TemplateType}", template.TemplateType);
        return NoContent();
    }

    /// <summary>
    /// 템플릿으로부터 Convention Action 생성
    /// </summary>
    [HttpPost("{templateId}/apply-to-convention/{conventionId}")]
    public async Task<ActionResult> ApplyTemplateToConvention(int templateId, int conventionId, [FromBody] ApplyTemplateDto dto)
    {
        var template = await _context.ActionTemplates.FindAsync(templateId);
        if (template == null)
            return NotFound("Template not found");

        var convention = await _context.Conventions.FindAsync(conventionId);
        if (convention == null)
            return NotFound("Convention not found");

        // 이미 같은 ActionType이 있는지 확인
        var actionType = dto.ActionType ?? template.TemplateType;
        var exists = await _context.ConventionActions
            .AnyAsync(a => a.ConventionId == conventionId && a.ActionType == actionType);

        if (exists)
            return BadRequest("이미 동일한 ActionType이 존재합니다.");

        var action = new ConventionAction
        {
            ConventionId = conventionId,
            TemplateId = templateId,
            ActionType = actionType,
            Title = dto.Title ?? template.TemplateName,
            Deadline = dto.Deadline,
            MapsTo = dto.MapsTo ?? template.DefaultRoute,
            ConfigJson = dto.ConfigJson ?? template.DefaultConfigJson,
            IsActive = dto.IsActive,
            IsRequired = dto.IsRequired,
            OrderNum = dto.OrderNum,
            IconClass = dto.IconClass ?? template.IconClass,
            Category = dto.Category ?? template.Category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ConventionActions.Add(action);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Applied template {TemplateType} to convention {ConventionId}", 
            template.TemplateType, conventionId);

        return Ok(action);
    }
}
