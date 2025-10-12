using LocalRAG.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConventionChatController : ControllerBase
{
    private readonly ConventionChatService _chatService;
    private readonly ConventionIndexingService _indexingService;
    private readonly ILogger<ConventionChatController> _logger;

    public ConventionChatController(
        ConventionChatService chatService,
        ConventionIndexingService indexingService,
        ILogger<ConventionChatController> logger)
    {
        _chatService = chatService;
        _indexingService = indexingService;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<ChatResponse>> Ask([FromBody] AskRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { message = "질문을 입력해주세요." });
            }

            var userContext = CreateUserContext(request);
            var response = await _chatService.AskAsync(request.Question, request.ConventionId, userContext);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question");
            return StatusCode(500, new { message = "답변 생성 중 오류가 발생했습니다.", error = ex.Message });
        }
    }

    [HttpPost("conventions/{conventionId}/ask")]
    public async Task<ActionResult<ChatResponse>> AskAboutConvention(int conventionId, [FromBody] AskRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { message = "질문을 입력해주세요." });
            }

            var userContext = CreateUserContext(request);
            // [수정] 누락되었던 AskAboutConventionAsync 호출을 복구합니다.
            var response = await _chatService.AskAboutConventionAsync(conventionId, request.Question, userContext);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question for convention {ConventionId}", conventionId);
            return StatusCode(500, new { message = "답변 생성 중 오류가 발생했습니다.", error = ex.Message });
        }
    }

    [HttpGet("conventions/{conventionId}/suggestions")]
    public async Task<ActionResult<List<string>>> GetSuggestedQuestions(int conventionId, [FromQuery] string? role = null, [FromQuery] int? guestId = null)
    {
        try
        {
            ChatUserContext? userContext = null;
            if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                userContext = new ChatUserContext
                {
                    Role = userRole,
                    GuestId = guestId
                };
            }

            var suggestions = await _chatService.GetSuggestedQuestionsAsync(conventionId, userContext);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions for convention {ConventionId}", conventionId);
            return StatusCode(500, new { message = "추천 질문 생성 중 오류가 발생했습니다." });
        }
    }

    [HttpPost("reindex")]
    public async Task<ActionResult<IndexingResult>> ReindexAll()
    {
        try
        {
            _logger.LogInformation("Starting full reindex");
            var result = await _indexingService.ReindexAllConventionsAsync();

            return Ok(new
            {
                success = true,
                message = $"색인 완료. 처리된 행사: {result.SuccessCount}, 총 색인된 문서: {result.TotalDocumentsIndexed}",
                result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reindexing");
            return StatusCode(500, new { message = "재색인 중 오류가 발생했습니다.", error = ex.Message });
        }
    }

    [HttpPost("conventions/{conventionId}/index")]
    public async Task<ActionResult> IndexConvention(int conventionId)
    {
        try
        {
            var documentCount = await _indexingService.IndexConventionAsync(conventionId);

            return Ok(new
            {
                success = true,
                message = $"Convention {conventionId} 색인 완료. {documentCount}개 문서가 색인되었습니다.",
                documentCount
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing convention {ConventionId}", conventionId);
            return StatusCode(500, new { message = "색인 중 오류가 발생했습니다.", error = ex.Message });
        }
    }

    [HttpPost("ask/with-history")]
    public async Task<ActionResult<ChatResponse>> AskWithHistory([FromBody] AskWithHistoryRequest request)
    {
        try
        {
            var userContext = CreateUserContext(request);
            var response = await _chatService.AskWithHistoryAsync(
                request.Question,
                request.History ?? new List<ChatMessage>(),
                request.ConventionId,
                userContext);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question with history");
            return StatusCode(500, new { message = "답변 생성 중 오류가 발생했습니다." });
        }
    }

    private ChatUserContext? CreateUserContext(BaseAskRequest request)
    {
        if (string.IsNullOrEmpty(request.Role) || !Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            return null;

        return new ChatUserContext
        {
            Role = userRole,
            GuestId = request.GuestId,
            MemberId = request.MemberId
        };
    }
}

public class BaseAskRequest
{
    public string Question { get; set; } = string.Empty;
    public int? ConventionId { get; set; }
    public string? Role { get; set; }
    public int? GuestId { get; set; }
    public string? MemberId { get; set; }
}

public class AskRequest : BaseAskRequest
{
}

public class AskWithHistoryRequest : BaseAskRequest
{
    public List<ChatMessage>? History { get; set; }
}