using LocalRAG.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers;

/// <summary>
/// Convention 챗봇 API
/// 
/// 사용자가 Convention 데이터에 대해 자연어로 질문하고 답변을 받을 수 있습니다.
/// </summary>
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

    /// <summary>
    /// 일반 질문
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// POST /api/conventionchat/ask
    /// {
    ///   "question": "이번 주에 예정된 행사가 있나요?"
    /// }
    /// </remarks>
    [HttpPost("ask")]
    public async Task<ActionResult<ChatResponse>> Ask([FromBody] AskRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { message = "질문을 입력해주세요." });
            }

            var response = await _chatService.AskAsync(
                request.Question, 
                request.ConventionId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question");
            return StatusCode(500, new { message = "답변 생성 중 오류가 발생했습니다.", error = ex.Message });
        }
    }

    /// <summary>
    /// 특정 행사에 대한 질문
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// POST /api/conventionchat/conventions/1/ask
    /// {
    ///   "question": "이번 행사의 일정을 알려주세요"
    /// }
    /// </remarks>
    [HttpPost("conventions/{conventionId}/ask")]
    public async Task<ActionResult<ChatResponse>> AskAboutConvention(
        int conventionId,
        [FromBody] AskRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { message = "질문을 입력해주세요." });
            }

            var response = await _chatService.AskAboutConventionAsync(
                conventionId, 
                request.Question);

            return Ok(response);
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

    /// <summary>
    /// 추천 질문 가져오기
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// GET /api/conventionchat/conventions/1/suggestions
    /// </remarks>
    [HttpGet("conventions/{conventionId}/suggestions")]
    public async Task<ActionResult<List<string>>> GetSuggestedQuestions(int conventionId)
    {
        try
        {
            var suggestions = await _chatService.GetSuggestedQuestionsAsync(conventionId);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions for convention {ConventionId}", conventionId);
            return StatusCode(500, new { message = "추천 질문 생성 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 전체 데이터 재색인
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// POST /api/conventionchat/reindex
    /// </remarks>
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
                message = $"색인 완료. 성공: {result.SuccessCount}, 실패: {result.FailureCount}",
                result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reindexing");
            return StatusCode(500, new { message = "재색인 중 오류가 발생했습니다.", error = ex.Message });
        }
    }

    /// <summary>
    /// 특정 행사 색인
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// POST /api/conventionchat/conventions/1/index
    /// </remarks>
    [HttpPost("conventions/{conventionId}/index")]
    public async Task<ActionResult> IndexConvention(int conventionId)
    {
        try
        {
            var documentId = await _indexingService.IndexConventionAsync(conventionId);
            
            return Ok(new
            {
                success = true,
                message = $"Convention {conventionId} 색인 완료",
                documentId
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

    /// <summary>
    /// 대화 히스토리를 포함한 질문 (추후 구현)
    /// </summary>
    [HttpPost("ask/with-history")]
    public async Task<ActionResult<ChatResponse>> AskWithHistory(
        [FromBody] AskWithHistoryRequest request)
    {
        try
        {
            var response = await _chatService.AskWithHistoryAsync(
                request.Question,
                request.History ?? new List<ChatMessage>(),
                request.ConventionId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question with history");
            return StatusCode(500, new { message = "답변 생성 중 오류가 발생했습니다." });
        }
    }
}

// ============================================================
// DTOs
// ============================================================

/// <summary>
/// 질문 요청
/// </summary>
public class AskRequest
{
    public string Question { get; set; } = string.Empty;
    public int? ConventionId { get; set; }
}

/// <summary>
/// 히스토리 포함 질문 요청
/// </summary>
public class AskWithHistoryRequest
{
    public string Question { get; set; } = string.Empty;
    public int? ConventionId { get; set; }
    public List<ChatMessage>? History { get; set; }
}
