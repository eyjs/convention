using LocalRAG.Interfaces;
using LocalRAG.Models;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using LocalRAG.Data;

namespace LocalRAG.Services;

public class ConventionChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConventionChatService> _logger;
    private readonly ConventionDbContext _context;
    private readonly ILlmProvider _llmProvider;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;

    public ConventionChatService(
        IUnitOfWork unitOfWork,
        ILogger<ConventionChatService> logger,
        ConventionDbContext context,
        ILlmProvider llmProvider,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _context = context;
        _llmProvider = llmProvider;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task<ChatResponse> AskAsync(string question, int? conventionId = null, ChatUserContext? userContext = null)
    {
        _logger.LogInformation("Processing question: '{Question}' for GuestId: {GuestId}", question, userContext?.GuestId);

        if (userContext?.GuestId.HasValue == true)
        {
            var directAnswer = await HandlePersonalizedQuestions(question, userContext.GuestId.Value);
            if (directAnswer != null)
            {
                _logger.LogInformation("Direct answer provided for question: '{Question}'", question);
                return new ChatResponse { Answer = directAnswer, LlmProvider = "Direct Answer" };
            }
        }

        try
        {
            _logger.LogInformation("Step 1: Retrieving documents with original question: {Question}", question);
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);
            var searchResults = await _vectorStore.SearchAsync(queryEmbedding, 5);
            var sources = await FilterSourcesByPermission(searchResults, userContext);
            var contextForLlm = sources.Any() ? string.Join("\n\n", sources.Select(s => s.Content)) : null;

            var finalPrompt = await AugmentQuestionForLlm(question, userContext);

            _logger.LogInformation("Step 2 & 3: Augmenting and Generating response with {Provider}", _llmProvider.ProviderName);
            var finalAnswer = await _llmProvider.GenerateResponseAsync(finalPrompt, contextForLlm);

            var response = new ChatResponse
            {
                Answer = finalAnswer,
                Sources = sources.Select(s => new SourceInfo
                {
                    Content = s.Content,
                    Similarity = s.Similarity,
                    Type = s.Metadata?.GetValueOrDefault("type")?.ToString() ?? "unknown",
                    ConventionId = s.Metadata is not null && s.Metadata.TryGetValue("convention_id", out var convIdObj) && convIdObj is JsonElement convIdElement && convIdElement.TryGetInt32(out var convId) ? convId : null,
                    ConventionTitle = s.Metadata?.GetValueOrDefault("title")?.ToString()
                }).ToList(),
                LlmProvider = _llmProvider.ProviderName
            };

            _logger.LogInformation("RAG pipeline completed. Found {SourceCount} sources.", response.Sources.Count);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RAG pipeline for question: {Question}", question);
            throw;
        }
    }

    // [수정] 누락되었던 AskAboutConventionAsync 메서드를 다시 추가합니다.
    public async Task<ChatResponse> AskAboutConventionAsync(int conventionId, string question, ChatUserContext? userContext = null)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention is null) throw new ArgumentException($"Convention {conventionId} not found");

        if (userContext is not null)
        {
            if (!await VerifyConventionAccess(conventionId, userContext))
                throw new UnauthorizedAccessException("해당 행사에 접근 권한이 없습니다.");
        }
        return await AskAsync(question, conventionId, userContext);
    }

    private async Task<string?> HandlePersonalizedQuestions(string question, int guestId)
    {
        var guest = await _context.Guests
            .Include(g => g.GuestAttributes)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == guestId);

        if (guest is null) return null;

        var q = question.Trim().ToLower();
        if ((q.Contains("나") || q.Contains("내") || q.Contains("my")) && (q.Contains("누구") || q.Contains("정보") || q.Contains("이름")))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"네, {guest.GuestName}님의 정보를 알려드릴게요.");
            if (!string.IsNullOrEmpty(guest.CorpPart)) sb.AppendLine($"- 부서: {guest.CorpPart}");
            if (!string.IsNullOrEmpty(guest.Telephone)) sb.AppendLine($"- 연락처: {guest.Telephone}");
            if (guest.GuestAttributes?.Any() == true)
            {
                sb.AppendLine("- 추가 정보:");
                foreach (var attr in guest.GuestAttributes) sb.AppendLine($"  - {attr.AttributeKey}: {attr.AttributeValue}");
            }
            return sb.ToString();
        }

        if ((q.Contains("나") || q.Contains("내") || q.Contains("my")) && q.Contains("일정"))
        {
            var guestSchedules = await _context.GuestScheduleTemplates
                .Where(gst => gst.GuestId == guestId)
                .Include(gst => gst.ScheduleTemplate) // ThenInclude 사용을 위해 Include 추가
                .ThenInclude(st => st!.ScheduleItems) // st가 null일 수 있음을 알림(!)
                .AsNoTracking()
                .ToListAsync();

            var allItems = guestSchedules
                .Select(gst => gst.ScheduleTemplate)
                .Where(st => st is not null) // Null check for ScheduleTemplate
                .SelectMany(st => st.ScheduleItems)
                .OrderBy(si => si.ScheduleDate).ThenBy(si => si.StartTime).ToList();

            if (!allItems.Any()) return $"{guest.GuestName}님에게는 아직 배정된 일정이 없습니다.";

            var sb = new StringBuilder();
            var today = DateTime.Today;

            if (q.Contains("오늘"))
            {
                var todayItems = allItems.Where(i => i.ScheduleDate.Date == today).ToList();
                if (!todayItems.Any()) return "오늘은 예정된 일정이 없습니다.";
                sb.AppendLine($"{guest.GuestName}님의 오늘 일정입니다.");
                foreach (var item in todayItems) sb.AppendLine($"- {item.StartTime}: {item.Title}{(string.IsNullOrEmpty(item.Location) ? "" : $" (장소: {item.Location})")}");
            }
            else
            {
                sb.AppendLine($"{guest.GuestName}님의 전체 일정입니다.");
                foreach (var item in allItems) sb.AppendLine($"- {item.ScheduleDate:MM/dd} {item.StartTime}: {item.Title}{(string.IsNullOrEmpty(item.Location) ? "" : $" (장소: {item.Location})")}");
            }
            return sb.ToString();
        }

        return null;
    }

    public async Task<ChatResponse> AskWithHistoryAsync(string question, List<ChatMessage> history, int? conventionId = null, ChatUserContext? userContext = null)
    {
        var historyText = string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));
        var fullQuestion = $"[대화 기록]\n{historyText}\n\n[현재 질문]\n{question}";
        return await AskAsync(fullQuestion, conventionId, userContext);
    }

    private async Task<string> AugmentQuestionForLlm(string question, ChatUserContext? userContext)
    {
        if (userContext?.GuestId is null) return question;
        var guest = await _context.Guests.FindAsync(userContext.GuestId.Value);
        if (guest is null) return question;
        return $"You are answering a question from a user named '{guest.GuestName}'. Keep this user context in mind. The user's question is: {question}";
    }

    private Task<List<VectorSearchResult>> FilterSourcesByPermission(List<VectorSearchResult> sources, ChatUserContext? userContext)
    {
        if (userContext is null)
            return Task.FromResult(sources.Where(s => s.Metadata?.GetValueOrDefault("type")?.ToString() == "convention").ToList());

        var filteredSources = userContext.Role switch
        {
            UserRole.Admin => sources,
            UserRole.Guest => sources.Where(s =>
            {
                var type = s.Metadata?.GetValueOrDefault("type")?.ToString();
                if (type == "convention") return true;
                if (type == "guest" && s.Metadata is not null && s.Metadata.TryGetValue("guest_id", out var guestIdObj) && guestIdObj is JsonElement guestIdElement && guestIdElement.TryGetInt32(out var guestId))
                    return guestId == userContext.GuestId;
                return false;
            }).ToList(),
            _ => new List<VectorSearchResult>()
        };
        return Task.FromResult(filteredSources);
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync(int conventionId, ChatUserContext? userContext = null)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention is null) return new List<string>();

        var suggestions = new List<string> { $"{convention.Title} 행사는 언제 진행되나요?", "오늘 일정은 무엇인가요?", "행사 담당자를 알려주세요." };
        if (userContext?.Role == UserRole.Guest)
        {
            suggestions.Add("내 정보를 알려주세요.");
            suggestions.Add("내 전체 일정을 알려주세요.");
        }
        if (userContext?.Role == UserRole.Admin)
        {
            suggestions.Add("참석자 명단을 보여주세요.");
            suggestions.Add("전체 참석자는 몇 명인가요?");
        }
        return suggestions;
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