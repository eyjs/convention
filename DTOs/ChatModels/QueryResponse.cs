using System.Collections.Generic;

namespace LocalRAG.DTOs.ChatModels
{
    public class QueryResponse
    {
        public string Answer { get; set; }
        public List<SourceDocument> Sources { get; set; }
        public string LlmProvider { get; set; }

        public QueryResponse(string answer, List<SourceDocument> sources, string llmProvider)
        {
            Answer = answer;
            Sources = sources;
            LlmProvider = llmProvider;
        }
    }

    public class SourceDocument
    {
        public string DocumentId { get; set; }
        public string Content { get; set; }
        public float Similarity { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public SourceDocument(string documentId, string content, float similarity, Dictionary<string, object> metadata)
        {
            DocumentId = documentId;
            Content = content;
            Similarity = similarity;
            Metadata = metadata;
        }
    }
}
