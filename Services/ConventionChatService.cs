using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Models;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace LocalRAG.Services;

public class ConventionChatService
{
    private readonly ILogger<ConventionChatService> _logger;
    private readonly ConventionDbContext _context;
    private readonly ILlmProvider _llmProvider;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly IUnitOfWork _unitOfWork;

    public ConventionChatService(
        ILogger<ConventionChatService> logger,
        ConventionDbContext context,
        ILlmProvider llmProvider,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _context = context;
        _llmProvider = llmProvider;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChatResponse> AskAsync(string question, int? conventionId = null, ChatUserContext? userContext = null, List<ChatMessage>? history = null)
    {
        _logger.LogInformation("Processing question: '{Question}' for GuestId: {GuestId}", question, userContext?.GuestId);

        var intent = await _llmProvider.ClassifyIntentAsync(question, history);

        if (intent.StartsWith("personal") && userContext?.GuestId.HasValue == true)
        {
            // 1. Try to get real-time data from DB first
            var guest = await _context.Guests
                .Include(g => g.GuestAttributes)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == userContext.GuestId.Value);

            if (guest != null)
            {
                _logger.LogInformation("Found guest {GuestId} in database, building real-time context.", guest.Id);
                var assignedItems = await _context.GuestScheduleTemplates
                    .Where(gst => gst.GuestId == guest.Id)
                    .Include(gst => gst.ScheduleTemplate.ScheduleItems)
                    .AsNoTracking()
                    .SelectMany(gst => gst.ScheduleTemplate.ScheduleItems)
                    .OrderBy(si => si.ScheduleDate).ThenBy(si => si.StartTime)
                    .ToListAsync();

                var contextSb = new StringBuilder();
                contextSb.AppendLine($"# {guest.GuestName}님의 개인 정보");
                contextSb.AppendLine($"- 이름: {guest.GuestName}");
                if (!string.IsNullOrEmpty(guest.CorpPart)) contextSb.AppendLine($"- 부서: {guest.CorpPart}");
                if (!string.IsNullOrEmpty(guest.Telephone)) contextSb.AppendLine($"- 연락처: {guest.Telephone}");
                if (!string.IsNullOrEmpty(guest.Email)) contextSb.AppendLine($"- 이메일: {guest.Email}");
                if (!string.IsNullOrEmpty(guest.Affiliation)) contextSb.AppendLine($"- 소속: {guest.Affiliation}");

                if (assignedItems.Any())
                {
                    contextSb.AppendLine("\n## 나의 전체 일정");
                    foreach (var item in assignedItems)
                    {
                        contextSb.AppendLine($"- {item.ScheduleDate:yyyy-MM-dd} {item.StartTime}: {item.Title}");
                    }
                }

                if (guest.GuestAttributes.Any())
                {
                    contextSb.AppendLine("\n## 추가 정보");
                    foreach (var attr in guest.GuestAttributes)
                    {
                        contextSb.AppendLine($"- {attr.AttributeKey}: {attr.AttributeValue}");
                    }
                }

                var personalContext = contextSb.ToString();
                var finalAnswer = await _llmProvider.GenerateResponseAsync(question, personalContext, history);
                return new ChatResponse { Answer = finalAnswer, LlmProvider = _llmProvider.ProviderName };
            }

            // 2. Fallback to searching the vector store
            _logger.LogInformation("Guest {GuestId} not found in DB, falling back to vector store search.", userContext.GuestId.Value);
            var guestFilter = new Dictionary<string, object>
            {
                { "guest_id", userContext.GuestId.Value },
                { "type", "guest" }
            };

            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);
            var searchResults = await _vectorStore.SearchAsync(queryEmbedding, topK: 1, filter: guestFilter);
            var guestDocument = searchResults.FirstOrDefault();

            if (guestDocument != null)
            {
                _logger.LogInformation("Found personal document for GuestId {GuestId} with similarity {Similarity}", userContext.GuestId.Value, guestDocument.Similarity);
                var context = guestDocument.Content;
                var finalAnswer = await _llmProvider.GenerateResponseAsync(question, context, history);
                return new ChatResponse { Answer = finalAnswer, LlmProvider = _llmProvider.ProviderName };
            }

            _logger.LogWarning("Personal query for GuestId {GuestId} did not find a matching guest in DB or vector store.", userContext.GuestId.Value);
        }

        // Fallback to general RAG pipeline
        var effectiveConventionId = conventionId;
        if (!effectiveConventionId.HasValue && userContext?.GuestId.HasValue == true)
        {
            var guest = await _context.Guests.FindAsync(userContext.GuestId.Value);
            if (guest != null)
            {
                effectiveConventionId = guest.ConventionId;
                _logger.LogInformation("Inferred ConventionId {ConventionId} from GuestId {GuestId}", effectiveConventionId, userContext.GuestId.Value);
            }
        }

        var generalQueryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);
        
        var filter = new Dictionary<string, object> { { "type", "convention" } };
        if (effectiveConventionId.HasValue)
        {
            filter.Add("convention_id", effectiveConventionId.Value);
        }

        var generalSearchResults = await _vectorStore.SearchAsync(generalQueryEmbedding, 5, filter);

        var contextForLlm = generalSearchResults.Any() ? string.Join("\n\n", generalSearchResults.Select(s => s.Content)) : null;
        var finalAnswerGeneral = await _llmProvider.GenerateResponseAsync(question, contextForLlm, history);

        return new ChatResponse
        {
            Answer = finalAnswerGeneral,
            Sources = generalSearchResults.Select(s => new SourceInfo
            {
                Content = s.Content,
                Similarity = s.Similarity,
                Type = s.Metadata?.GetValueOrDefault("type")?.ToString() ?? "unknown",
                ConventionId = s.Metadata is not null && s.Metadata.TryGetValue("convention_id", out var convIdObj) && convIdObj is JsonElement convIdElement && convIdElement.TryGetInt32(out var convId) ? convId : null,
                ConventionTitle = s.Metadata?.GetValueOrDefault("title")?.ToString()
            }).ToList(),
            LlmProvider = _llmProvider.ProviderName
        };
    }


    public async Task<List<string>> GetSuggestedQuestionsAsync(int cId, ChatUserContext? uCtx)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(cId);
        if (convention is null) return new List<string>();

        var suggestions = new List<string> { $"{convention.Title} 행사는 언제 진행되나요?", "오늘 일정은 무엇인가요?" };
        if (uCtx?.Role == UserRole.Guest)
        {            suggestions.Add("내 정보를 알려주세요.");
            suggestions.Add("내 전체 일정을 알려주세요.");
        }
        return suggestions;
    }
    public async Task<ChatResponse> AskAboutConventionAsync(int conventionId, string question, ChatUserContext? userContext = null, List<ChatMessage>? history = null)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention is null) throw new ArgumentException($"Convention {conventionId} not found");

        if (userContext is not null)
        {
            if (!await VerifyConventionAccess(conventionId, userContext))
                throw new UnauthorizedAccessException("해당 행사에 접근 권한이 없습니다.");
        }
        return await AskAsync(question, conventionId, userContext, history);
    }
    private async Task<bool> VerifyConventionAccess(int conventionId, ChatUserContext userContext)
    {
        if (userContext.GuestId is null) return false;
        return userContext.Role switch
        {
            UserRole.Admin => true,
            UserRole.Guest => await _context.Guests.AnyAsync(g => g.Id == userContext.GuestId && g.ConventionId == conventionId),
            _ => false,
        };
    }
}
