using LocalRAG.Data;
using LocalRAG.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IAuthService _authService;

    public SetupController(ConventionDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    /// <summary>
    /// 관리자 계정 생성 (개발용)
    /// </summary>
    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateAdmin()
    {
        try
        {
            var existingAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.LoginId == "admin");

            if (existingAdmin != null)
            {
                return BadRequest(new { message = "관리자 계정이 이미 존재합니다." });
            }

            var admin = new Entities.User
            {
                LoginId = "admin",
                PasswordHash = _authService.HashPassword("admin123"),
                Name = "관리자",
                Email = "admin@convention.com",
                Phone = "010-0000-0000",
                Role = "Admin",
                IsActive = true,
                EmailVerified = true,
                PhoneVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "관리자 계정이 생성되었습니다.",
                loginId = "admin",
                password = "admin123",
                userId = admin.Id
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"계정 생성 실패: {ex.Message}" });
        }
    }
}
