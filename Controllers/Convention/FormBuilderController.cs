using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities.FormBuilder;
using System.Security.Claims;
using System.Text.Json;
using LocalRAG.DTOs.FormBuilder; // DTO 네임스페이스 추가

namespace LocalRAG.Controllers.Convention;

/// <summary>
/// FormBuilder 시스템 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/forms")]
[Authorize]
public class FormBuilderController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<FormBuilderController> _logger;

    public FormBuilderController(
        ConventionDbContext context,
        ILogger<FormBuilderController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 폼 설계도 조회
    /// </summary>
    /// <param name="id">FormDefinition ID</param>
    [HttpGet("{id}/definition")]
    public async Task<IActionResult> GetFormDefinition(int id)
    {
        var formDto = await _context.FormDefinitions
            .Where(f => f.Id == id)
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
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (formDto == null)
        {
            return NotFound(new { message = "폼을 찾을 수 없습니다." });
        }

        return Ok(formDto);
    }

    // ... 이하 다른 메소드들은 유지 ...
    /// <summary>
    /// 사용자의 기존 응답 조회
    /// </summary>
    [HttpGet("submission/{formDefinitionId}")]
    public async Task<IActionResult> GetMySubmission(int formDefinitionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        var submission = await _context.FormSubmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.FormDefinitionId == formDefinitionId && s.UserId == userId);

        if (submission == null)
        {
            return NotFound(new { message = "제출 데이터가 없습니다." });
        }

        var jsonData = JsonDocument.Parse(submission.SubmissionDataJson);
        return Ok(jsonData.RootElement);
    }

    /// <summary>
    /// 폼 데이터 제출 (Create/Update)
    /// </summary>
    [HttpPost("{formDefinitionId}/submit")]
    public async Task<IActionResult> SubmitFormData(int formDefinitionId, [FromBody] JsonElement payload)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        if (!await _context.FormDefinitions.AnyAsync(f => f.Id == formDefinitionId))
        {
            return BadRequest(new { message = "폼을 찾을 수 없습니다." });
        }

        var submission = await _context.FormSubmissions
            .FirstOrDefaultAsync(s => s.FormDefinitionId == formDefinitionId && s.UserId == userId);

        if (submission != null)
        {
            submission.SubmissionDataJson = payload.ToString();
            submission.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            submission = new FormSubmission
            {
                FormDefinitionId = formDefinitionId,
                UserId = userId,
                SubmissionDataJson = payload.ToString(),
            };
            _context.FormSubmissions.Add(submission);
        }

        var action = await _context.ConventionActions
            .FirstOrDefaultAsync(a => a.TargetId == formDefinitionId &&
                                     a.BehaviorType == Entities.Action.ActionBehaviorType.FormBuilder);

        if (action != null)
        {
            var status = await _context.UserActionStatuses
                .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);

            if (status == null)
            {
                _context.UserActionStatuses.Add(new LocalRAG.Entities.UserActionStatus
                {
                    UserId = userId,
                    ConventionActionId = action.Id,
                    IsComplete = true,
                    CompletedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                status.IsComplete = true;
                status.CompletedAt = DateTime.UtcNow;
                status.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "제출이 완료되었습니다." });
    }

    /// <summary>
    /// [관리자용] 특정 폼의 모든 제출 데이터 조회
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("{formDefinitionId}/submissions/all")]
    public async Task<IActionResult> GetAllSubmissions(int formDefinitionId)
    {
        if (!await _context.FormDefinitions.AnyAsync(f => f.Id == formDefinitionId))
        {
            return NotFound(new { message = "폼을 찾을 수 없습니다." });
        }

        var submissionsRaw = await _context.FormSubmissions
            .Include(s => s.User)
            .Where(s => s.FormDefinitionId == formDefinitionId)
            .ToListAsync();

        var submissions = submissionsRaw.Select(s => new
        {
            s.Id,
            s.UserId,
            UserName = s.User?.Name,
            UserEmail = s.User?.Email,
            SubmissionData = JsonDocument.Parse(s.SubmissionDataJson).RootElement,
            s.SubmittedAt,
            s.UpdatedAt
        }).ToList();

        return Ok(submissions);
    }
}