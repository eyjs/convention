namespace LocalRAG.DTOs.AdminModels;

public class SendSmsRequestDto
{
    /// <summary>
    /// 발송할 템플릿의 원본 내용 (변수 포함)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 수신자 User ID 목록
    /// </summary>
    public List<int> TargetUserIds { get; set; } = new();
}
