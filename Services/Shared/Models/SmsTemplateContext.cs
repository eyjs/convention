namespace LocalRAG.Services.Shared.Models;

/// <summary>
/// SMS 템플릿 변수 치환을 위한 컨텍스트 데이터
/// 필요한 모든 변수를 미리 가공하여 담고 있습니다.
/// </summary>
public class SmsTemplateContext
{
    // --- 참석자 정보 ---
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string CorpPart { get; set; } = string.Empty;
    
    // 동적 속성 (User Attributes) - 예: passport_no, t_shirt_size 등
    public Dictionary<string, string> GuestAttributes { get; set; } = new();

    // --- 행사 정보 ---
    public string ConventionTitle { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty; // 포맷팅된 문자열 (yyyy.MM.dd)
    public string EndDate { get; set; } = string.Empty;
    
    // TODO: Convention 엔티티에 PreEndDate가 추가되면 여기서도 매핑
    public string PreEndDate { get; set; } = string.Empty; 

    // --- 시스템 생성 정보 ---
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 좌석 배치도 딥링크 (내 자리 보기)
    /// 알림톡에서 #{seat_link} 변수로 사용
    /// </summary>
    public string SeatLink { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
}
