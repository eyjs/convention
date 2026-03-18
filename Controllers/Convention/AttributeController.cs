using LocalRAG.Entities;
using LocalRAG.DTOs.ConventionModels;
using LocalRAG.Repositories;
using LocalRAG.Constants;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class AttributeController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AttributeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // 속성 정의 목록 조회
    [HttpGet("conventions/{conventionId}/definitions")]
    public async Task<IActionResult> GetAttributeDefinitions(int conventionId)
    {
        var definitions = await _unitOfWork.AttributeDefinitions
            .FindAsync(ad => ad.ConventionId == conventionId);

        var sorted = definitions.OrderBy(ad => ad.OrderNum).ToList();
        return Ok(sorted);
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

        await _unitOfWork.AttributeDefinitions.AddAsync(definition);
        await _unitOfWork.SaveChangesAsync();

        return Ok(definition);
    }

    // 속성 정의 수정
    [HttpPut("definitions/{id}")]
    public async Task<IActionResult> UpdateAttributeDefinition(int id, [FromBody] AttributeDefinitionDto dto)
    {
        var definition = await _unitOfWork.AttributeDefinitions.GetByIdAsync(id);
        if (definition == null) return NotFound();

        definition.AttributeKey = dto.AttributeKey;
        definition.Options = dto.Options;
        definition.OrderNum = dto.OrderNum;
        definition.IsRequired = dto.IsRequired;

        _unitOfWork.AttributeDefinitions.Update(definition);
        await _unitOfWork.SaveChangesAsync();
        return Ok(definition);
    }

    // 속성 정의 삭제
    [HttpDelete("definitions/{id}")]
    public async Task<IActionResult> DeleteAttributeDefinition(int id)
    {
        var definition = await _unitOfWork.AttributeDefinitions.GetByIdAsync(id);
        if (definition == null) return NotFound();

        _unitOfWork.AttributeDefinitions.Remove(definition);
        await _unitOfWork.SaveChangesAsync();
        return Ok();
    }
}
