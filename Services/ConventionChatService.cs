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

    public async Task<ChatResponse> AskAsync(string question, int? conventionId = null, ChatUserContext? userContext = null)
    {
        _logger.LogInformation("Processing question: '{Question}' for GuestId: {GuestId}", question, userContext?.GuestId);

        // 1. [핵심] 질문 의도 파악
        var intent = await _llmProvider.ClassifyIntentAsync(question);
        _logger.LogInformation("Classified intent as: {Intent}", intent);

        // 2. 의도에 따라 분기 처리
        if (userContext?.GuestId.HasValue == true)
        {
            switch (intent)
            {
                case "personal_info":
                    var info = await GetPersonalInfo(userContext.GuestId.Value);
                    return new ChatResponse { Answer = info, LlmProvider = "Direct Answer" };

                case "personal_schedule":
                    var schedule = await GetPersonalSchedule(userContext.GuestId.Value, question);
                    return new ChatResponse { Answer = schedule, LlmProvider = "Direct Answer" };
            }
        }

        // 3. 의도가 'general_query'이거나 로그인하지 않은 경우, RAG 파이프라인 실행
        try
        {
            _logger.LogInformation("Intent is '{Intent}', proceeding with RAG pipeline.", intent);
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);
            var searchResults = await _vectorStore.SearchAsync(queryEmbedding, 5);
            var sources = await FilterSourcesByPermission(searchResults, userContext);
            var contextForLlm = sources.Any() ? string.Join("\n\n", sources.Select(s => s.Content)) : null;

            var userInfoForLlm = await GetUserInfoForLlm(userContext);
            var finalAnswer = await _llmProvider.GenerateResponseAsync(question, contextForLlm, userInfoForLlm);

            return new ChatResponse
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RAG pipeline for question: {Question}", question);
            throw;
        }
    }

    private async Task<string> GetPersonalInfo(int guestId)
    {
        var guest = await _context.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guestId);
        if (guest is null) return "참석자 정보를 찾을 수 없습니다.";

        var sb = new StringBuilder();
        sb.AppendLine($"네, {guest.GuestName}님의 정보를 알려드릴게요.");
        if (!string.IsNullOrEmpty(guest.CorpPart)) sb.AppendLine($"- 부서: {guest.CorpPart}");
        if (!string.IsNullOrEmpty(guest.Telephone)) sb.AppendLine($"- 연락처: {guest.Telephone}");
        return sb.ToString();
    }

    private async Task<string> GetPersonalSchedule(int guestId, string question)
    {
        var guest = await _context.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guestId);
        if (guest is null) return "참석자 정보를 찾을 수 없습니다.";

        var guestSchedules = await _context.GuestScheduleTemplates
            .Where(gst => gst.GuestId == guestId)
            .Include(gst => gst.ScheduleTemplate.ScheduleItems)
            .AsNoTracking().ToListAsync();

        var allItems = guestSchedules
            .Select(gst => gst.ScheduleTemplate)
            .Where(st => st != null)
            .SelectMany(st => st.ScheduleItems)
            .OrderBy(si => si.ScheduleDate).ThenBy(si => si.StartTime).ToList();

        if (!allItems.Any()) return $"{guest.GuestName}님에게는 아직 배정된 일정이 없습니다.";

        var sb = new StringBuilder();
        if (question.Contains("오늘"))
        {
            var todayItems = allItems.Where(i => i.ScheduleDate.Date == DateTime.Today).ToList();
            if (!todayItems.Any()) return "오늘은 예정된 일정이 없습니다.";
            sb.AppendLine($"{guest.GuestName}님의 오늘 일정입니다.");
            foreach (var item in todayItems) sb.AppendLine($"- {item.StartTime}: {item.Title}");
        }
        else
        {
            sb.AppendLine($"{guest.GuestName}님의 전체 일정입니다.");
            foreach (var item in allItems) sb.AppendLine($"- {item.ScheduleDate:MM/dd} {item.StartTime}: {item.Title}");
        }
        return sb.ToString();
    }

    public async Task<ChatResponse> AskWithHistoryAsync(string q, List<ChatMessage> h, int? cId, ChatUserContext? uCtx) { return await AskAsync($"{string.Join("\n", h.Select(m => $"{m.Role}: {m.Content}"))}\nQuestion: {q}", cId, uCtx); }
    private async Task<string?> GetUserInfoForLlm(ChatUserContext? ctx) { if (ctx?.GuestId is null) return null; var g = await _context.Guests.FindAsync(ctx.GuestId.Value); return g is null ? null : $"The user asking the question is named '{g.GuestName}'."; }
    private Task<List<VectorSearchResult>> FilterSourcesByPermission(List<VectorSearchResult> s, ChatUserContext? u)
    {
        if (u is null) return Task.FromResult(s.Where(res => res.Metadata?.GetValueOrDefault("type")?.ToString() == "convention").ToList());
        var filtered = u.Role switch
        {
            UserRole.Admin => s,
            UserRole.Guest => s.Where(res => {
                var type = res.Metadata?.GetValueOrDefault("type")?.ToString();
                if (type == "convention") return true;
                if (type == "guest" && res.Metadata != null && res.Metadata.TryGetValue("guest_id", out var gIdObj) && gIdObj is JsonElement gIdEl && gIdEl.TryGetInt32(out var gId)) return gId == u.GuestId;
                return false;
            }).ToList(),
            _ => new List<VectorSearchResult>()
        };
        return Task.FromResult(filtered);
    }
    public async Task<List<string>> GetSuggestedQuestionsAsync(int cId, ChatUserContext? uCtx)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(cId);
        if (convention is null) return new List<string>();

        var suggestions = new List<string> { $"{convention.Title} 행사는 언제 진행되나요?", "오늘 일정은 무엇인가요?" };
        if (uCtx?.Role == UserRole.Guest)
        {
            suggestions.Add("내 정보를 알려주세요.");
            suggestions.Add("내 전체 일정을 알려주세요.");
        }
        return suggestions;
    }
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