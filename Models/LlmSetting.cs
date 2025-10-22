using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Models;

public class LlmSetting
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ProviderName { get; set; } = string.Empty; // "llama3", "gemini", "openai"

    [MaxLength(500)]
    public string? ApiKey { get; set; }

    [MaxLength(200)]
    public string? BaseUrl { get; set; }

    [MaxLength(100)]
    public string? ModelName { get; set; }

    public bool IsActive { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }

    // 추가 설정 (JSON으로 저장)
    public string? AdditionalSettings { get; set; }
}
