namespace LocalRAG.DTOs.UserModels;

public class UpdateProfileFieldRequest
{
    public string FieldName { get; set; } = string.Empty;
    public string? FieldValue { get; set; }
}
