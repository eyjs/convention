using LocalRAG.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Text.RegularExpressions;

namespace LocalRAG.Services.Ai;

public class OnnxEmbeddingService : IEmbeddingService
{
    private const int CLS_TOKEN_ID = 101;
    private const int SEP_TOKEN_ID = 102;
    private const int PAD_TOKEN_ID = 0;
    private const int MAX_SEQUENCE_LENGTH = 512;
    private const int MAX_WORDS = 510; // MAX_SEQUENCE_LENGTH - 2 (for CLS and SEP)
    private const int VOCAB_SIZE = 30000;
    private const int TOKEN_ID_OFFSET = 1000;

    private readonly InferenceSession _session;
    private readonly IConfiguration _configuration;
    private readonly string _modelPath;

    public int EmbeddingDimensions { get; }

    public OnnxEmbeddingService(IConfiguration configuration)
    {
        _configuration = configuration;
        _modelPath = _configuration["EmbeddingSettings:ModelPath"] ?? "models/all-MiniLM-L6-v2.onnx";
        EmbeddingDimensions = _configuration.GetValue<int>("EmbeddingSettings:Dimensions", 384);
        
        try
        {
            if (File.Exists(_modelPath))
            {
                _session = new InferenceSession(_modelPath);
            }
            else
            {
                // ONNX 모델이 없는 경우 기본 구현으로 폴백
                _session = null!;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize ONNX embedding model: {ex.Message}", ex);
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (_session == null)
        {
            // ONNX 모델이 없는 경우 기본 구현 사용
            return await GenerateSimpleEmbeddingAsync(text);
        }

        try
        {
            // 텍스트 전처리
            var processedText = PreprocessText(text);
            var tokens = TokenizeText(processedText);
            
            // ONNX 모델용 입력 텐서 생성
            var inputTensor = CreateInputTensor(tokens);
            
            // 추론 실행
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
            };

            using var results = await Task.Run(() => _session.Run(inputs));
            
            // 결과 텐서에서 임베딩 추출
            var outputTensor = results.FirstOrDefault()?.AsTensor<float>();
            if (outputTensor == null)
                throw new InvalidOperationException("Failed to get embedding from model output");

            // 평균 풀링 또는 CLS 토큰 사용
            var embedding = ExtractEmbedding(outputTensor);
            
            // 정규화
            return NormalizeEmbedding(embedding);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate embedding: {ex.Message}", ex);
        }
    }

    private async Task<float[]> GenerateSimpleEmbeddingAsync(string text)
    {
        // 기본 구현: 텍스트 해시 기반 벡터 생성
        var hash = text.GetHashCode();
        var random = new Random(hash);
        
        var embedding = new float[EmbeddingDimensions];
        for (int i = 0; i < EmbeddingDimensions; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2.0 - 1.0);
        }
        
        return await Task.FromResult(NormalizeEmbedding(embedding));
    }

    private static string PreprocessText(string text)
    {
        // 기본 전처리: 소문자화, 특수문자 제거
        text = text.ToLowerInvariant();
        text = Regex.Replace(text, @"[^\w\s가-힣]", " ");
        text = Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }

    private static int[] TokenizeText(string text)
    {
        // 간단한 토큰화 (실제로는 SentencePiece나 BERT 토크나이저 사용)
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tokens = new List<int> { CLS_TOKEN_ID }; // [CLS] token

        foreach (var word in words.Take(MAX_WORDS)) // 최대 길이 제한
        {
            // 단어를 해시 기반으로 토큰 ID 생성 (실제로는 vocab 사전 사용)
            var tokenId = Math.Abs(word.GetHashCode()) % VOCAB_SIZE + TOKEN_ID_OFFSET;
            tokens.Add(tokenId);
        }

        tokens.Add(SEP_TOKEN_ID); // [SEP] token

        // 패딩
        while (tokens.Count < MAX_SEQUENCE_LENGTH)
        {
            tokens.Add(PAD_TOKEN_ID); // [PAD] token
        }

        return tokens.ToArray();
    }

    private static Tensor<long> CreateInputTensor(int[] tokens)
    {
        var longTokens = tokens.Select(t => (long)t).ToArray();
        return new DenseTensor<long>(longTokens, new[] { 1, tokens.Length });
    }

    private float[] ExtractEmbedding(Tensor<float> outputTensor)
    {
        var embedding = new float[EmbeddingDimensions];
        
        // CLS 토큰의 임베딩 사용 (첫 번째 토큰)
        for (int i = 0; i < Math.Min(EmbeddingDimensions, outputTensor.Dimensions[2]); i++)
        {
            embedding[i] = outputTensor[0, 0, i]; // [batch, sequence, hidden]
        }
        
        return embedding;
    }

    private static float[] NormalizeEmbedding(float[] embedding)
    {
        var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= magnitude;
            }
        }
        
        return embedding;
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
