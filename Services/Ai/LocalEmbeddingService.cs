using LocalRAG.Interfaces;

namespace LocalRAG.Services.Ai;

public class LocalEmbeddingService : IEmbeddingService
{
    public int EmbeddingDimensions => 384;

    public Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // 임시 구현: 텍스트 해시 기반 간단한 벡터 생성
        // 실제로는 ONNX 모델이나 Transformer 모델을 사용해야 함
        var hash = text.GetHashCode();
        var random = new Random(hash);
        
        var embedding = new float[EmbeddingDimensions];
        for (int i = 0; i < EmbeddingDimensions; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2.0 - 1.0); // -1 ~ 1 범위
        }
        
        // 정규화
        var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] /= magnitude;
        }
        
        return Task.FromResult(embedding);
    }
}
