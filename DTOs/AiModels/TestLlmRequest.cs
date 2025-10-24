namespace LocalRAG.DTOs.AiModels
{
    public class TestLlmRequest
    {
        public string? Provider { get; set; }
        public string Prompt { get; set; } = string.Empty;
    }
}
