using LocalRAG.Data;
using LocalRAG.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AttributeTemplateController : ControllerBase
{
    private readonly ConventionDbContext _context;

    public AttributeTemplateController(ConventionDbContext context)
    {
        _context = context;
    }

    // 속성 템플릿 목록 조회
    [HttpGet("conventions/{conventionId}")]
    public async Task<IActionResult> GetAttributeTemplates(int conventionId)
    {
        var templates = await _context.AttributeTemplates
            .Where(at => at.ConventionId == conventionId)
            .OrderBy(at => at.OrderNum)
            .Select(at => new
            {
                at.Id,
                at.AttributeKey,
                at.AttributeValues,
                at.OrderNum
            })
            .ToListAsync();

        return Ok(templates);
    }

    // 속성 템플릿 생성
    [HttpPost("conventions/{conventionId}")]
    public async Task<IActionResult> CreateAttributeTemplate(int conventionId, [FromBody] AttributeTemplateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AttributeKey))
            return BadRequest(new { message = "속성명을 입력해주세요." });

        var template = new AttributeTemplate
        {
            ConventionId = conventionId,
            AttributeKey = dto.AttributeKey.Trim(),
            AttributeValues = dto.AttributeValues,
            OrderNum = dto.OrderNum
        };

        _context.AttributeTemplates.Add(template);
        await _context.SaveChangesAsync();

        return Ok(template);
    }

    // 속성 템플릿 수정
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAttributeTemplate(int id, [FromBody] AttributeTemplateDto dto)
    {
        var template = await _context.AttributeTemplates.FindAsync(id);
        if (template == null) return NotFound();

        template.AttributeKey = dto.AttributeKey.Trim();
        template.AttributeValues = dto.AttributeValues;
        template.OrderNum = dto.OrderNum;

        await _context.SaveChangesAsync();
        return Ok(template);
    }

    // 속성 템플릿 삭제
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttributeTemplate(int id)
    {
        var template = await _context.AttributeTemplates.FindAsync(id);
        if (template == null) return NotFound();

        _context.AttributeTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public class AttributeTemplateDto
{
    public string AttributeKey { get; set; } = string.Empty;
    public string? AttributeValues { get; set; }
    public int OrderNum { get; set; }
}
