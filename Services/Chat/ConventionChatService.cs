using LocalRAG.Models;
using LocalRAG.Repositories;
using LocalRAG.Interfaces;
using System.Text;

namespace LocalRAG.Services.Chat;

public class ConventionChatService : IConventionChatService
{
    private readonly SourceIdentifier _sourceIdentifier;
    private readonly GuestContextualDataProvider _guestContextService;
    private readonly RagSearchService _ragSearchService;
    private readonly LlmResponseService _llmResponseService;
    private readonly ConventionAccessService _accessService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConventionChatService> _logger;

    public ConventionChatService(
        SourceIdentifier sourceIdentifier,
        GuestContextualDataProvider guestContextService,
        RagSearchService ragSearchService,
        LlmResponseService llmResponseService,
        ConventionAccessService accessService,
        IUnitOfWork unitOfWork,
        ILogger<ConventionChatService> logger)
    {
        _sourceIdentifier = sourceIdentifier;
        _guestContextService = guestContextService;
        _ragSearchService = ragSearchService;
        _llmResponseService = llmResponseService;
        _accessService = accessService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ChatResponse> AskAsync(
        string question,
        int? conventionId = null,
        ChatUserContext? userContext = null,
        List<ChatRequestMessage>? history = null)
    {
        _logger.LogInformation("Processing question: '{Question}' for ConventionId: {ConventionId}, GuestId: {GuestId}",
            question, conventionId, userContext?.GuestId);

        try
        {
            // 권한 검증
            if (conventionId.HasValue)
            {
                if (userContext == null)
                {
                    throw new UnauthorizedAccessException("사용자 정보가 없어 행사에 접근할 수 없습니다.");
                }

                var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId.Value);
                if (convention == null)
                {
                    throw new ArgumentException($"Convention {conventionId} not found");
                }

                if (!await _accessService.VerifyAsync(conventionId.Value, userContext))
                {
                    throw new UnauthorizedAccessException("해당 행사에 접근 권한이 없습니다.");
                }
            }

            // Step 1: 필요한 데이터 소스 식별
            var requiredSources = await _sourceIdentifier.GetRequiredSourcesAsync(question, history);
            _logger.LogInformation("Required sources: [{Sources}]", string.Join(", ", requiredSources));

            // Step 2: 컨텍스트 수집
            string? aggregatedContext = null;

            if (requiredSources.Count == 1 && 
                requiredSources[0] == SourceIdentifier.SourceType.General_LLM)
            {
                _logger.LogInformation("General question detected, skipping context gathering");
            }
            else
            {
                aggregatedContext = await GatherContextFromMultipleSourcesAsync(
                    requiredSources, 
                    question, 
                    conventionId, 
                    userContext);
            }

            // Step 3: 시스템 지시문 생성
            var systemInstruction = BuildSystemInstruction(requiredSources);

            // Step 4: LLM 응답 생성
            var answer = await _llmResponseService.GenerateResponseAsync(
                question, 
                aggregatedContext, 
                history, 
                systemInstruction);

            return new ChatResponse
            {
                Answer = answer,
                LlmProvider = _llmResponseService.ProviderName,
                Intent = string.Join(", ", requiredSources)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            throw;
        }
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync(
        int conventionId,
        ChatUserContext? userContext)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null) return new List<string>();

        var suggestions = new List<string>
        {
            $"{convention.Title} 행사는 언제 진행되나요?",
            "오늘 일정은 무엇인가요?"
        };

        if (userContext?.Role == UserRole.Guest)
        {
            suggestions.Add("내 정보를 알려주세요.");
            suggestions.Add("내 전체 일정을 알려주세요.");
        }

        return suggestions;
    }

    #region Private Methods

    private async Task<string?> GatherContextFromMultipleSourcesAsync(
        List<SourceIdentifier.SourceType> sources,
        string question,
        int? conventionId,
        ChatUserContext? userContext)
    {
        var contextTasks = new List<Task<ContextResult>>();

        foreach (var source in sources)
        {
            if (source == SourceIdentifier.SourceType.General_LLM)
                continue;

            var task = GatherContextFromSingleSourceAsync(
                source, 
                question, 
                conventionId, 
                userContext);
            
            contextTasks.Add(task);
        }

        if (contextTasks.Count == 0)
            return null;

        var contextResults = await Task.WhenAll(contextTasks);

        var validContexts = contextResults
            .Where(r => r.Success && !string.IsNullOrWhiteSpace(r.Context))
            .ToList();

        if (validContexts.Count == 0)
        {
            _logger.LogWarning("No valid context gathered from any source");
            return null;
        }

        var aggregated = new StringBuilder();
        
        foreach (var contextResult in validContexts)
        {
            aggregated.AppendLine($"=== Context from {contextResult.SourceName} ===");
            aggregated.AppendLine(contextResult.Context);
            aggregated.AppendLine();
            aggregated.AppendLine("---");
            aggregated.AppendLine();
        }

        var finalContext = aggregated.ToString();
        
        _logger.LogInformation(
            "Aggregated context from {Count} sources (total length: {Length} chars)",
            validContexts.Count,
            finalContext.Length);

        return finalContext;
    }

