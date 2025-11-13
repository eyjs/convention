using LocalRAG.Data;
using LocalRAG.Entities;
using LocalRAG.DTOs.ConventionModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AttributeController : ControllerBase
{
    private readonly ConventionDbContext _context;

    public AttributeController(ConventionDbContext context)
    {
        _context = context;
    }

    // 속성 정의 목록 조회
    [HttpGet("conventions/{conventionId}/definitions")]
    public async Task<IActionResult> GetAttributeDefinitions(int conventionId)
    {
        var definitions = await _context.AttributeDefinitions
            .Where(ad => ad.ConventionId == conventionId)
            .OrderBy(ad => ad.OrderNum)
            .ToListAsync();

        return Ok(definitions);
    }

    // 속성 정의 생성
    [HttpPost("conventions/{conventionId}/definitions")]
    public async Task<IActionResult> CreateAttributeDefinition(int conventionId, [FromBody] AttributeDefinitionDto dto)
    {
        var definition = new AttributeDefinition
        {
            ConventionId = conventionId,
            AttributeKey = dto.AttributeKey,
            Options = dto.Options,
            OrderNum = dto.OrderNum,
            IsRequired = dto.IsRequired
        };

        _context.AttributeDefinitions.Add(definition);
        await _context.SaveChangesAsync();

        return Ok(definition);
    }

    // 속성 정의 수정
    [HttpPut("definitions/{id}")]
    public async Task<IActionResult> UpdateAttributeDefinition(int id, [FromBody] AttributeDefinitionDto dto)
    {
        var definition = await _context.AttributeDefinitions.FindAsync(id);
        if (definition == null) return NotFound();

        definition.AttributeKey = dto.AttributeKey;
        definition.Options = dto.Options;
        definition.OrderNum = dto.OrderNum;
        definition.IsRequired = dto.IsRequired;

        await _context.SaveChangesAsync();
        return Ok(definition);
    }

    // 속성 정의 삭제
    [HttpDelete("definitions/{id}")]
    public async Task<IActionResult> DeleteAttributeDefinition(int id)
    {
        var definition = await _context.AttributeDefinitions.FindAsync(id);
        if (definition == null) return NotFound();

        _context.AttributeDefinitions.Remove(definition);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

