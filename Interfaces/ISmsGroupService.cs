using LocalRAG.DTOs.AdminModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// SMS 그룹 발송 서비스 인터페이스
/// 그룹 조회, 엑셀 템플릿 생성/파싱, 변수 치환 발송
/// </summary>
public interface ISmsGroupService
{
    /// <summary>
    /// 행사별 그룹 목록 + 수신자 수 조회
    /// </summary>
    Task<List<SmsGroupInfoDto>> GetGroupsAsync(int conventionId);

    /// <summary>
    /// 선택된 그룹의 수신자 정보가 채워진 엑셀 템플릿 생성
    /// </summary>
    Task<byte[]> GenerateExcelTemplateAsync(int conventionId, string? groupName);

    /// <summary>
    /// 업로드된 엑셀 파싱
    /// </summary>
    Task<SmsExcelParseResultDto> ParseExcelAsync(int conventionId, Stream excelStream);

    /// <summary>
    /// 엑셀 변수 치환 후 순차 발송 (send-one 방식 재활용)
    /// </summary>
    Task<SendSmsDirectResult> SendWithExcelVariablesAsync(int conventionId, SmsExcelSendRequestDto request);
}
