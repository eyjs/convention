using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities.FormBuilder;
using System.Security.Claims;
using System.Text.Json;
using System.IO;
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

// ... (기존 코드)

    /// <summary>
    /// 폼 데이터 제출 (Create/Update)
    /// </summary>
    /// <param name="formDefinitionId">FormDefinition ID</param>
    /// <param name="requestDto">제출할 폼 데이터, 파일 및 파일 필드 키</param>
    [HttpPost("{formDefinitionId}/submit")]
    public async Task<IActionResult> SubmitFormData(
        int formDefinitionId,
        [FromForm] FormSubmissionRequestDto requestDto)
    {
        _logger.LogInformation("SubmitFormData called for FormDefinitionId: {FormDefinitionId}", formDefinitionId); // 로깅 추가
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

        // JSON 데이터 파싱
        _logger.LogInformation("Received FormDataJson: {FormDataJson}", requestDto.FormDataJson); // 로깅 추가

        if (string.IsNullOrWhiteSpace(requestDto.FormDataJson))
        {
            _logger.LogError("FormDataJson is null or whitespace.");
            return BadRequest(new { message = "제출된 폼 데이터가 비어있습니다." });
        }

        var tempDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(requestDto.FormDataJson);
        if (tempDict == null)
        {
            _logger.LogError("Failed to deserialize FormDataJson: {FormDataJson}", requestDto.FormDataJson); // 로깅 추가
            return BadRequest(new { message = "제출된 폼 데이터가 유효하지 않습니다." });
        }

        // 파일 처리
        if (requestDto.File != null && requestDto.File.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + requestDto.File.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await requestDto.File.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{uniqueFileName}";
            
            // fileFieldKey가 제공되면 해당 키의 값을 파일 URL로 업데이트
            if (!string.IsNullOrEmpty(requestDto.FileFieldKey) && tempDict.ContainsKey(requestDto.FileFieldKey))
            {
                tempDict[requestDto.FileFieldKey] = JsonDocument.Parse($"\"{fileUrl}\"").RootElement;
            }
            else
            {
                // fileFieldKey가 없거나 해당 키가 formDataJson에 없으면 새로운 속성으로 추가
                tempDict.Add("uploadedFileUrl", JsonDocument.Parse($"\"{fileUrl}\"").RootElement);
            }
            requestDto.FormDataJson = JsonSerializer.Serialize(tempDict); // 수정된 JSON으로 업데이트
        }

        var submission = await _context.FormSubmissions
            .FirstOrDefaultAsync(s => s.FormDefinitionId == formDefinitionId && s.UserId == userId);

        if (submission != null)
        {
            // Update
            submission.SubmissionDataJson = requestDto.FormDataJson; // 수정된 formDataJson 사용
            submission.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create
            submission = new FormSubmission
            {
                FormDefinitionId = formDefinitionId,
                UserId = userId,
                SubmissionDataJson = requestDto.FormDataJson, // 수정된 formDataJson 사용
            };
            _context.FormSubmissions.Add(submission);
        }

        // 연결된 ConventionAction의 상태 업데이트
        var action = await _context.ConventionActions
            .FirstOrDefaultAsync(a => a.TargetId == formDefinitionId &&
                                     a.BehaviorType == Entities.Action.BehaviorType.FormBuilder);

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

    /// <summary>
    /// 업로드된 파일 다운로드
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/files/download")]
    public IActionResult DownloadFile([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return BadRequest(new { message = "파일 경로가 필요합니다." });
        }

        // 경로 검증 (보안)
        if (path.Contains("..") || !path.StartsWith("/uploads/"))
        {
            return BadRequest(new { message = "잘못된 파일 경로입니다." });
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.TrimStart('/'));

        if (!global::System.IO.File.Exists(filePath))
        {
            return NotFound(new { message = "파일을 찾을 수 없습니다." });
        }

        var fileBytes = global::System.IO.File.ReadAllBytes(filePath);
        var fileName = Path.GetFileName(filePath);
        var contentType = GetContentType(fileName);

        return base.File(fileBytes, contentType, fileName);
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}