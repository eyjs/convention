namespace LocalRAG.Interfaces;

public interface ILlmProvider
{
    string ProviderName { get; }

    // [����] ����� ���ؽ�Ʈ(userContext)�� ���޹޴� �Ķ���͸� �߰��մϴ�.
    Task<string> GenerateResponseAsync(string prompt, string? context = null, string? userContext = null);

    Task<float[]> GenerateEmbeddingAsync(string text);
}