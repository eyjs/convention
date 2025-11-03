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
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
