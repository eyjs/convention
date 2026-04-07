using LocalRAG.Constants;
using LocalRAG.Interfaces;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminSmsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly ILogger<AdminSmsController> _logger;

    public AdminSmsController(
        IUnitOfWork unitOfWork,
        ISmsService smsService,
        ILogger<AdminSmsController> logger)
    {
        _unitOfWork = unitOfWork;
        _smsService = smsService;
        _logger = logger;
    }

    // 상한/제한 상수
    private const int MaxRecipientsPerRequest = 5000;
    private const int MaxMessageLength = 2000; // 프로시저 @p_msg VARCHAR(2000)

    /// <summary>
    /// 엑셀 기반 단발성 문자 발송
    /// 클라이언트에서 수신자별 변수 치환 완료된 메시지를 받아 개별 발송
    /// </summary>
    [HttpPost("conventions/{conventionId}/sms/send-direct")]
    public async Task<IActionResult> SendSmsDirect(int conventionId, [FromBody] SendSmsDirectRequestDto dto)
    {
        if (dto.Recipients == null || dto.Recipients.Count == 0)
            return BadRequest(new { message = "수신자가 없습니다." });

        if (dto.Recipients.Count > MaxRecipientsPerRequest)
            return BadRequest(new { message = $"수신자는 한 번에 최대 {MaxRecipientsPerRequest}명까지 가능합니다." });

        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null)
            return NotFound(new { message = "행사 정보를 찾을 수 없습니다." });

        var result = new SendSmsDirectResult
        {
            TotalCount = dto.Recipients.Count
        };

        foreach (var recipient in dto.Recipients)
        {
            // 전화번호 정규화: 숫자만 남기기 (하이픈/공백/괄호 등 제거)
            var normalizedPhone = NormalizePhone(recipient.Phone);

            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                AddFail(result, recipient, "전화번호 없음");
                continue;
            }

            if (!IsValidKoreanMobile(normalizedPhone))
            {
                AddFail(result, recipient, $"잘못된 전화번호 형식: {recipient.Phone}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(recipient.Message))
            {
                AddFail(result, recipient, "메시지 없음");
                continue;
            }

            if (recipient.Message.Length > MaxMessageLength)
            {
                AddFail(result, recipient, $"메시지가 너무 깁니다 ({recipient.Message.Length}/{MaxMessageLength}자)");
                continue;
            }

            try
            {
                var success = await _smsService.SendSmsAsync(
                    conventionId,
                    recipient.Name ?? "Guest",
                    normalizedPhone,
                    recipient.Message);

                if (success)
                {
                    result.SuccessCount++;
                }
                else
                {
                    AddFail(result, recipient, "발송 실패");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS 발송 오류 — To: {Phone}", recipient.Phone);
                AddFail(result, recipient, ex.Message);
            }
        }

        _logger.LogInformation(
            "SMS 일괄 발송 완료 — Convention: {ConvId}, Total: {Total}, Success: {Success}, Fail: {Fail}",
            conventionId, result.TotalCount, result.SuccessCount, result.FailCount);

        return Ok(result);
    }

    private static void AddFail(SendSmsDirectResult result, SmsDirectRecipient recipient, string reason)
    {
        result.FailCount++;
        result.FailedItems.Add(new SmsDirectFailItem
        {
            Name = recipient.Name,
            Phone = recipient.Phone,
            Reason = reason
        });
    }

    /// <summary>
    /// 전화번호 정규화: 숫자만 남기기
    /// "010-1234-5678" → "01012345678"
    /// "010 1234 5678" → "01012345678"
    /// "+82 10 1234 5678" → "821012345678" (국제번호도 그대로 보존)
    /// </summary>
    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = System.Text.RegularExpressions.Regex.Replace(phone, @"\D", "");

        // +82로 시작하는 국제번호를 010 형식으로 변환 (82 + 10xxxxxxxx → 010xxxxxxxx)
        if (digits.StartsWith("82") && digits.Length >= 11)
        {
            digits = "0" + digits.Substring(2);
        }

        return digits;
    }

    /// <summary>
    /// 한국 휴대폰 번호 형식 검증 (010/011/016/017/018/019 + 7~8자리)
    /// </summary>
    private static bool IsValidKoreanMobile(string phone)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^01[016789]\d{7,8}$");
    }
}
