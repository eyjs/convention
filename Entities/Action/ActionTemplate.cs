namespace LocalRAG.Entities.Action;

/// <summary>
/// 공통으로 사용 가능한 액션 템플릿
/// </summary>
public class ActionTemplate
{
    public int Id { get; set; }
    
    /// <summary>
    /// 템플릿 고유 식별자 (예: "SURVEY", "SCHEDULE_CHOICE", "DOCUMENT_UPLOAD")
    /// </summary>
    public string TemplateType { get; set; } = string.Empty;
    
    /// <summary>
    /// 템플릿 이름 (예: "설문조사", "일정 선택", "문서 업로드")
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;
    
    /// <summary>
    /// 템플릿 설명
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 카테고리 (예: "참여자 정보", "일정 관리", "피드백")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 아이콘 클래스 또는 URL
    /// </summary>
    public string? IconClass { get; set; }
    
    /// <summary>
    /// 기본 Vue 라우터 경로
    /// </summary>
    public string DefaultRoute { get; set; } = string.Empty;
    
    /// <summary>
    /// 기본 설정 JSON (스키마 포함)
    /// </summary>
    public string? DefaultConfigJson { get; set; }
    
    /// <summary>
    /// 필수 필드 목록 (JSON 배열)
    /// </summary>
    public string? RequiredFields { get; set; }
    
    /// <summary>
    /// 템플릿 활성화 여부
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int OrderNum { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}