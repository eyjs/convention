using LocalRAG.Interfaces;
using LocalRAG.Repositories;

namespace LocalRAG.Services;

/// <summary>
/// Convention 챗봇 서비스
/// 
/// 사용자의 질문에 대해 RAG를 사용하여 답변을 생성합니다.
/// </summary>
public class ConventionChatService
{
    private readonly IRagService _ragService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConventionChatService> _logger;

    public ConventionChatService(
        IRagService ragService,
        IUnitOfWork unitOfWork,
        ILogger<ConventionChatService> logger)
    {
        _ragService = ragService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 사용자 질문에 답변합니다.
    /// </summary>
    public async Task<ChatResponse> AskAsync(string question, int? conventionId = null)
    {
        _logger.LogInformation("Processing question: {Question}", question);

        try
        {
            // RAG로 답변 생성
            var ragResponse = await _ragService.QueryAsync(question, topK: 5);

            // Convention ID가 지정된 경우 필터링
            var sources = ragResponse.Sources;
            if (conventionId.HasValue)
            {
                sources = sources
                    .Where(s => s.Metadata != null && 
                           s.Metadata.ContainsKey("convention_id") && 
                           (int)s.Metadata["convention_id"] == conventionId.Value)
                    .ToList();
            }

            // 응답 구성
            var response = new ChatResponse
            {
                Answer = ragResponse.Answer,
                Sources = sources.Select(s => new SourceInfo
                {
                    Content = s.Content,
                    Similarity = s.Similarity,
                    Type = s.Metadata?.GetValueOrDefault("type")?.ToString() ?? "unknown",
                    ConventionId = s.Metadata?.GetValueOrDefault("convention_id") as int?,
                    ConventionTitle = s.Metadata?.GetValueOrDefault("title")?.ToString()
                }).ToList(),
                LlmProvider = ragResponse.LlmProvider
            };

            _logger.LogInformation(
                "Answer generated. Sources: {SourceCount}, Provider: {Provider}",
                response.Sources.Count, response.LlmProvider);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question: {Question}", question);
            throw;
        }
    }

    /// <summary>
    /// 특정 Convention에 대해 질문합니다.
    /// </summary>
    public async Task<ChatResponse> AskAboutConventionAsync(
        int conventionId, 
        string question)
    {
        // Convention 존재 확인
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null)
        {
            throw new ArgumentException($"Convention {conventionId} not found");
        }

        return await AskAsync(question, conventionId);
    }

    /// <summary>
    /// 추천 질문을 생성합니다.
    /// </summary>
    public async Task<List<string>> GetSuggestedQuestionsAsync(int conventionId)
    {
        var convention = await _unitOfWork.Conventions
            .GetConventionWithDetailsAsync(conventionId);

        if (convention == null)
        {
            return new List<string>();
        }

        var suggestions = new List<string>
        {
            $"{convention.Title} 행사는 언제 진행되나요?",
            "이번 행사의 담당자는 누구인가요?",
            "행사 일정을 알려주세요"
        };

        if (convention.Guests?.Any() == true)
        {
            suggestions.Add("참석자 명단을 알려주세요");
            suggestions.Add($"참석자는 총 몇 명인가요?");
        }

        if (convention.Schedules?.Any() == true)
        {
            suggestions.Add("오늘 일정은 무엇인가요?");
            var firstScheduleDate = convention.Schedules
                .Min(s => s.ScheduleDate);
            suggestions.Add($"{firstScheduleDate:MM월 dd일} 일정을 알려주세요");
        }

        return suggestions;
    }

    /// <summary>
    /// 대화 히스토리를 고려한 답변 생성 (추후 구현)
    /// </summary>
    public async Task<ChatResponse> AskWithHistoryAsync(
        string question,
        List<ChatMessage> history,
        int? conventionId = null)
    {
        // TODO: 대화 컨텍스트를 활용한 답변 생성
        // 현재는 기본 AskAsync 사용
        return await AskAsync(question, conventionId);
    }
}

/// <summary>
/// 챗봇 응답
/// </summary>
public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceInfo> Sources { get; set; } = new();
    public string LlmProvider { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// 출처 정보
/// </summary>
public class SourceInfo
{
    public string Content { get; set; } = string.Empty;
    public float Similarity { get; set; }
    public string Type { get; set; } = string.Empty; // convention, guest, schedule, menu
    public int? ConventionId { get; set; }
    public string? ConventionTitle { get; set; }
}

/// <summary>
/// 대화 메시지
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = string.Empty; // user, assistant
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
