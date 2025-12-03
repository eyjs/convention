namespace LocalRAG.DTOs.UploadModels;

/// <summary>
/// 옵션투어 업로드 요청
/// </summary>
public class OptionTourUploadRequest
{
    public List<OptionTourDto> Options { get; set; } = new();
    public List<ParticipantOptionMappingDto> ParticipantMappings { get; set; } = new();
}

/// <summary>
/// 옵션투어 DTO (엑셀 시트1)
/// </summary>
public class OptionTourDto
{
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int OptionId { get; set; } // 사용자 지정 ID
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 참석자별 옵션 매핑 DTO (엑셀 시트2)
/// </summary>
public class ParticipantOptionMappingDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty; // 주민번호
    public string Division { get; set; } = string.Empty; // 사업단/소속
    public string Group { get; set; } = string.Empty;
    public List<int> OptionIds { get; set; } = new(); // 콤마로 구분된 옵션ID 리스트
}
