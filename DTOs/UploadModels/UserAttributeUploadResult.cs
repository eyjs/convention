namespace LocalRAG.DTOs.UploadModels;

/// <summary>
/// 사용자 속성 업로드 결과
/// </summary>
public class UserAttributeUploadResult
{
    public bool Success { get; set; }
    public int AttributesCreated { get; set; }
    public int AttributesUpdated { get; set; }
    public int UsersProcessed { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 통계 정보 (속성키별 값의 분포)
    /// </summary>
    public Dictionary<string, Dictionary<string, int>> Statistics { get; set; } = new();
}
