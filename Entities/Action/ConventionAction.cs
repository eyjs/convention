namespace LocalRAG.Entities.Action;

/// <summary>
/// 액션의 실행 방식을 정의하는 Enum
/// </summary>
    public enum BehaviorType
    {
        StatusOnly = 0,
        ModuleLink = 1,
        FormBuilder = 2,
        Link = 3,
        ShowComponentPopup = 4 // 새로운 BehaviorType 추가
    }

public class ConventionAction
{
    public int Id { get; set; }
    public int ConventionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public string MapsTo { get; set; } = string.Empty;
    [Obsolete("Use FormBuilder system instead")]
    public string? ConfigJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int OrderNum { get; set; }
    public int? TemplateId { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? IconClass { get; set; }
    public string? Category { get; set; }
    public string? ActionCategory { get; set; }
    public string? TargetLocation { get; set; }
    public BehaviorType BehaviorType { get; set; } = BehaviorType.StatusOnly;

    /// <summary>
    /// BehaviorType=FormBuilder일 경우, 대상 FormDefinition의 ID
    /// </summary>
    public int? TargetId { get; set; }

    /// <summary>
    /// BehaviorType=ModuleLink일 경우, 대상 모듈의 ID (예: Survey.Id)
    /// </summary>
    public int? TargetModuleId { get; set; }

    public string? ModuleIdentifier { get; set; }
    public string? FrontendRoute { get; set; }

    // Navigation Property
    public Convention? Convention { get; set; }
    public ActionTemplate? Template { get; set; }
    public ICollection<UserActionStatus> UserActionStatuses { get; set; } = new List<UserActionStatus>();
}
