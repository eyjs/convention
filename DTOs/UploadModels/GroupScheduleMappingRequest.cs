using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.UploadModels;

/// <summary>
/// 그룹-일정 매핑 요청
/// </summary>
public class GroupScheduleMappingRequest
{
    [Required]
    public int ConventionId { get; set; }

    [Required]
    [MinLength(1)]
    public string UserGroup { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<int> ActionIds { get; set; } = new();
}

/// <summary>
/// 그룹-일정 매핑 결과
/// </summary>
public class GroupScheduleMappingResult
{
    public bool Success { get; set; }
    public int UsersAffected { get; set; }
    public int MappingsCreated { get; set; }
    public int DuplicatesSkipped { get; set; }
    public List<string> Errors { get; set; } = new();
}
