namespace LocalRAG.DTOs.ActionModels;

/// <summary>
/// 체크리스트 전체 상태 DTO
/// </summary>
public class ChecklistStatusDto
{
    /// <summary>
    /// 전체 아이템 수
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// 완료된 아이템 수
    /// </summary>
    public int CompletedItems { get; set; }

    /// <summary>
    /// 진행률 (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// 가장 가까운 미완료 액션의 마감일
    /// </summary>
    public DateTime? OverallDeadline { get; set; }

    /// <summary>
    /// 체크리스트 아이템 목록
    /// </summary>
    public List<ChecklistItemDto> Items { get; set; } = new();
}

/// <summary>
/// 체크리스트 개별 아이템 DTO
/// </summary>
public class ChecklistItemDto
{
    /// <summary>
    /// 액션 ID
    /// </summary>
    public int ActionId { get; set; }

    /// <summary>
    /// 표시될 제목
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 완료 여부
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// 마감일
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// Vue 라우터 경로
    /// </summary>
    public string NavigateTo { get; set; } = string.Empty;

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int OrderNum { get; set; }
}
