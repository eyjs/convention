namespace LocalRAG.DTOs.UploadModels;

/// <summary>
/// 사용자 업로드 결과
/// </summary>
public class UserUploadResult
{
    public bool Success { get; set; }
    public int UsersCreated { get; set; }
    public int UsersUpdated { get; set; }
    public int TotalProcessed { get; set; }
    public int RemovedUserConventions { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    // 시트2: 그룹-일정 매핑 결과
    public int ScheduleAssignmentsCreated { get; set; }
    public int ScheduleDuplicatesSkipped { get; set; }
    public List<string> ScheduleWarnings { get; set; } = new();

    // 시트3: 속성 결과
    public int AttributeUsersProcessed { get; set; }
    public int AttributesCreated { get; set; }
    public int AttributesUpdated { get; set; }
    public List<string> AttributeWarnings { get; set; } = new();
}
