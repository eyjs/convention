namespace LocalRAG.DTOs.UserModels;

/// <summary>
/// 사용자 다중 속성 일괄 할당 DTO
/// </summary>
public class BulkAssignAttributesDto
{
    /// <summary>
    /// 행사 ID
    /// </summary>
    public int ConventionId { get; set; }
    
    /// <summary>
    /// 사용자별 속성 매핑 목록
    /// </summary>
    public List<UserAttributeMapping> UserMappings { get; set; } = new();
}

/// <summary>
/// 개별 사용자 속성 매핑
/// </summary>
public class UserAttributeMapping
{
    /// <summary>
    /// 사용자 ID
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// 속성 키-값 쌍 (예: {"버스": "1호차", "티셔츠": "L"})
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = new();
}

/// <summary>
/// 사용자 목록 조회 응답
/// </summary>
public class UserWithAttributesDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CorpName { get; set; }
    public string? CorpPart { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    
    /// <summary>
    /// 현재 할당된 속성들
    /// </summary>
    public Dictionary<string, string> CurrentAttributes { get; set; } = new();
}

/// <summary>
/// 일괄 할당 결과
/// </summary>
public class BulkAssignResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
