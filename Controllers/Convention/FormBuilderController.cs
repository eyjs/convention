using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using LocalRAG.DTOs.FormBuilder;
using LocalRAG.Interfaces;

namespace LocalRAG.Controllers.Convention;

/// <summary>
/// FormBuilder 시스템 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/forms")]
[Authorize]
public class FormBuilderController : ControllerBase
{
    private readonly IFormBuilderService _formBuilderService;
    private readonly ILogger<FormBuilderController> _logger;

    public FormBuilderController(
        IFormBuilderService formBuilderService,
        ILogger<FormBuilderController> logger)
    {
        _formBuilderService = formBuilderService;
        _logger = logger;
    }

    /// <summary>
    /// 폼 설계도 조회
    /// </summary>
    /// <param name="id">FormDefinition ID</param>
    [HttpGet("{id}/definition")]
    public async Task<IActionResult> GetFormDefinition(int id)
    {
        var formDto = await _formBuilderService.GetFormDefinitionAsync(id);

        if (formDto == null)
        {
            return NotFound(new { message = "폼을 찾을 수 없습니다." });
        }

        return Ok(formDto);
    }

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

        var submissionJson = await _formBuilderService.GetUserSubmissionAsync(formDefinitionId, userId);

        if (submissionJson == null)
        {
            return NotFound(new { message = "제출 데이터가 없습니다." });
        }

        var jsonData = JsonDocument.Parse(submissionJson);
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("사용자 정보를 확인할 수 없습니다.");
        }

        var success = await _formBuilderService.SubmitFormDataAsync(
            formDefinitionId,
            userId,
            requestDto.FormDataJson,
            requestDto.File,
            requestDto.FileFieldKey);

        if (!success)
        {
            return BadRequest(new { message = "폼 제출에 실패했습니다." });
        }

        return Ok(new { message = "제출이 완료되었습니다." });
    }

    /// <summary>
    /// [관리자용] 특정 폼의 모든 제출 데이터 조회
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("{formDefinitionId}/submissions/all")]
    public async Task<IActionResult> GetAllSubmissions(int formDefinitionId)
    {
        var submissions = await _formBuilderService.GetAllSubmissionsAsync(formDefinitionId);

        if (submissions.Count == 0)
        {
            var formExists = await _formBuilderService.GetFormDefinitionAsync(formDefinitionId);
            if (formExists == null)
            {
                return NotFound(new { message = "폼을 찾을 수 없습니다." });
            }
        }

        return Ok(submissions);
    }

    /// <summary>
    /// 업로드된 파일 다운로드
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/files/download")]
    public IActionResult DownloadFile([FromQuery] string path)
    {
        if (!_formBuilderService.ValidateFilePath(path))
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
        var contentType = _formBuilderService.GetContentType(fileName);

        return base.File(fileBytes, contentType, fileName);
    }
}