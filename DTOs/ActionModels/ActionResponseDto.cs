namespace LocalRAG.DTOs.ActionModels
{
    public class ActionResponseDto
    {
        public string ResponseDataJson { get; set; } = string.Empty;
    }

    public class ToggleActionDto
    {
        public bool IsComplete { get; set; }
    }
}
