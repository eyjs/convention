using LocalRAG.Models;
using LocalRAG.Repositories;
using LocalRAG.Interfaces;
using System.Text.Json;

namespace LocalRAG.Services.Chat;

/// <summary>
/// Convention Chat Orchestrator - 플로우만 관리
/// </summary>
public class ConventionChatService : IConventionChatService
{
    private readonly ILogger<ConventionChatService> _logger;
    private readonly ChatIntentRouter _intentRouter;
    private readonly GuestContextualDataProvider _guestContextService;
    private readonly RagSearchService _ragSearchService;
    private readonly LlmResponseService _llmResponseService;
    private readonly ConventionAccessService _accessService;
    private readonly IUnitOfWork _unitOfWork;

    public ConventionChatService(
        ILogger<ConventionChatService> logger,
        ChatIntentRouter intentRouter,
        GuestContextualDataProvider guestContextService,
        RagSearchService ragSearchService,
        LlmResponseService llmResponseService,
        ConventionAccessService accessService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _intentRouter = intentRouter;
        _guestContextService = guestContextService;
        _ragSearchService = ragSearchService;
        _llmResponseService = llmResponseService;
        _accessService = accessService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 통합 질문 처리 (SRP 적용)
    /// </summary>
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
            // (핵심 개선) conventionId가 있는 경우, 권한 검증 로직을 AskAsync에 통합
            if (conventionId.HasValue)
            {
                // 사용자 컨텍스트가 없으면 권한을 확인할 수 없으므로 예외 처리
                if (userContext == null)
                {
                    throw new UnauthorizedAccessException("사용자 정보가 없어 행사에 접근할 수 없습니다.");
                }

                // 행사 존재 여부 확인
                var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId.Value);
                if (convention == null)
                {
                    throw new ArgumentException($"Convention {conventionId} not found");
                }

                // 접근 권한 검증
                if (!await _accessService.VerifyAsync(conventionId.Value, userContext))
                {
                    throw new UnauthorizedAccessException("해당 행사에 접근 권한이 없습니다.");
                }
            }

            // Step 1: 의도 분류
            var intent = await _intentRouter.GetIntentAsync(question, history);
            _logger.LogInformation("Classified intent: {Intent}", intent);

            // Step 2: 컨텍스트 구축
            string? context = await BuildContextAsync(intent, question, conventionId, userContext);

            // Step 3: System Instruction 결정
            string? systemInstruction = _intentRouter.GetSystemInstruction(intent);

            // Step 4: LLM 응답 생성
            var answer = await _llmResponseService.GenerateResponseAsync(
                question, context, history, systemInstruction);

            // Step 5: 응답 반환
            return new ChatResponse
            {
                Answer = answer,
                LlmProvider = _llmResponseService.ProviderName,
                Intent = intent.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            throw;
        }
    }

    /// <summary>
    /// 추천 질문 생성
    /// </summary>
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

    /// <summary>
    /// 의도에 따른 컨텍스트 구축
    /// </summary>
    private async Task<string?> BuildContextAsync(
        ChatIntentRouter.Intent intent,
        string question,
        int? conventionId,
        ChatUserContext? userContext)
    {
        switch (intent)
        {
            case ChatIntentRouter.Intent.PersonalInfo:
            case ChatIntentRouter.Intent.PersonalSchedule:
                // 개인 정보/일정 컨텍스트
                _logger.LogInformation("Building personal context for GuestId: {GuestId}", userContext?.GuestId);
                return await _guestContextService.BuildGuestContextAsync(userContext, intent);

            case ChatIntentRouter.Intent.Event:
            case ChatIntentRouter.Intent.Unknown:
                // RAG 검색 컨텍스트
                _logger.LogInformation("Building RAG context for question");
                return await _ragSearchService.BuildContextAsync(question, conventionId, userContext);

            case ChatIntentRouter.Intent.General:
            default:
                // 일반 질문은 컨텍스트 없음
                _logger.LogInformation("No context needed for general query");
                return null;
        }
    }

    #endregion
}
