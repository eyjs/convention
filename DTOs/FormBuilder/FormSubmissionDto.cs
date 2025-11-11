using System.Text.Json;

namespace LocalRAG.DTOs.FormBuilder;

public class FormSubmissionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public JsonElement SubmissionData { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
