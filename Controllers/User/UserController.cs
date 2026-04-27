using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using LocalRAG.Constants;
using LocalRAG.Extensions;
using LocalRAG.DTOs.UserModels;
using LocalRAG.DTOs.ActionModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;

namespace LocalRAG.Controllers.User;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserProfileService userProfileService, IUnitOfWork unitOfWork, ILogger<UserController> logger)
    {
        _userProfileService = userProfileService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 내 일정 조회 (로그인한 사용자)
    /// </summary>
    [HttpGet("my-schedules")]
    public async Task<IActionResult> GetMySchedules([FromQuery] int conventionId)
    {
        try
        {
            var userId = User.GetUserId();
            var schedules = await _userProfileService.GetMySchedulesAsync(userId, conventionId);
            return Ok(schedules);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "일정 조회 실패");
            return StatusCode(500, new { message = "일정을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 참가자 목록 조회
    /// </summary>
    [HttpGet("participants")]
    public async Task<IActionResult> GetParticipants([FromQuery] int conventionId, [FromQuery] string? search = null)
    {
        try
        {
            var participants = await _userProfileService.GetParticipantsAsync(conventionId, search);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참가자 목록 조회 실패");
            return StatusCode(500, new { message = "참가자 목록을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 내 행사 종합 정보 조회
    /// </summary>
    [HttpGet("my-convention-info/{conventionId}")]
    public async Task<IActionResult> GetMyConventionInfo(int conventionId)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _userProfileService.GetMyConventionInfoAsync(userId, conventionId);
            if (result == null)
                return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "행사 종합 정보 조회 실패");
            return StatusCode(500, new { message = "정보를 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 참가자 상세 조회
    /// </summary>
    [HttpGet("participants/{id}")]
    public async Task<IActionResult> GetParticipant(int id, [FromQuery] int? conventionId = null)
    {
        try
        {
            var result = await _userProfileService.GetParticipantDetailAsync(id, conventionId);
            if (result == null)
                return NotFound(new { message = "참가자를 찾을 수 없습니다." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참가자 상세 조회 실패");
            return StatusCode(500, new { message = "참가자 정보를 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 참석자 다중 속성 일괄 할당
    /// </summary>
    [HttpPost("bulk-assign-attributes")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> BulkAssignAttributes([FromBody] BulkAssignAttributesDto dto)
    {
        if (dto == null || dto.UserMappings == null || !dto.UserMappings.Any())
        {
            return BadRequest(new { message = "요청 데이터가 비어있습니다." });
        }

        _logger.LogInformation("일괄 속성 할당 시작: ConventionId={ConventionId}, Count={Count}명",
            dto.ConventionId, dto.UserMappings.Count);

        var result = await _userProfileService.BulkAssignAttributesAsync(dto);

        return result.Success ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// 속성 템플릿으로 참석자 목록 조회 (속성 포함)
    /// </summary>
    [HttpGet("participants-with-attributes")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetParticipantsWithAttributes([FromQuery] int conventionId)
    {
        try
        {
            var result = await _userProfileService.GetParticipantsWithAttributesAsync(conventionId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참석자 목록 조회 실패");
            return StatusCode(500, new { message = "참석자 목록을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 여행 정보 제출 (PROFILE_OVERSEAS 액션)
    /// </summary>
    [HttpPut("my-travel-info")]
    public async Task<IActionResult> UpdateMyTravelInfo([FromQuery] int conventionId, [FromBody] TravelInfoDto dto)
    {
        try
        {
            var userId = User.GetUserId();
            var (success, message) = await _userProfileService.SubmitTravelInfoAsync(userId, conventionId, dto);

            if (!success)
                return NotFound(new { message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Travel info update failed");
            return StatusCode(500, new { message = "여행 정보 저장 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 현재 로그인한 사용자의 체크리스트 조회
    /// </summary>
    [HttpGet("my-checklist")]
    public async Task<IActionResult> GetMyChecklist([FromQuery] int conventionId)
    {
        try
        {
            var userId = User.GetUserIdOrNull();
            if (userId == null)
            {
                return BadRequest(new { message = "사용자 정보를 찾을 수 없습니다." });
            }

            var (exists, checklist) = await _userProfileService.GetMyChecklistAsync(userId.Value, conventionId);

            if (!exists)
                return NotFound(new { message = "해당 행사의 참석자가 아닙니다." });

            return Ok(checklist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get checklist failed");
            return StatusCode(500, new { message = "체크리스트 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 내 정보 조회
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.GetUserId();
        var profile = await _userProfileService.GetProfileAsync(userId);

        if (profile == null)
            return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });

        return Ok(profile);
    }

    /// <summary>
    /// 내 정보 수정
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userId = User.GetUserId();
        var (success, errorMessage) = await _userProfileService.UpdateProfileAsync(userId, dto);

        if (!success)
        {
            if (errorMessage == "사용자 정보를 찾을 수 없습니다.")
                return NotFound(new { message = errorMessage });
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "정보가 수정되었습니다." });
    }

    /// <summary>
    /// 내 정보 단일 필드 수정
    /// </summary>
    [HttpPatch("profile/field")]
    public async Task<IActionResult> UpdateProfileField([FromBody] UpdateProfileFieldRequest request)
    {
        var userId = User.GetUserId();
        var (success, errorMessage) = await _userProfileService.UpdateProfileFieldAsync(userId, request);

        if (!success)
        {
            if (errorMessage == "사용자 정보를 찾을 수 없습니다.")
                return NotFound(new { message = errorMessage });
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "정보가 수정되었습니다." });
    }

    /// <summary>
    /// 비밀번호 변경
    /// </summary>
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.GetUserId();
        var (success, errorMessage) = await _userProfileService.ChangePasswordAsync(userId, dto);

        if (!success)
        {
            if (errorMessage == "사용자 정보를 찾을 수 없습니다.")
                return NotFound(new { message = errorMessage });
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "비밀번호가 변경되었습니다." });
    }

    /// <summary>
    /// 기본 비밀번호 경고 다시 보지 않기
    /// </summary>
    [HttpPost("dismiss-password-warning")]
    public async Task<IActionResult> DismissPasswordWarning()
    {
        var userId = User.GetUserId();
        var user = await _unitOfWork.Users.Query.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();
        user.DefaultPasswordDismissed = true;
        await _unitOfWork.SaveChangesAsync();
        return Ok(new { message = "경고가 숨겨졌습니다." });
    }

    /// <summary>
    /// 프로필 사진 업로드
    /// </summary>
    [HttpPost("profile/photo")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "파일이 비어있습니다." });

        try
        {
            var userId = User.GetUserId();
            var (success, errorMessage, url) = await _userProfileService.UploadProfilePhotoAsync(userId, file);

            if (!success)
            {
                if (errorMessage == "사용자 정보를 찾을 수 없습니다.")
                    return NotFound(new { message = errorMessage });
                return StatusCode(500, new { message = errorMessage });
            }

            return Ok(new { message = "프로필 사진이 성공적으로 업로드되었습니다.", profileImageUrl = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "프로필 사진 업로드 실패");
            return StatusCode(500, new { message = "프로필 사진 업로드 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 여권 사진 업로드
    /// </summary>
    [HttpPost("profile/passport-image")]
    public async Task<IActionResult> UploadPassportImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "파일이 비어있습니다." });

        try
        {
            var userId = User.GetUserId();
            var (success, errorMessage, url) = await _userProfileService.UploadPassportImageAsync(userId, file);

            if (!success)
            {
                if (errorMessage == "사용자 정보를 찾을 수 없습니다.")
                    return NotFound(new { message = errorMessage });
                return StatusCode(500, new { message = errorMessage });
            }

            return Ok(new { message = "여권 사진이 성공적으로 업로드되었습니다.", passportImageUrl = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "여권 사진 업로드 실패");
            return StatusCode(500, new { message = "여권 사진 업로드 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 내가 참여 중인 행사 목록 조회
    /// </summary>
    [HttpGet("conventions")]
    public async Task<IActionResult> GetMyConventions()
    {
        try
        {
            var userId = User.GetUserId();
            var conventions = await _userProfileService.GetUserConventionsAsync(userId);
            return Ok(conventions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "행사 목록 조회 실패");
            return StatusCode(500, new { message = "행사 목록 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 메인홈 대시보드 — 진행중인 행사의 준비 현황 + 다가오는 일정 + 최신 공지
    /// </summary>
    [HttpGet("home-dashboard")]
    public async Task<IActionResult> GetHomeDashboard()
    {
        try
        {
            var userId = User.GetUserId();
            var dashboard = await _userProfileService.GetHomeDashboardAsync(userId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "홈 대시보드 조회 실패");
            return StatusCode(500, new { message = "홈 대시보드 조회 중 오류가 발생했습니다." });
        }
    }
}
