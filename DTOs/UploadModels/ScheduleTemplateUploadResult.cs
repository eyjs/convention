namespace LocalRAG.DTOs.UploadModels;

/// <summary>
/// 일정 템플릿 업로드 결과
/// </summary>
public class ScheduleTemplateUploadResult
{
    public bool Success { get; set; }
    public int TemplatesCreated { get; set; }
    public int ItemsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<ConventionActionInfo> CreatedActions { get; set; } = new();
}

public class ConventionActionInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? ScheduleDateTime { get; set; }
}
