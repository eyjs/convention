namespace LocalRAG.Interfaces;

/// <summary>
/// 인증번호 관리 서비스
/// </summary>
public interface IVerificationService
{
    /// <summary>
    /// 인증번호 생성 및 저장
    /// </summary>
    string GenerateCode(string key);
    
    /// <summary>
    /// 인증번호 검증
    /// </summary>
    bool VerifyCode(string key, string code);
    
    /// <summary>
    /// 인증번호 삭제
    /// </summary>
    void RemoveCode(string key);
}