    private async Task<ContextResult> GatherContextFromSingleSourceAsync(
        SourceIdentifier.SourceType source,
        string question,
        int? conventionId,
        ChatUserContext? userContext)
    {
        try
        {
            string? context = null;

            switch (source)
            {
                case SourceIdentifier.SourceType.Personal_DB:
                    _logger.LogDebug("Gathering context from Personal_DB");
                    context = await _guestContextService.BuildGuestContextAsync(
                        userContext, 
                        ChatIntentRouter.Intent.PersonalInfo);
                    break;

                case SourceIdentifier.SourceType.Schedule_DB:
                    _logger.LogDebug("Gathering context from Schedule_DB");
                    context = await _guestContextService.BuildGuestContextAsync(
                        userContext, 
                        ChatIntentRouter.Intent.PersonalSchedule);
                    break;

                case SourceIdentifier.SourceType.RAG_Convention:
                    _logger.LogDebug("Gathering context from RAG_Convention (ID: {ConventionId})", conventionId);
                    context = await _ragSearchService.BuildContextAsync(
                        question, 
                        conventionId, 
                        userContext);
                    break;

                case SourceIdentifier.SourceType.RAG_Global:
                    _logger.LogDebug("Gathering context from RAG_Global");
                    context = await _ragSearchService.BuildContextAsync(
                        question, 
                        conventionId: null,
                        userContext);
                    break;

                default:
                    _logger.LogWarning("Unexpected source type: {Source}", source);
                    break;
            }

            return new ContextResult
            {
                SourceName = source.ToString(),
                Context = context,
                Success = !string.IsNullOrWhiteSpace(context)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error gathering context from {Source}", source);
            return new ContextResult
            {
                SourceName = source.ToString(),
                Context = null,
                Success = false,
                Error = ex.Message
            };
        }
    }

    private string BuildSystemInstruction(List<SourceIdentifier.SourceType> sources)
    {
        bool hasRagSource = sources.Any(s => 
            s == SourceIdentifier.SourceType.RAG_Convention || 
            s == SourceIdentifier.SourceType.RAG_Global);

        bool hasPersonalData = sources.Any(s => 
            s == SourceIdentifier.SourceType.Personal_DB || 
            s == SourceIdentifier.SourceType.Schedule_DB);

        var instruction = new StringBuilder();
        
        instruction.AppendLine("You are a helpful and knowledgeable convention assistant.");
        instruction.AppendLine();

        if (hasRagSource && hasPersonalData)
        {
            instruction.AppendLine("You have access to both the convention information database and the user's personal information.");
            instruction.AppendLine("Use the provided context to give accurate, personalized answers.");
            instruction.AppendLine("When information is from the user's personal data, make it clear (e.g., 'According to your schedule...')");
            instruction.AppendLine("When information is from the convention database, cite it appropriately.");
        }
        else if (hasRagSource)
        {
            instruction.AppendLine("Use the provided context from the convention database to answer questions accurately.");
            instruction.AppendLine("If the context doesn't contain enough information, say so clearly.");
            instruction.AppendLine("Cite specific sessions, speakers, or venues when relevant.");
        }
        else if (hasPersonalData)
        {
            instruction.AppendLine("You have access to the user's personal information and schedule.");
            instruction.AppendLine("Provide personalized responses based on their data.");
            instruction.AppendLine("Be respectful of their privacy and only share what they ask for.");
        }
        else
        {
            instruction.AppendLine("Answer the user's question using your general knowledge.");
            instruction.AppendLine("Be friendly, concise, and helpful.");
        }

        instruction.AppendLine();
        instruction.AppendLine("Always respond in a natural, conversational tone.");
        instruction.AppendLine("If you're not sure about something, be honest about it.");

        return instruction.ToString();
    }

    private class ContextResult
    {
        public string SourceName { get; set; } = string.Empty;
        public string? Context { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    #endregion
}
