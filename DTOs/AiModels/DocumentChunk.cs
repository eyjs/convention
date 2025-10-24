namespace LocalRAG.DTOs.AiModels
{
    public class DocumentChunk
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public float[] Embedding { get; set; } // 임베딩 벡터를 저장할 필드 추가
    }
}