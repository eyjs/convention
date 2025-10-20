using LocalRAG.Interfaces;
using LocalRAG.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LocalRAG.Controllers.Ai;

[ApiController]
[Route("api/[controller]")]
public class ConventionChatController : ControllerBase
{
    private readonly IConventionChatService _chatService;
    private readonly IUserContextFactory _userContextFactory;
    private readonly ILogger<ConventionChatController> _logger;

    public ConventionChatController(
        IConventionChatService chatService,
        IUserContextFactory userContextFactory,
        ILogger<ConventionChatController> logger)
    {
        _chatService = chatService;
        _userContextFactory = userContextFactory;
        _logger = logger;
    }

    // 통합된 질문 처리 엔드포인트
    [HttpPost("ask")]
    [Authorize] // 챗봇 API는 인증된 사용자만 접근 가능
    [HttpPost("conventions/{conventionId}/ask")]
    public async Task<ActionResult<ChatResponse>> Ask(
        [FromBody] AskRequest request,
        [FromRoute] int? conventionId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { message = "질문을 입력해주세요." });

            var userContext = _userContextFactory.CreateUserContext();
            if (userContext == null)
            {
                // UserContextFactory에서 인증되지 않은 경우 Anonymous로 처리하므로,
                // 이 경우는 토큰 파싱 자체에 실패한 심각한 오류일 수 있습니다.
                return Unauthorized(new { message = "인증 정보가 유효하지 않습니다." });
            }
            var effectiveConventionId = conventionId ?? request.ConventionId;

            // 서비스 호출 시에는 안전하게 생성된 userContext를 전달합니다.
            var response = await _chatService.AskAsync(
                request.Question,
                effectiveConventionId,
                userContext,
                request.History);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // 서비스 단에서 발생한 권한 없음 예외는 403 Forbidden으로 처리하는 것이 더 적절합니다.
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat question (ConventionId: {ConventionId})", conventionId);
            return StatusCode(500, new { message = "답변 생성 중 오류가 발생했습니다.", error = ex.Message });
        }
    }
}

public class AskRequest
{
    public string Question { get; set; } = string.Empty;
    public int? ConventionId { get; set; }
    public List<ChatRequestMessage>? History { get; set; }
}
// ===== Request Models =====

