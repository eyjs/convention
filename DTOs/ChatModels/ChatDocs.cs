namespace LocalRAG.DTOs.ChatModels;

// 👇 --- 여기가 수정된 부분입니다 --- 👇
public class IndexingResult
{
    /// <summary>
    /// 성공적으로 처리된 행사의 수
    /// </summary>
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 실제 VectorStore에 색인된 총 문서(행사+참석자+일정 등)의 수
    /// </summary>
    public int TotalDocumentsIndexed { get; set; }
    public int TotalCount => SuccessCount + FailureCount;
}