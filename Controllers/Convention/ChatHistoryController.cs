using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.Interfaces;
using LocalRAG.Extensions;

namespace LocalRAG.Controllers.Convention
{
    [ApiController]
    [Route("api/chat-history")]
    [Authorize]
    public class ChatHistoryController : ControllerBase
    {
        private readonly IChatHistoryService _chatHistoryService;
        private readonly ILogger<ChatHistoryController> _logger;

        public ChatHistoryController(IChatHistoryService chatHistoryService, ILogger<ChatHistoryController> logger)
        {
            _chatHistoryService = chatHistoryService;
            _logger = logger;
        }

        [HttpGet("{conventionId}")]
        public async Task<IActionResult> GetChatHistory(int conventionId)
        {
            var userId = User.GetUserIdOrNull();
            if (userId == null)
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                var hasAccess = await _chatHistoryService.HasConventionAccessAsync(userId.Value, conventionId);
                if (!hasAccess)
                    return StatusCode(StatusCodes.Status403Forbidden, "You do not have access to this convention's chat history.");
            }

            try
            {
                var messages = await _chatHistoryService.GetChatHistoryAsync(conventionId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chat history for convention {ConventionId}", conventionId);
                return StatusCode(500, "An error occurred while fetching chat history.");
            }
        }

        [HttpPost("{conventionId}/read")]
        public async Task<IActionResult> MarkAsRead(int conventionId)
        {
            var userId = User.GetUserIdOrNull();
            if (userId == null)
                return Unauthorized();

            await _chatHistoryService.MarkAsReadAsync(userId.Value, conventionId);
            return NoContent();
        }
    }
}
