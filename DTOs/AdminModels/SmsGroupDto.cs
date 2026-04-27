namespace LocalRAG.DTOs.AdminModels;

/// <summary>
/// 그룹 목록 조회 응답 항목
/// </summary>
public class SmsGroupInfoDto
{
    public string GroupName { get; set; } = string.Empty;
    public int Count { get; set; }
    public int NoPhoneCount { get; set; }
}

/// <summary>
/// 엑셀 파싱 결과 응답
/// </summary>
public class SmsExcelParseResultDto
{
    public List<string> Columns { get; set; } = new();
    public List<SmsExcelRecipientDto> Recipients { get; set; } = new();
    public List<SmsExcelErrorRowDto> ErrorRows { get; set; } = new();
}

public class SmsExcelRecipientDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class SmsExcelErrorRowDto
{
    public int Row { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 엑셀 변수 치환 발송 요청
/// </summary>
public class SmsExcelSendRequestDto
{
    public string Template { get; set; } = string.Empty;
    public List<SmsExcelRecipientDto> Recipients { get; set; } = new();
}
