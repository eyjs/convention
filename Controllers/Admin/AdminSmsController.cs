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

    /// <summary>
    /// 엑셀 기반 단발성 문자 발송
    /// 클라이언트에서 수신자별 변수 치환 완료된 메시지를 받아 개별 발송
    /// </summary>
    [HttpPost("conventions/{conventionId}/sms/send-direct")]
    public async Task<IActionResult> SendSmsDirect(int conventionId, [FromBody] SendSmsDirectRequestDto dto)
    {
        if (dto.Recipients == null || dto.Recipients.Count == 0)
            return BadRequest(new { message = "수신자가 없습니다." });

        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null)
            return NotFound(new { message = "행사 정보를 찾을 수 없습니다." });

        var result = new SendSmsDirectResult
        {
            TotalCount = dto.Recipients.Count
        };

        foreach (var recipient in dto.Recipients)
        {
            if (string.IsNullOrWhiteSpace(recipient.Phone))
            {
                result.FailCount++;
                result.FailedItems.Add(new SmsDirectFailItem
                {
                    Name = recipient.Name,
                    Phone = recipient.Phone,
                    Reason = "전화번호 없음"
                });
                continue;
            }

            if (string.IsNullOrWhiteSpace(recipient.Message))
            {
                result.FailCount++;
                result.FailedItems.Add(new SmsDirectFailItem
                {
                    Name = recipient.Name,
                    Phone = recipient.Phone,
                    Reason = "메시지 없음"
                });
                continue;
            }

            try
            {
                var success = await _smsService.SendSmsAsync(
                    conventionId,
                    recipient.Name ?? "Guest",
                    recipient.Phone,
                    recipient.Message);

                if (success)
                {
                    result.SuccessCount++;
                }
                else
                {
                    result.FailCount++;
                    result.FailedItems.Add(new SmsDirectFailItem
                    {
                        Name = recipient.Name,
                        Phone = recipient.Phone,
                        Reason = "발송 실패"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS 발송 오류 — To: {Phone}", recipient.Phone);
                result.FailCount++;
                result.FailedItems.Add(new SmsDirectFailItem
                {
                    Name = recipient.Name,
                    Phone = recipient.Phone,
                    Reason = ex.Message
                });
            }
        }

        _logger.LogInformation(
            "SMS 일괄 발송 완료 — Convention: {ConvId}, Total: {Total}, Success: {Success}, Fail: {Fail}",
            conventionId, result.TotalCount, result.SuccessCount, result.FailCount);

        return Ok(result);
    }
}
