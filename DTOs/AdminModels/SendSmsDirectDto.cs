namespace LocalRAG.DTOs.AdminModels;

/// <summary>
/// 엑셀 기반 단발성 문자 발송 요청 (클라이언트에서 변수 치환 완료된 상태)
/// </summary>
public class SendSmsDirectRequestDto
{
    public List<SmsDirectRecipient> Recipients { get; set; } = new();
}

public class SmsDirectRecipient
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SendSmsDirectResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<SmsDirectFailItem> FailedItems { get; set; } = new();
}

public class SmsDirectFailItem
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
