namespace LocalRAG.Entities.Action;

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
    /// 프로그램적 키 (예: "PROFILE_OVERSEAS", "SCHEDULE_CHOICE", "SURVEY_POST_EVENT")
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

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
    /// </summary>
    public string MapsTo { get; set; } = string.Empty;

    /// <summary>
    /// 복잡한 액션을 위한 JSON 설정 (예: 투어 옵션, 설문 문항)
    /// </summary>
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

    // Navigation Property
    public Convention? Convention { get; set; }
    public ActionTemplate? Template { get; set; }
    public ICollection<UserActionStatus> UserActionStatuses { get; set; } = new List<UserActionStatus>();
}
