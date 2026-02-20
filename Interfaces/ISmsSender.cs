namespace LocalRAG.Interfaces;

/// <summary>
/// SMS 발송을 담당하는 Core 인터페이스 (Infrastructure Layer)
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// SMS를 발송하고 결과 메시지 ID(cmp_msg)를 반환합니다.
    /// </summary>
    /// <param name="mobile">수신자 전화번호</param>
    /// <param name="message">메시지 내용</param>
    /// <param name="receiverName">수신자 이름 (선택사항)</param>
    /// <returns>발송 성공 시 cmp_msg ID, 실패 시 null/empty</returns>
    Task<string?> SendSmsAsync(string mobile, string message, string? receiverName = null);
}
