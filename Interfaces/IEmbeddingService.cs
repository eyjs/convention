namespace LocalRAG.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    int EmbeddingDimensions { get; }
}
