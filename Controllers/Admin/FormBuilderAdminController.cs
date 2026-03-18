using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Entities.FormBuilder;
using LocalRAG.Entities.Action;
using LocalRAG.DTOs.FormBuilder;
using LocalRAG.Repositories;
using LocalRAG.Constants;

namespace LocalRAG.Controllers.Admin;

/// <summary>
/// 어드민용 FormBuilder 관리 API
/// </summary>
[ApiController]
[Route("api/admin/conventions/{conventionId}/forms")]
[Authorize(Roles = Roles.Admin)]
public class FormBuilderAdminController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FormBuilderAdminController> _logger;

    public FormBuilderAdminController(
        IUnitOfWork unitOfWork,
        ILogger<FormBuilderAdminController> logger)
    {
        _unitOfWork = unitOfWork;
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
            var forms = await _unitOfWork.FormDefinitions.Query
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
            if (formDefinition.ConventionId != conventionId)
            {
                formDefinition.ConventionId = conventionId;
            }

            var conventionExists = await _unitOfWork.Conventions.ExistsAsync(c => c.Id == conventionId);
            if (!conventionExists)
            {
                return NotFound(new { message = "행사를 찾을 수 없습니다." });
            }

            formDefinition.CreatedAt = DateTime.UtcNow;
            formDefinition.UpdatedAt = null;

            if (formDefinition.Fields != null)
            {
                for (int i = 0; i < formDefinition.Fields.Count; i++)
                {
                    formDefinition.Fields.ElementAt(i).OrderIndex = i;
                }
            }

            await _unitOfWork.FormDefinitions.AddAsync(formDefinition);
            await _unitOfWork.SaveChangesAsync();

            var createdForm = await _unitOfWork.FormDefinitions.Query
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
            var form = await _unitOfWork.FormDefinitions.Query
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
            var existingForm = await _unitOfWork.FormDefinitions.Query
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

            if (existingForm == null)
            {
                return NotFound(new { message = "폼을 찾을 수 없습니다." });
            }

            existingForm.Name = updatedForm.Name;
            existingForm.Description = updatedForm.Description;
            existingForm.UpdatedAt = DateTime.UtcNow;

            var existingFields = await _unitOfWork.FormFields.FindAsync(f => f.FormDefinitionId == existingForm.Id);
            _unitOfWork.FormFields.RemoveRange(existingFields);

            if (updatedForm.Fields != null && updatedForm.Fields.Any())
            {
                foreach (var field in updatedForm.Fields)
                {
                    field.FormDefinitionId = existingForm.Id;
                    field.Id = 0;
                    await _unitOfWork.FormFields.AddAsync(field);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.FormDefinitions.Query
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
            var form = await _unitOfWork.FormDefinitions.Query
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

            if (form == null)
            {
                return NotFound(new { message = "폼을 찾을 수 없습니다." });
            }

            var linkedActions = await _unitOfWork.ConventionActions.FindAsync(
                a => a.BehaviorType == BehaviorType.FormBuilder && a.TargetId == id);

            if (linkedActions.Any())
            {
                return BadRequest(new
                {
                    message = $"이 폼을 사용하는 {linkedActions.Count()}개의 액션이 있어 삭제할 수 없습니다. 먼저 액션을 삭제하거나 수정하세요.",
                    linkedActionIds = linkedActions.Select(a => a.Id).ToList()
                });
            }

            var submissions = await _unitOfWork.FormSubmissions.FindAsync(s => s.FormDefinitionId == id);

            if (submissions.Any())
            {
                _unitOfWork.FormSubmissions.RemoveRange(submissions);
            }

            _unitOfWork.FormFields.RemoveRange(form.Fields);
            _unitOfWork.FormDefinitions.Remove(form);

            await _unitOfWork.SaveChangesAsync();

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
            var form = await _unitOfWork.FormDefinitions
                .GetAsync(f => f.Id == id && f.ConventionId == conventionId);

            if (form == null)
            {
                return NotFound(new { message = "폼을 찾을 수 없습니다." });
            }

            var submissionCount = await _unitOfWork.FormSubmissions
                .CountAsync(s => s.FormDefinitionId == id);

            var linkedActionsCount = await _unitOfWork.ConventionActions
                .CountAsync(a => a.BehaviorType == BehaviorType.FormBuilder && a.TargetId == id);

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
