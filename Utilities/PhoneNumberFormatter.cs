using System.Text.RegularExpressions;

namespace LocalRAG.Utilities;

/// <summary>
/// 전화번호 및 주민등록번호 포맷팅 유틸리티
///
/// 사용 목적:
/// 1. 입력: 사용자가 어떤 형식으로든 입력 가능 (하이픈 있음/없음)
/// 2. 검증: 하이픈 제거 후 자릿수와 형태 확인
/// 3. 저장: DB에 일관된 형식으로 저장 (하이픈 포함)
/// </summary>
public static class PhoneNumberFormatter
{
    /// <summary>
    /// 전화번호를 정규화합니다 (숫자만 추출)
    /// 예: "010-1234-5678" → "01012345678"
    /// </summary>
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return Regex.Replace(input, @"[^\d]", "");
    }

    /// <summary>
    /// 한국 휴대전화번호를 검증합니다
    /// 유효 형식: 010/011/016/017/018/019 + 3~4자리 + 4자리 (총 10~11자리)
    /// </summary>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var normalized = Normalize(phoneNumber);

        // 10자리 또는 11자리
        if (normalized.Length < 10 || normalized.Length > 11)
            return false;

        // 010, 011, 016, 017, 018, 019로 시작
        if (normalized.Length == 11)
        {
            return normalized.StartsWith("010") ||
                   normalized.StartsWith("011") ||
                   normalized.StartsWith("016") ||
                   normalized.StartsWith("017") ||
                   normalized.StartsWith("018") ||
                   normalized.StartsWith("019");
        }

        // 지역번호 (02, 031, 032 등)도 허용
        return true;
    }

    /// <summary>
    /// 전화번호를 표준 형식으로 포맷합니다
    /// 예: "01012345678" → "010-1234-5678"
    ///     "0212345678" → "02-1234-5678"
    /// </summary>
    public static string FormatPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        var normalized = Normalize(phoneNumber);

        if (string.IsNullOrEmpty(normalized))
            return string.Empty;

        // 11자리 휴대전화 (010-XXXX-XXXX)
        if (normalized.Length == 11 && normalized.StartsWith("01"))
        {
            return $"{normalized.Substring(0, 3)}-{normalized.Substring(3, 4)}-{normalized.Substring(7, 4)}";
        }

        // 10자리 휴대전화 (011-XXX-XXXX)
        if (normalized.Length == 10 && normalized.StartsWith("01"))
        {
            return $"{normalized.Substring(0, 3)}-{normalized.Substring(3, 3)}-{normalized.Substring(6, 4)}";
        }

        // 10자리 지역번호 (02-XXXX-XXXX)
        if (normalized.Length == 10 && normalized.StartsWith("02"))
        {
            return $"{normalized.Substring(0, 2)}-{normalized.Substring(2, 4)}-{normalized.Substring(6, 4)}";
        }

        // 9자리 지역번호 (02-XXX-XXXX)
        if (normalized.Length == 9 && normalized.StartsWith("02"))
        {
            return $"{normalized.Substring(0, 2)}-{normalized.Substring(2, 3)}-{normalized.Substring(5, 4)}";
        }

        // 11자리 지역번호 (031-XXXX-XXXX)
        if (normalized.Length == 11 && !normalized.StartsWith("01"))
        {
            return $"{normalized.Substring(0, 3)}-{normalized.Substring(3, 4)}-{normalized.Substring(7, 4)}";
        }

        // 10자리 지역번호 (031-XXX-XXXX)
        if (normalized.Length == 10 && !normalized.StartsWith("01"))
        {
            return $"{normalized.Substring(0, 3)}-{normalized.Substring(3, 3)}-{normalized.Substring(6, 4)}";
        }

        // 포맷할 수 없는 경우 정규화된 값 반환
        return normalized;
    }

    /// <summary>
    /// 주민등록번호를 검증합니다
    /// 유효 형식: YYMMDD-XXXXXXX (총 13자리)
    /// </summary>
    public static bool IsValidResidentNumber(string? residentNumber)
    {
        if (string.IsNullOrWhiteSpace(residentNumber))
            return false;

        var normalized = Normalize(residentNumber);

        // 정확히 13자리
        if (normalized.Length != 13)
            return false;

        // 생년월일 부분 검증 (YYMMDD)
        if (!int.TryParse(normalized.Substring(0, 2), out int year) ||
            !int.TryParse(normalized.Substring(2, 2), out int month) ||
            !int.TryParse(normalized.Substring(4, 2), out int day))
        {
            return false;
        }

        // 월 범위 검증
        if (month < 1 || month > 12)
            return false;

        // 일 범위 검증 (간단한 검증)
        if (day < 1 || day > 31)
            return false;

        // 성별 코드 검증 (7번째 자리: 1,2,3,4,5,6,7,8)
        var genderCode = normalized[6];
        if (genderCode < '1' || genderCode > '8')
            return false;

        return true;
    }

    /// <summary>
    /// 주민등록번호를 표준 형식으로 포맷합니다
    /// 예: "1234561234567" → "123456-1234567"
    /// </summary>
    public static string FormatResidentNumber(string? residentNumber)
    {
        if (string.IsNullOrWhiteSpace(residentNumber))
            return string.Empty;

        var normalized = Normalize(residentNumber);

        if (normalized.Length != 13)
            return normalized; // 형식이 맞지 않으면 정규화된 값 반환

        return $"{normalized.Substring(0, 6)}-{normalized.Substring(6, 7)}";
    }
}
