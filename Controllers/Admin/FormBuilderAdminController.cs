using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities.FormBuilder;
using LocalRAG.DTOs.FormBuilder;
using System.Security.Claims;

namespace LocalRAG.Controllers.Admin;

/// <summary>
/// 어드민용 FormBuilder 관리 API
/// </summary>
[ApiController]
[Route("api/admin/conventions/{conventionId}/forms")]
[Authorize(Roles = "Admin")]
public class FormBuilderAdminController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<FormBuilderAdminController> _logger;

    public FormBuilderAdminController(
        ConventionDbContext context,
        ILogger<FormBuilderAdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 특정 행사의 폼 목록 조회
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetForms(int conventionId)
    {
        try
        {
            var forms = await _context.FormDefinitions
                .Include(f => f.Fields)
                .Where(f => f.ConventionId == conventionId)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking()
                .Select(f => new FormDefinitionDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    ConventionId = f.ConventionId,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    Fields = f.Fields.OrderBy(field => field.OrderIndex).Select(field => new FormFieldDto
                    {
                        Id = field.Id,
                        Key = field.Key,
                        Label = field.Label,
                        FieldType = field.FieldType,
                        OrderIndex = field.OrderIndex,
                        IsRequired = field.IsRequired,
                        Placeholder = field.Placeholder,
                        OptionsJson = field.OptionsJson
                    }).ToList()
                })
                .ToListAsync();

            return Ok(forms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "폼 목록 조회 중 오류 발생. ConventionId={ConventionId}", conventionId);
            return StatusCode(500, new { message = "폼 목록 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 폼 생성
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateForm(int conventionId, [FromBody] FormDefinition formDefinition)
    {
        try
        {
            // ConventionId 확인 및 설정
            if (formDefinition.ConventionId != conventionId)
            {
                formDefinition.ConventionId = conventionId;
            }

            // 행사 존재 확인
            var conventionExists = await _context.Conventions.AnyAsync(c => c.Id == conventionId);
            if (!conventionExists)
            {
                return NotFound(new { message = "행사를 찾을 수 없습니다." });
            }

            // 타임스탬프 설정
            formDefinition.CreatedAt = DateTime.UtcNow;
            formDefinition.UpdatedAt = null;

            // 필드 OrderIndex 설정
            if (formDefinition.Fields != null)
            {
                for (int i = 0; i < formDefinition.Fields.Count; i++)
                {
                    formDefinition.Fields.ElementAt(i).OrderIndex = i;
                }
            }

            _context.FormDefinitions.Add(formDefinition);
            await _context.SaveChangesAsync();

            // 생성된 폼 다시 로드 (필드 포함) - DTO로 변환
            var createdForm = await _context.FormDefinitions
                .Include(f => f.Fields)
                .Where(f => f.Id == formDefinition.Id)
                .AsNoTracking()
                .Select(f => new FormDefinitionDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    ConventionId = f.ConventionId,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    Fields = f.Fields.OrderBy(field => field.OrderIndex).Select(field => new FormFieldDto
                    {
                        Id = field.Id,
                        Key = field.Key,
                        Label = field.Label,
                        FieldType = field.FieldType,
                        OrderIndex = field.OrderIndex,
                        IsRequired = field.IsRequired,
                        Placeholder = field.Placeholder,
                        OptionsJson = field.OptionsJson
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(
                nameof(GetFormById),
                new { conventionId = conventionId, id = formDefinition.Id },
                createdForm
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "폼 생성 중 오류 발생. ConventionId={ConventionId}", conventionId);
            return StatusCode(500, new { message = "폼 생성 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 특정 폼 조회
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFormById(int conventionId, int id)
    {
        try
        {
            var form = await _context.FormDefinitions
                .Include(f => f.Fields)
                .Where(f => f.Id == id && f.ConventionId == conventionId)
                .AsNoTracking()
                .Select(f => new FormDefinitionDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    ConventionId = f.ConventionId,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    Fields = f.Fields.OrderBy(field => field.OrderIndex).Select(field => new FormFieldDto
                    {
                        Id = field.Id,
                        Key = field.Key,
                        Label = field.Label,
                        FieldType = field.FieldType,
                        OrderIndex = field.OrderIndex,
                        IsRequired = field.IsRequired,
                        Placeholder = field.Placeholder,
                        OptionsJson = field.OptionsJson
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (form == null)
            {
                return NotFound(new { message = "폼을 찾을 수 없습니다." });
            }

            return Ok(form);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "폼 조회 중 오류 발생. FormId={FormId}", id);
            return StatusCode(500, new { message = "폼 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 폼 수정
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateForm(int conventionId, int id, [FromBody] FormDefinition updatedForm)
    {
        try
        {
            var existingForm = await _context.FormDefinitions
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

            if (existingForm == null)
            {
                return NotFound(new { message = "폼을 찾을 수 없습니다." });
            }

            // 기본 정보 업데이트
            existingForm.Name = updatedForm.Name;
            existingForm.Description = updatedForm.Description;
            existingForm.UpdatedAt = DateTime.UtcNow;

            // 기존 필드 삭제
            _context.FormFields.RemoveRange(existingForm.Fields);

            // 새 필드 추가
            if (updatedForm.Fields != null && updatedForm.Fields.Any())
            {
                foreach (var field in updatedForm.Fields)
                {
                    field.FormDefinitionId = existingForm.Id;
                    field.Id = 0; // 새 필드로 생성
                    _context.FormFields.Add(field);
                }
            }

            await _context.SaveChangesAsync();

            // 업데이트된 폼 다시 로드 - DTO로 변환
            var result = await _context.FormDefinitions
                .Include(f => f.Fields)
                .Where(f => f.Id == id)
                .AsNoTracking()
                .Select(f => new FormDefinitionDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    ConventionId = f.ConventionId,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    Fields = f.Fields.OrderBy(field => field.OrderIndex).Select(field => new FormFieldDto
                    {
                        Id = field.Id,
                        Key = field.Key,
                        Label = field.Label,
                        FieldType = field.FieldType,
                        OrderIndex = field.OrderIndex,
                        IsRequired = field.IsRequired,
                        Placeholder = field.Placeholder,
                        OptionsJson = field.OptionsJson
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "폼 수정 중 오류 발생. FormId={FormId}", id);
            return StatusCode(500, new { message = "폼 수정 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 폼 삭제
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteForm(int conventionId, int id)
    {
        try
        {
            var form = await _context.FormDefinitions
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

            if (form == null)
            {
                return NotFound(new { message = "폼을 찾을 수 없습니다." });
            }

            // 이 폼을 사용하는 액션이 있는지 확인
            var linkedActions = await _context.ConventionActions
                .Where(a => a.BehaviorType == Entities.Action.BehaviorType.FormBuilder
                         && a.TargetId == id)
                .ToListAsync();

            if (linkedActions.Any())
            {
                return BadRequest(new
                {
                    message = $"이 폼을 사용하는 {linkedActions.Count}개의 액션이 있어 삭제할 수 없습니다. 먼저 액션을 삭제하거나 수정하세요.",
                    linkedActionIds = linkedActions.Select(a => a.Id).ToList()
                });
            }

            // 관련 제출 데이터 삭제
            var submissions = await _context.FormSubmissions
                .Where(s => s.FormDefinitionId == id)
                .ToListAsync();

            if (submissions.Any())
            {
                _context.FormSubmissions.RemoveRange(submissions);
            }

            // 필드 삭제 (Cascade로 자동 삭제될 수도 있지만 명시적으로)
            _context.FormFields.RemoveRange(form.Fields);

            // 폼 삭제
            _context.FormDefinitions.Remove(form);

            await _context.SaveChangesAsync();

            return Ok(new { message = "폼이 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "폼 삭제 중 오류 발생. FormId={FormId}", id);
            return StatusCode(500, new { message = "폼 삭제 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 폼 통계 조회 (제출 수 등)
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetFormStats(int conventionId, int id)
    {
        try
        {
            var form = await _context.FormDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

            if (form == null)
            {
                return NotFound(new { message = "폼을 찾을 수 없습니다." });
            }

            var submissionCount = await _context.FormSubmissions
                .CountAsync(s => s.FormDefinitionId == id);

            var linkedActionsCount = await _context.ConventionActions
                .CountAsync(a => a.BehaviorType == Entities.Action.BehaviorType.FormBuilder
                              && a.TargetId == id);

            return Ok(new
            {
                formId = id,
                formName = form.Name,
                submissionCount = submissionCount,
                linkedActionsCount = linkedActionsCount,
                createdAt = form.CreatedAt,
                updatedAt = form.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "폼 통계 조회 중 오류 발생. FormId={FormId}", id);
            return StatusCode(500, new { message = "폼 통계 조회 중 오류가 발생했습니다." });
        }
    }
}
