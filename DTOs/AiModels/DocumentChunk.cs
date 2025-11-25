namespace LocalRAG.DTOs.AiModels
{
    public class DocumentChunk
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}