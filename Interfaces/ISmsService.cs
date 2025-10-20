namespace LocalRAG.Interfaces;

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
