namespace LocalRAG.DTOs.AdminModels;

public class SendAlimtalkRequestDto
{
    /// <summary>
    /// 팝빌 알림톡 템플릿 코드
    /// </summary>
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>
    /// 발송할 내용 (템플릿 변수 포함 가능)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 대체문자 내용 (알림톡 실패 시)
    /// </summary>
    public string? AltContent { get; set; }

    /// <summary>
    /// 수신자 User ID 목록
    /// </summary>
    public List<int> TargetUserIds { get; set; } = new();
}
