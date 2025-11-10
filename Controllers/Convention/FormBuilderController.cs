using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities.FormBuilder;
using System.Security.Claims;
using System.Text.Json;

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
        var formDef = await _context.FormDefinitions
            .Include(f => f.Fields.OrderBy(field => field.OrderIndex))
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (formDef == null)
        {
            return NotFound(new { message = "폼을 찾을 수 없습니다." });
        }

        return Ok(formDef);
    }

    /// <summary>
    /// 사용자의 기존 응답 조회
    /// </summary>
    /// <param name="formDefinitionId">FormDefinition ID</param>
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

        // JSON 문자열을 객체로 파싱하여 반환
        var jsonData = JsonDocument.Parse(submission.SubmissionDataJson);
        return Ok(jsonData.RootElement);
    }

    /// <summary>
    /// 폼 데이터 제출 (Create/Update)
    /// </summary>
    /// <param name="formDefinitionId">FormDefinition ID</param>
    /// <param name="payload">제출할 JSON 데이터</param>
    [HttpPost("{formDefinitionId}/submit")]
    public async Task<IActionResult> SubmitFormData(int formDefinitionId, [FromBody] JsonElement payload)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        // FormDefinition 존재 확인
        if (!await _context.FormDefinitions.AnyAsync(f => f.Id == formDefinitionId))
        {
            return BadRequest(new { message = "폼을 찾을 수 없습니다." });
        }

        var submission = await _context.FormSubmissions
            .FirstOrDefaultAsync(s => s.FormDefinitionId == formDefinitionId && s.UserId == userId);

        if (submission != null)
        {
            // Update
            submission.SubmissionDataJson = payload.ToString();
            submission.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create
            submission = new FormSubmission
            {
                FormDefinitionId = formDefinitionId,
                UserId = userId,
                SubmissionDataJson = payload.ToString(),
            };
            _context.FormSubmissions.Add(submission);
        }

        // 연결된 ConventionAction의 상태 업데이트
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
    /// <param name="formDefinitionId">FormDefinition ID</param>
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

        // JSON 파싱은 메모리에서 수행
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
