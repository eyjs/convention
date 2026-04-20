namespace LocalRAG.DTOs.UploadModels;

/// <summary>
/// 일정 업로드 미리보기 결과 (저장 전 검증용)
/// 멀티시트: Sheets 배열로 시트별 미리보기 제공
/// </summary>
public class ScheduleTemplatePreviewResult
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int WarningRows { get; set; }
    public int SkippedRows { get; set; }
    public int ConflictRows { get; set; }
    /// <summary>
    /// 단일 시트 호환용 (Sheets[0].Items와 동일)
    /// </summary>
    public List<SchedulePreviewItem> Items { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 멀티시트: 시트별 미리보기 목록
    /// </summary>
    public List<SheetPreview> Sheets { get; set; } = new();
}

/// <summary>
/// 시트 단위 미리보기
/// </summary>
public class SheetPreview
{
    /// <summary>
    /// 시트명 (= ScheduleTemplate.CourseName)
    /// </summary>
    public string SheetName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int WarningRows { get; set; }
    public int SkippedRows { get; set; }
    public int ConflictRows { get; set; }
    public List<SchedulePreviewItem> Items { get; set; } = new();
}

public class SchedulePreviewItem
{
    /// <summary>
    /// Excel 원본 행 번호
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// valid | warning | skipped
    /// </summary>
    public string Status { get; set; } = "valid";

    /// <summary>
    /// 경고/에러 메시지
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 기존 일정과 날짜·시간 충돌 여부
    /// </summary>
    public bool HasConflict { get; set; }

    /// <summary>
    /// 충돌 상세 정보
    /// </summary>
    public string? ConflictDetail { get; set; }

    public string? Date { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Location { get; set; }
    /// <summary>
    /// 지도 링크 URL (E열)
    /// </summary>
    public string? MapUrl { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    /// <summary>
    /// 노출할 참석자 속성 키 (쉼표 구분, H열)
    /// 예: "룸번호,룸메이트"
    /// </summary>
    public string? VisibleAttributes { get; set; }
}

/// <summary>
/// 미리보기 확인 후 최종 저장 요청
/// </summary>
public class ScheduleTemplateConfirmRequest
{
    public string CourseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SchedulePreviewItem> Items { get; set; } = new();
}

/// <summary>
/// 멀티시트 일괄 저장 요청
/// </summary>
public class MultiSheetConfirmRequest
{
    public List<SheetConfirmItem> Sheets { get; set; } = new();
}

public class SheetConfirmItem
{
    public string CourseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SchedulePreviewItem> Items { get; set; } = new();
}
