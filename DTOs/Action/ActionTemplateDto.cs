namespace LocalRAG.DTOs.Action;

/// <summary>
/// 액션 템플릿 생성/수정 DTO
/// </summary>
public class ActionTemplateDto
{
    public string TemplateType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public string DefaultRoute { get; set; } = string.Empty;
    public string? DefaultConfigJson { get; set; }
    public string? RequiredFields { get; set; }
    public bool IsActive { get; set; } = true;
    public int OrderNum { get; set; }
}

/// <summary>
/// 템플릿을 Convention에 적용할 때 사용하는 DTO
/// </summary>
public class ApplyTemplateDto
{
    public string? ActionType { get; set; }  // null이면 템플릿 기본값 사용
    public string? Title { get; set; }       // null이면 템플릿 기본값 사용
    public DateTime? Deadline { get; set; }
    public string? MapsTo { get; set; }      // null이면 템플릿 기본값 사용
    public string? ConfigJson { get; set; }   // null이면 템플릿 기본값 사용
    public string? IconClass { get; set; }    // null이면 템플릿 기본값 사용
    public string? Category { get; set; }     // null이면 템플릿 기본값 사용
    public bool IsActive { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    public int OrderNum { get; set; }
}

/// <summary>
/// 액션 관리 화면용 통합 DTO
/// </summary>
public class ActionManagementDto
{
    public int ConventionId { get; set; }
    public string ConventionName { get; set; } = string.Empty;
    public List<ConventionActionDetailDto> Actions { get; set; } = new();
    public List<ActionTemplateSummaryDto> AvailableTemplates { get; set; } = new();
}

/// <summary>
/// Convention Action 상세 정보 (템플릿 정보 포함)
/// </summary>
public class ConventionActionDetailDto : ConventionActionDto
{
    public string? TemplateName { get; set; }
    public string? TemplateType { get; set; }
    public string? IconClass { get; set; }
    public string? Category { get; set; }
    public bool IsRequired { get; set; }
    public int CompletedCount { get; set; }
    public int TotalGuestCount { get; set; }
    public double CompletionRate => TotalGuestCount > 0 ? (double)CompletedCount / TotalGuestCount * 100 : 0;
}

/// <summary>
/// 템플릿 요약 정보 (목록용)
/// </summary>
public class ActionTemplateSummaryDto
{
    public int Id { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
}
