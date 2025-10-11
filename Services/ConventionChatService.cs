using LocalRAG.Interfaces;
using LocalRAG.Repositories;

namespace LocalRAG.Services;

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

    public async Task<ChatResponse> AskAsync(string question, int? conventionId = null, ChatUserContext? userContext = null)
    {
        _logger.LogInformation("Processing question: {Question}, ConventionId: {ConventionId}, Role: {Role}", 
            question, conventionId, userContext?.Role);

        try
        {
            var ragResponse = await _ragService.QueryAsync(question, topK: 5);
            var sources = ragResponse.Sources;

            if (conventionId.HasValue)
            {
                sources = sources
                    .Where(s => s.Metadata != null && 
                           s.Metadata.ContainsKey("convention_id") && 
                           (int)s.Metadata["convention_id"] == conventionId.Value)
                    .ToList();
            }

            if (userContext != null)
            {
                sources = await FilterSourcesByPermission(sources, userContext);
            }

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

            _logger.LogInformation("Answer generated. Sources: {SourceCount}, Provider: {Provider}", 
                response.Sources.Count, response.LlmProvider);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question: {Question}", question);
            throw;
        }
    }

    private async Task<List<VectorSearchResult>> FilterSourcesByPermission(List<VectorSearchResult> sources, ChatUserContext userContext)
    {
        switch (userContext.Role)
        {
            case UserRole.Admin:
                return sources;

            case UserRole.Guest:
                if (!userContext.GuestId.HasValue)
                    return new List<VectorSearchResult>();

                return sources.Where(s =>
                {
                    if (s.Metadata == null) return false;

                    var type = s.Metadata.GetValueOrDefault("type")?.ToString();
                    
                    if (type == "convention" || type == "schedule" || type == "menu")
                        return true;

                    if (type == "guest" && s.Metadata.ContainsKey("guest_id"))
                    {
                        var guestId = s.Metadata["guest_id"];
                        return guestId != null && (int)guestId == userContext.GuestId.Value;
                    }

                    return false;
                }).ToList();

            default:
                return new List<VectorSearchResult>();
        }
    }

    public async Task<ChatResponse> AskAboutConventionAsync(int conventionId, string question, ChatUserContext? userContext = null)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null)
        {
            throw new ArgumentException($"Convention {conventionId} not found");
        }

        if (userContext != null)
        {
            var hasAccess = await VerifyConventionAccess(conventionId, userContext);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("해당 행사에 접근 권한이 없습니다.");
            }
        }

        return await AskAsync(question, conventionId, userContext);
    }

    private async Task<bool> VerifyConventionAccess(int conventionId, ChatUserContext userContext)
    {
        switch (userContext.Role)
        {
            case UserRole.Admin:
                return true;

            case UserRole.Guest:
                if (!userContext.GuestId.HasValue) return false;
                var guest = await _unitOfWork.Guests.GetByIdAsync(userContext.GuestId.Value);
                return guest?.ConventionId == conventionId;

            default:
                return false;
        }
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync(int conventionId, ChatUserContext? userContext = null)
    {
        var convention = await _unitOfWork.Conventions.GetConventionWithDetailsAsync(conventionId);
        if (convention == null)
        {
            return new List<string>();
        }

        var suggestions = new List<string>
        {
            $"{convention.Title} 행사는 언제 진행되나요?",
            "오늘 일정은 무엇인가요?",
            "행사 일정을 알려주세요"
        };

        if (userContext?.Role == UserRole.Guest)
        {
            suggestions.Add("내 정보를 알려주세요");
            suggestions.Add("내 일정을 알려주세요");
        }

        if (userContext?.Role == UserRole.Admin)
        {
            suggestions.Add("참석자 명단을 알려주세요");
            suggestions.Add("참석자는 총 몇 명인가요?");
        }

        return suggestions;
    }

    public async Task<ChatResponse> AskWithHistoryAsync(string question, List<ChatMessage> history, int? conventionId = null, ChatUserContext? userContext = null)
    {
        return await AskAsync(question, conventionId, userContext);
    }
}

public enum UserRole
{
    Admin,
    Guest
}

public class ChatUserContext
{
    public UserRole Role { get; set; }
    public int? GuestId { get; set; }
    public string? MemberId { get; set; }
}

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceInfo> Sources { get; set; } = new();
    public string LlmProvider { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class SourceInfo
{
    public string Content { get; set; } = string.Empty;
    public float Similarity { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? ConventionId { get; set; }
    public string? ConventionTitle { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
