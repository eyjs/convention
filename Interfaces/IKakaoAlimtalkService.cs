using Popbill.Kakao;

namespace LocalRAG.Interfaces;

/// <summary>
/// 카카오 알림톡 발송 서비스 (팝빌 Popbill 기반)
/// </summary>
public interface IKakaoAlimtalkService
{
    /// <summary>
    /// 알림톡 단건 발송
    /// </summary>
    Task<(bool Success, string? ReceiptNum, string? ErrorMessage)> SendAsync(
        string templateCode,
        string senderNum,
        string receiverNum,
        string receiverName,
        string content,
        string? altContent = null);

    /// <summary>
    /// 알림톡 대량 발송
    /// </summary>
    Task<(int SuccessCount, int FailCount, string? ReceiptNum)> SendBulkAsync(
        string templateCode,
        string senderNum,
        List<AlimtalkReceiver> receivers);

    /// <summary>
    /// 팝빌 알림톡 템플릿 목록 조회
    /// </summary>
    List<ATSTemplate> ListTemplates();

    /// <summary>
    /// 팝빌 잔여 포인트 조회
    /// </summary>
    double GetBalance();
}

public class AlimtalkReceiver
{
    public string ReceiverNum { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? AltContent { get; set; }
}
