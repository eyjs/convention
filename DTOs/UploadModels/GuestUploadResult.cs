namespace LocalRAG.DTOs.UploadModels;

/// <summary>
/// 참석자 업로드 결과
/// </summary>
public class GuestUploadResult
{
    public bool Success { get; set; }
    public int GuestsCreated { get; set; }
    public int GuestsUpdated { get; set; }
    public int TotalProcessed { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
