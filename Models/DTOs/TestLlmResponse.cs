namespace LocalRAG.Models.DTOs
{
    public class TestLlmResponse
    {
        public string Response { get; set; }
        public string Provider { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }

        public TestLlmResponse(string response, string provider, bool success, string? error = null)
        {
            Response = response;
            Provider = provider;
            Success = success;
            Error = error;
        }
    }
}
