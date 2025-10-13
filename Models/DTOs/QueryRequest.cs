namespace LocalRAG.Models.DTOs
{
    public class QueryRequest
    {
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
    }
}
