namespace LocalRAG.DTOs.ActionModels;

/// <summary>
/// 오케스트레이터가 반환하는 통합 액션 DTO
/// BehaviorType에 관계없이 일관된 구조로 상태를 제공
/// </summary>
public class OrchestratedActionDto
{
    /// <summary>
    /// ConventionAction ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 액션 제목
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 액션 설명
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 마감일
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// 프론트엔드 라우팅 경로 (계산됨)
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// 필수 여부
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 카테고리 (그룹화용)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 아이콘 클래스
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// 액션 카테고리 (UI 렌더링 타입)
    /// </summary>
    public string? ActionCategory { get; set; }

    /// <summary>
    /// 타겟 위치 (UI 렌더링 위치)
    /// </summary>
    public string? TargetLocation { get; set; }

    /// <summary>
    /// 액션 실행 방식
    /// </summary>
    public string BehaviorType { get; set; } = string.Empty;

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 완료 상태 ("NotStarted", "InProgress", "Completed")
    /// </summary>
    public string Status { get; set; } = "NotStarted";

    /// <summary>
    /// 진행 요약 (예: "3/5 항목 응답 완료", "제출 완료")
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 완료일
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
