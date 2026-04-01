using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;

namespace LocalRAG.Services.Shared;

/// <summary>
/// SMS 관련 비즈니스 로직을 처리하는 도메인 서비스
/// 인증번호 발송, 알림 발송 등의 구체적인 유스케이스를 구현합니다.
/// </summary>
public class SmsService : ISmsService
{
    private readonly ISmsSender _smsSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SmsService> _logger;

    public SmsService(ISmsSender smsSender, IUnitOfWork unitOfWork, ILogger<SmsService> logger)
    {
        _smsSender = smsSender;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 인증번호 발송 유스케이스
    /// </summary>
    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        string message = $"[StarTour] 인증번호는 [{code}] 입니다.";

        // Core Sender 호출
        var msgId = await _smsSender.SendSmsAsync(phoneNumber, message, "Guest");

        if (!string.IsNullOrEmpty(msgId))
        {
            _logger.LogInformation("Verification SMS sent. Code: {Code}, MsgId: {MsgId}", code, msgId);

            // 로그 저장 (인증번호는 특정 행사와 무관하므로 ConventionId = null)
            await SaveSmsLogAsync(null, "Guest", phoneNumber, message, msgId);
            return true;
        }

        _logger.LogError("Failed to send verification SMS to {PhoneNumber}", phoneNumber);
        return false;
    }

    /// <summary>
    /// 일반적인 SMS 발송 메서드 (행사 ID 포함)
    /// </summary>
    public async Task<bool> SendSmsAsync(int? conventionId, string receiverName, string phoneNumber, string message)
    {
        var msgId = await _smsSender.SendSmsAsync(phoneNumber, message, receiverName);

        if (!string.IsNullOrEmpty(msgId))
        {
            await SaveSmsLogAsync(conventionId, receiverName, phoneNumber, message, msgId);
            return true;
        }

        return false;
    }

    private async Task SaveSmsLogAsync(int? conventionId, string receiverName, string phoneNumber, string message, string externalId)
    {
        try
        {
            var log = new SmsLog
            {
                ConventionId = conventionId,
                ReceiverName = receiverName,
                ReceiverPhone = phoneNumber,
                Message = message,
                SnsType = message.Length > 90 ? "LMS" : "SMS",
                ExternalId = externalId,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.SmsLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save SMS log. ExternalId: {ExternalId}", externalId);
            // 로그 저장이 실패했다고 해서 비즈니스 로직을 실패 처리하지는 않음
        }
    }
}
