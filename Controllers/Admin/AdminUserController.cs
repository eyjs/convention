using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Constants;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminUserController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUserController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
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

    [HttpPost("guests/{guestId}/send-sms")]
    public async Task<IActionResult> SendSMS(int guestId, [FromQuery] int? conventionId = null)
    {
        // SMS 전송 기능은 추후 구현 예정
        return Ok(new { message = "SMS 전송 기능은 추후 구현 예정입니다." });
    }
}
