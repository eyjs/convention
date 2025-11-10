namespace LocalRAG.Entities.Action;

/// <summary>
/// 액션의 실행 방식을 정의하는 Enum
/// </summary>
public enum ActionBehaviorType
{
    /// <summary>
    /// (기본값) 단순 완료 처리 - 기존 방식
    /// </summary>
    StatusOnly = 0,

    /// <summary>
    /// 폼 빌더 시스템 연동 (GenericForm 대체)
    /// </summary>
    FormBuilder = 1,

    /// <summary>
    /// 전문 모듈 연동 (예: 설문조사, 좌석 배치)
    /// </summary>
    ModuleLink = 2,

    /// <summary>
    /// 외부/내부 링크로 이동
    /// </summary>
    Link = 3
}

/// <summary>
/// 행사별 참여자 액션 템플릿
/// 어드민이 동적으로 액션을 추가하고 관리할 수 있음
/// </summary>
public class ConventionAction
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 연관된 행사 ID
    /// </summary>
    public int ConventionId { get; set; }

    /// <summary>
    /// 체크리스트에 표시될 제목 (예: "여행 서류 제출")
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 액션의 상세 설명 (HTML 지원)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 액션의 마감일 (카운트다운 타이머에 사용)
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// Vue 라우터 경로 (예: "/feature/travel-info")
    /// [Deprecated] ModuleLink 타입에서는 FrontendRoute + TargetId 조합 사용 권장
    /// </summary>
    public string MapsTo { get; set; } = string.Empty;

    /// <summary>
    /// [Deprecated] 복잡한 액션을 위한 JSON 설정
    /// FormBuilder 시스템으로 대체됨. 기존 호환성을 위해 유지
    /// </summary>
    [Obsolete("Use FormBuilder system instead")]
    public string? ConfigJson { get; set; }

    /// <summary>
    /// 생성일
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 수정일
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 활성화 여부
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 템플릿 ID (공통 템플릿을 사용하는 경우)
    /// </summary>
    public int? TemplateId { get; set; }

    /// <summary>
    /// 필수 여부
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// 아이콘 클래스 (템플릿 아이콘을 덮어쓸 경우)
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// 카테고리 (체크리스트 그룹화용)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 액션 카테고리 (UI 렌더링 타입: BUTTON, MENU, AUTO_POPUP, BANNER, CARD)
    /// null이면 체크리스트 전용 액션
    /// </summary>
    public string? ActionCategory { get; set; }

    /// <summary>
    /// 타겟 위치 (UI 렌더링 위치: HOME_SUB_HEADER, SCHEDULE_CONTENT_TOP 등)
    /// ActionCategory가 있을 때만 의미 있음
    /// </summary>
    public string? TargetLocation { get; set; }

    /// <summary>
    /// 액션 실행 방식 (StatusOnly, FormBuilder, ModuleLink, Link)
    /// 기본값: StatusOnly (기존 방식 유지)
    /// </summary>
    public ActionBehaviorType BehaviorType { get; set; } = ActionBehaviorType.StatusOnly;

    /// <summary>
    /// [신규] FormBuilder 또는 ModuleLink의 대상 ID
    /// - BehaviorType=FormBuilder -> FormDefinition.Id
    /// - BehaviorType=ModuleLink -> Survey.Id (또는 SeatMap.Id 등)
    /// </summary>
    public int? TargetId { get; set; }

    /// <summary>
    /// [Deprecated] ModuleLink 타입일 경우, 대상 모듈의 PK ID
    /// TargetId로 대체됨. 기존 호환성을 위해 유지
    /// </summary>
    [Obsolete("Use TargetId instead")]
    public int? TargetModuleId
    {
        get => TargetId;
        set => TargetId = value;
    }

    /// <summary>
    /// [신규] ModuleLink를 위한 모듈 식별자 (예: "Survey", "SeatMap")
    /// 오케스트레이터가 이 값을 보고 API 경로를 결정함
    /// </summary>
    public string? ModuleIdentifier { get; set; }

    /// <summary>
    /// [신규] ModuleLink를 위한 프론트엔드 라우트 경로
    /// 예: "/feature/survey/" (TargetId와 조합하여 /feature/survey/15 로 이동)
    /// </summary>
    public string? FrontendRoute { get; set; }

    // Navigation Property
    public Convention? Convention { get; set; }
    public ActionTemplate? Template { get; set; }
    public ICollection<UserActionStatus> UserActionStatuses { get; set; } = new List<UserActionStatus>();
}
