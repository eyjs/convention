namespace LocalRAG.DTOs.UploadModels;

/// <summary>
/// 옵션투어 업로드 결과
/// </summary>
public class OptionTourUploadResult
{
    public bool Success { get; set; }
    public int OptionsCreated { get; set; }
    public int MappingsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
