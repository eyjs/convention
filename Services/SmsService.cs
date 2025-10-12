namespace LocalRAG.Services;

/// <summary>
/// SMS 전송 서비스 인터페이스
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// 인증번호 SMS 전송
    /// </summary>
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
}

/// <summary>
/// SMS 전송 서비스 (회사 MSSQL Procedure 호출 가정)
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        try
        {
            // TODO: 실제 회사 MSSQL Procedure API 호출
            // 예시:
            // var result = await _httpClient.PostAsync("https://company-sms-api.com/send", 
            //     new { phone = phoneNumber, message = $"[행사관리] 인증번호: {code}" });
            
            _logger.LogInformation("SMS 발송 (가정): {Phone} - 인증번호 {Code}", phoneNumber, code);
            
            // 현재는 성공으로 가정
            await Task.Delay(100); // 실제 API 호출 시뮬레이션
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS 발송 실패: {Phone}", phoneNumber);
            return false;
        }
    }
}
