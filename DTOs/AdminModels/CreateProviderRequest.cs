namespace LocalRAG.DTOs.AdminModels;

public class CreateProviderRequest
{
    public string ProviderType { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; }
    public string? AdditionalSettings { get; set; }
}
