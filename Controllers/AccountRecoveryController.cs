using LocalRAG.Data;
using LocalRAG.Models;
using LocalRAG.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountRecoveryController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ISmsService _smsService;
    private readonly IVerificationService _verificationService;
    private readonly IAuthService _authService;
    private readonly ILogger<AccountRecoveryController> _logger;

    public AccountRecoveryController(
        ConventionDbContext context,
        ISmsService smsService,
        IVerificationService verificationService,
        IAuthService authService,
        ILogger<AccountRecoveryController> logger)
    {
        _context = context;
        _smsService = smsService;
        _verificationService = verificationService;
        _authService = authService;
        _logger = logger;
    }

    // 아이디 찾기 - 인증번호 발송
    [HttpPost("find-id/send-code")]
    public async Task<IActionResult> SendCodeForFindId([FromBody] FindIdSendCodeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "이름을 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return BadRequest(new { message = "전화번호를 입력해주세요." });

        // 사용자 존재 확인
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Name == dto.Name && u.Phone == dto.PhoneNumber);

        if (user == null)
            return NotFound(new { message = "입력하신 정보와 일치하는 계정을 찾을 수 없습니다." });

        // 인증번호 생성
        var key = $"find-id:{dto.PhoneNumber}";
        var code = _verificationService.GenerateCode(key);

        // SMS 발송
        var sent = await _smsService.SendVerificationCodeAsync(dto.PhoneNumber, code);
        
        if (!sent)
            return StatusCode(500, new { message = "SMS 발송에 실패했습니다. 잠시 후 다시 시도해주세요." });

        return Ok(new { message = "인증번호가 발송되었습니다." });
    }

    // 아이디 찾기 - 인증번호 확인 및 아이디 반환
    [HttpPost("find-id/verify")]
    public async Task<IActionResult> VerifyAndFindId([FromBody] FindIdVerifyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "이름을 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return BadRequest(new { message = "전화번호를 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "인증번호를 입력해주세요." });

        // 인증번호 확인
        var key = $"find-id:{dto.PhoneNumber}";
        if (!_verificationService.VerifyCode(key, dto.Code))
            return BadRequest(new { message = "인증번호가 올바르지 않거나 만료되었습니다." });

        // 사용자 조회
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Name == dto.Name && u.Phone == dto.PhoneNumber);

        if (user == null)
            return NotFound(new { message = "입력하신 정보와 일치하는 계정을 찾을 수 없습니다." });

        // 인증번호 삭제
        _verificationService.RemoveCode(key);

        // LoginId 마스킹 (보안)
        var maskedLoginId = MaskLoginId(user.LoginId);

        return Ok(new
        {
            loginId = user.LoginId,
            maskedLoginId = maskedLoginId,
            name = user.Name,
            createdAt = user.CreatedAt
        });
    }

    // 비밀번호 찾기 - 인증번호 발송
    [HttpPost("reset-password/send-code")]
    public async Task<IActionResult> SendCodeForResetPassword([FromBody] ResetPasswordSendCodeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LoginId))
            return BadRequest(new { message = "아이디를 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "이름을 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return BadRequest(new { message = "전화번호를 입력해주세요." });

        // 사용자 존재 확인
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.LoginId == dto.LoginId && u.Name == dto.Name && u.Phone == dto.PhoneNumber);

        if (user == null)
            return NotFound(new { message = "입력하신 정보와 일치하는 계정을 찾을 수 없습니다." });

        // 인증번호 생성
        var key = $"reset-password:{dto.PhoneNumber}:{dto.LoginId}";
        var code = _verificationService.GenerateCode(key);

        // SMS 발송
        var sent = await _smsService.SendVerificationCodeAsync(dto.PhoneNumber, code);
        
        if (!sent)
            return StatusCode(500, new { message = "SMS 발송에 실패했습니다. 잠시 후 다시 시도해주세요." });

        return Ok(new { message = "인증번호가 발송되었습니다." });
    }

    // 비밀번호 찾기 - 인증번호 확인 및 비밀번호 재설정
    [HttpPost("reset-password/verify")]
    public async Task<IActionResult> VerifyAndResetPassword([FromBody] ResetPasswordVerifyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LoginId))
            return BadRequest(new { message = "아이디를 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "이름을 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return BadRequest(new { message = "전화번호를 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "인증번호를 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "새 비밀번호를 입력해주세요." });

        if (dto.NewPassword.Length < 6)
            return BadRequest(new { message = "비밀번호는 최소 6자 이상이어야 합니다." });

        // 인증번호 확인
        var key = $"reset-password:{dto.PhoneNumber}:{dto.LoginId}";
        if (!_verificationService.VerifyCode(key, dto.Code))
            return BadRequest(new { message = "인증번호가 올바르지 않거나 만료되었습니다." });

        // 사용자 조회
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.LoginId == dto.LoginId && u.Name == dto.Name && u.Phone == dto.PhoneNumber);

        if (user == null)
            return NotFound(new { message = "입력하신 정보와 일치하는 계정을 찾을 수 없습니다." });

        // 비밀번호 재설정
        user.PasswordHash = _authService.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // 인증번호 삭제
        _verificationService.RemoveCode(key);

        _logger.LogInformation("비밀번호 재설정 완료: UserId={UserId}, LoginId={LoginId}", user.Id, user.LoginId);

        return Ok(new { message = "비밀번호가 성공적으로 재설정되었습니다." });
    }

    private string MaskLoginId(string loginId)
    {
        if (loginId.Length <= 8)
            return new string('*', loginId.Length);

        // 앞 4자리 + 중간 마스킹 + 뒤 4자리
        var visible = 4;
        var masked = loginId.Length - (visible * 2);
        return $"{loginId[..visible]}{new string('*', masked)}{loginId[^visible..]}";
    }
}

// DTOs
public class FindIdSendCodeDto
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class FindIdVerifyDto
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ResetPasswordSendCodeDto
{
    public string LoginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ResetPasswordVerifyDto
{
    public string LoginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
