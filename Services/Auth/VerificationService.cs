using System.Collections.Concurrent;
using LocalRAG.Interfaces;

namespace LocalRAG.Services.Auth;

/// <summary>
/// 메모리 기반 인증번호 관리 (5분 유효)
/// </summary>
public class VerificationService : IVerificationService
{
    private readonly ConcurrentDictionary<string, (string Code, DateTime ExpireAt)> _codes = new();
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(ILogger<VerificationService> logger)
    {
        _logger = logger;
    }

    public string GenerateCode(string key)
    {
        // 6자리 랜덤 숫자
        var code = Random.Shared.Next(100000, 999999).ToString();
        var expireAt = DateTime.UtcNow.AddMinutes(5);
        
        _codes[key] = (code, expireAt);
        
        _logger.LogInformation("인증번호 생성: {Key} - {Code} (만료: {ExpireAt})", key, code, expireAt);
        
        return code;
    }

    public bool VerifyCode(string key, string code)
    {
        if (!_codes.TryGetValue(key, out var stored))
        {
            _logger.LogWarning("인증번호 없음: {Key}", key);
            return false;
        }

        if (DateTime.UtcNow > stored.ExpireAt)
        {
            _codes.TryRemove(key, out _);
            _logger.LogWarning("인증번호 만료: {Key}", key);
            return false;
        }

        if (stored.Code != code)
        {
            _logger.LogWarning("인증번호 불일치: {Key}", key);
            return false;
        }

        _logger.LogInformation("인증번호 확인 성공: {Key}", key);
        return true;
    }

    public void RemoveCode(string key)
    {
        _codes.TryRemove(key, out _);
        _logger.LogInformation("인증번호 삭제: {Key}", key);
    }
}
