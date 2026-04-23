using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Constants;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminUserController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly IUserProfileService _userProfileService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminUserController(IAdminUserService adminUserService, IUserProfileService userProfileService, IUnitOfWork unitOfWork)
    {
        _adminUserService = adminUserService;
        _userProfileService = userProfileService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("conventions/{conventionId}/guests")]
    public async Task<IActionResult> GetGuests(int conventionId)
    {
        var guests = await _adminUserService.GetGuestsAsync(conventionId);
        return Ok(guests);
    }

    [HttpGet("guests/{guestId}/detail")]
    public async Task<IActionResult> GetGuestDetail(int guestId)
    {
        var result = await _adminUserService.GetGuestDetailAsync(guestId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("conventions/{conventionId}/guests")]
    public async Task<IActionResult> CreateGuest(int conventionId, [FromBody] UserDto dto)
    {
        var (success, result, statusCode) = await _adminUserService.CreateGuestAsync(conventionId, dto);
        return StatusCode(statusCode, result);
    }

    [HttpPut("guests/{id}")]
    public async Task<IActionResult> UpdateGuest(int id, [FromBody] UserDto dto)
    {
        var (success, result, statusCode) = await _adminUserService.UpdateGuestAsync(id, dto);
        return StatusCode(statusCode, result);
    }

    [HttpDelete("guests/{id}")]
    public async Task<IActionResult> DeleteGuest(int id)
    {
        var (found, statusCode) = await _adminUserService.DeleteGuestAsync(id);
        if (!found) return NotFound();
        return Ok();
    }

    [HttpPost("guests/{guestId}/convert-to-user")]
    public async Task<IActionResult> ConvertToUser(int guestId, [FromBody] CreateUserDto dto)
    {
        var (success, result, statusCode) = await _adminUserService.ConvertToUserAsync(guestId, dto);
        return StatusCode(statusCode, result);
    }

    [HttpPut("guests/{guestId}/user")]
    public async Task<IActionResult> UpdateUserForGuest(int guestId, [FromBody] UpdateUserDto dto)
    {
        var (success, result, statusCode) = await _adminUserService.UpdateUserRoleAsync(guestId, dto);
        return StatusCode(statusCode, result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminUserService.GetUsersAsync(searchTerm, role, page, pageSize);
        return Ok(result);
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        var (found, result, statusCode) = await _adminUserService.ToggleUserStatusAsync(id, dto);
        if (!found) return NotFound();
        return Ok(result);
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        var (success, result, statusCode) = await _adminUserService.UpdateUserRoleDirectAsync(id, dto);
        return StatusCode(statusCode, result);
    }

    [HttpPost("users/{userId}/reset-password")]
    public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetPasswordDto dto)
    {
        var (success, result, statusCode) = await _adminUserService.ResetPasswordAsync(userId, dto);
        return StatusCode(statusCode, result);
    }

    [HttpGet("guests/{guestId}/access-link")]
    public async Task<IActionResult> GetAccessLink(int guestId, [FromQuery] int? conventionId = null)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var (found, result) = await _adminUserService.GetAccessLinkAsync(guestId, conventionId, baseUrl);
        if (!found) return NotFound();
        return Ok(result);
    }

    [HttpPost("conventions/{conventionId}/guests/link")]
    public async Task<IActionResult> LinkExistingUsers(int conventionId, [FromBody] LinkUserDto dto)
    {
        var (success, result, statusCode) = await _adminUserService.LinkExistingUsersAsync(conventionId, dto.UserIds, dto.GroupName);
        return StatusCode(statusCode, result);
    }

    [HttpPut("users/{userId}/passport-verification")]
    public async Task<IActionResult> TogglePassportVerification(int userId, [FromBody] PassportVerificationDto dto)
    {
        var (success, result, statusCode) = await _adminUserService.TogglePassportVerificationAsync(userId, dto.Verified);
        return StatusCode(statusCode, result);
    }

    [HttpPost("guests/{guestId}/passport-image")]
    public async Task<IActionResult> UploadGuestPassportImage(int guestId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "파일이 없습니다." });

        var (success, errorMessage, url) = await _userProfileService.UploadPassportImageAsync(guestId, file);
        if (!success)
            return BadRequest(new { message = errorMessage });

        return Ok(new { url });
    }

    // === 여권 검증 대시보드 ===

    [HttpGet("conventions/{conventionId}/passport-stats")]
    public async Task<IActionResult> GetPassportStats(int conventionId)
    {
        var users = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
            .Select(uc => uc.User)
            .ToListAsync();

        return Ok(new
        {
            total = users.Count,
            approved = users.Count(u => u.PassportVerified),
            pending = users.Count(u => !u.PassportVerified && !string.IsNullOrEmpty(u.PassportImageUrl)),
            rejected = users.Count(u => u.PassportRejectedAt != null && !u.PassportVerified),
            unregistered = users.Count(u => !u.PassportVerified && string.IsNullOrEmpty(u.PassportImageUrl) && u.PassportRejectedAt == null)
        });
    }

    [HttpGet("conventions/{conventionId}/passport-list")]
    public async Task<IActionResult> GetPassportList(int conventionId, [FromQuery] string status = "all")
    {
        var query = _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User);

        var data = await query.Select(uc => new
        {
            uc.User.Id,
            uc.User.Name,
            uc.User.Phone,
            GroupName = uc.GroupName,
            uc.User.FirstName,
            uc.User.LastName,
            uc.User.PassportNumber,
            PassportExpiryDate = uc.User.PassportExpiryDate != null ? uc.User.PassportExpiryDate.Value.ToString("yyyy-MM-dd") : null,
            uc.User.PassportImageUrl,
            uc.User.PassportVerified,
            uc.User.PassportVerifiedAt,
            uc.User.PassportRejectionReason,
            uc.User.PassportRejectedAt
        }).ToListAsync();

        var filtered = status switch
        {
            "approved" => data.Where(u => u.PassportVerified).ToList(),
            "pending" => data.Where(u => !u.PassportVerified && !string.IsNullOrEmpty(u.PassportImageUrl)).ToList(),
            "rejected" => data.Where(u => u.PassportRejectedAt != null && !u.PassportVerified).ToList(),
            "unregistered" => data.Where(u => !u.PassportVerified && string.IsNullOrEmpty(u.PassportImageUrl) && u.PassportRejectedAt == null).ToList(),
            _ => data
        };

        return Ok(filtered);
    }

    [HttpPost("guests/{guestId}/passport/approve")]
    public async Task<IActionResult> ApprovePassport(int guestId)
    {
        var user = await _unitOfWork.Users.Query.FirstOrDefaultAsync(u => u.Id == guestId);
        if (user == null) return NotFound();

        user.PassportVerified = true;
        user.PassportVerifiedAt = DateTime.UtcNow;
        user.PassportRejectionReason = null;
        user.PassportRejectedAt = null;
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "여권 승인 완료" });
    }

    [HttpPost("guests/{guestId}/passport/reject")]
    public async Task<IActionResult> RejectPassport(int guestId, [FromBody] RejectPassportRequest request)
    {
        var user = await _unitOfWork.Users.Query.FirstOrDefaultAsync(u => u.Id == guestId);
        if (user == null) return NotFound();

        user.PassportVerified = false;
        user.PassportVerifiedAt = null;
        user.PassportRejectionReason = request.Reason;
        user.PassportRejectedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "여권 거절 처리됨" });
    }

    [HttpPost("guests/{guestId}/send-sms")]
    public async Task<IActionResult> SendSMS(int guestId, [FromQuery] int? conventionId = null)
    {
        // SMS 전송 기능은 추후 구현 예정
        return Ok(new { message = "SMS 전송 기능은 추후 구현 예정입니다." });
    }
}

public class RejectPassportRequest
{
    public string Reason { get; set; } = string.Empty;
}
