using LocalRAG.DTOs.UploadModels;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers;

/// <summary>
/// Excel 업로드 관련 API
/// - 참석자 업로드
/// - 일정 템플릿 업로드
/// - 속성 업로드
/// - 그룹-일정 매핑
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // 인증 필요
public class UploadController : ControllerBase
{
    private readonly IUserUploadService _userUploadService;
    private readonly IScheduleTemplateUploadService _scheduleTemplateUploadService;
    private readonly IAttributeUploadService _attributeUploadService;
    private readonly IGroupScheduleMappingService _groupScheduleMappingService;
    private readonly INameTagUploadService _nameTagUploadService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        IUserUploadService userUploadService,
        IScheduleTemplateUploadService scheduleTemplateUploadService,
        IAttributeUploadService attributeUploadService,
        IGroupScheduleMappingService groupScheduleMappingService,
        INameTagUploadService nameTagUploadService,
        ILogger<UploadController> logger)
    {
        _userUploadService = userUploadService;
        _scheduleTemplateUploadService = scheduleTemplateUploadService;
        _attributeUploadService = attributeUploadService;
        _groupScheduleMappingService = groupScheduleMappingService;
        _nameTagUploadService = nameTagUploadService;
        _logger = logger;
    }

    /// <summary>
    /// 참석자 정보 업로드
    /// Excel 형식: [소속|부서|이름|사번(주민번호)|전화번호|그룹]
    /// </summary>
    [HttpPost("conventions/{conventionId}/guests")]
    [ProducesResponseType(typeof(UserUploadResult), 200)]
    public async Task<IActionResult> UploadGuests(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "파일이 비어있습니다." });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });
        }

        _logger.LogInformation("Uploading guests for convention {ConventionId}, file: {FileName}",
            conventionId, file.FileName);

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _userUploadService.UploadUsersAsync(conventionId, stream);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload guests");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 일정 템플릿 업로드
    /// Excel 형식:
    /// - A열: "월/일(요일)_일정명_시:분" (예: "11/03(일)_조식_07:30")
    /// - B열: 상세 내용 (엑셀 내부 줄바꿈 지원)
    /// </summary>
    [HttpPost("conventions/{conventionId}/schedule-templates")]
    [ProducesResponseType(typeof(ScheduleTemplateUploadResult), 200)]
    public async Task<IActionResult> UploadScheduleTemplates(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "파일이 비어있습니다." });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });
        }

        _logger.LogInformation("Uploading schedule templates for convention {ConventionId}, file: {FileName}",
            conventionId, file.FileName);

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _scheduleTemplateUploadService.UploadScheduleTemplatesAsync(conventionId, stream);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload schedule templates");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 참석자 속성 업로드
    /// Excel 형식: [이름|전화번호|속성1|속성2|...]
    /// 통계 정보 포함
    /// </summary>
    [HttpPost("conventions/{conventionId}/attributes")]
    [ProducesResponseType(typeof(UserAttributeUploadResult), 200)]
    public async Task<IActionResult> UploadAttributes(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "파일이 비어있습니다." });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });
        }

        _logger.LogInformation("Uploading attributes for convention {ConventionId}, file: {FileName}",
            conventionId, file.FileName);

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _attributeUploadService.UploadAttributesAsync(conventionId, stream);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload attributes");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 그룹-일정 매핑 (일괄 배정)
    /// 특정 그룹의 모든 참석자에게 여러 일정을 한 번에 배정
    /// </summary>
    [HttpPost("schedule-mapping/group")]
    [ProducesResponseType(typeof(GroupScheduleMappingResult), 200)]
    public async Task<IActionResult> MapGroupToSchedules([FromBody] GroupScheduleMappingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Mapping group '{Group}' to {ActionCount} actions in convention {ConventionId}",
            request.UserGroup, request.ActionIds.Count, request.ConventionId);

        try
        {
            var result = await _groupScheduleMappingService.MapGroupToSchedulesAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map group to schedules");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 행사의 모든 그룹 목록 조회
    /// </summary>
    [HttpGet("conventions/{conventionId}/groups")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public async Task<IActionResult> GetGroups(int conventionId)
    {
        _logger.LogInformation("Getting groups for convention {ConventionId}", conventionId);

        try
        {
            var groups = await _groupScheduleMappingService.GetGroupsInConventionAsync(conventionId);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get groups");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 명찰 인쇄용 데이터 업로드
    /// Excel 형식: [번호|테이블명(Group)|이름1|직책1|이름2|직책2|...]
    /// 이 엔드포인트는 데이터를 DB에 저장하지 않고, 파싱된 결과를 바로 반환하여 인쇄 미리보기 생성에 사용됩니다.
    /// </summary>
    [HttpPost("conventions/{conventionId}/name-tags")]
    [ProducesResponseType(typeof(NameTagUploadResult), 200)]
    public async Task<IActionResult> UploadNameTagsForPrinting(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "파일이 비어있습니다." });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });
        }

        _logger.LogInformation("Uploading name tags for printing for convention {ConventionId}, file: {FileName}",
            conventionId, file.FileName);

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _nameTagUploadService.UploadNameTagsAsync(stream);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload name tags for printing");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }
}
