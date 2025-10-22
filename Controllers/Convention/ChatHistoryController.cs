using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Models;
using System.Security.Claims;
namespace LocalRAG.Controllers.Convention
{
    [ApiController]
    [Route("api/chat-history")]
    [Authorize]
    public class ChatHistoryController : ControllerBase
    {
        private readonly ConventionDbContext _context;
        private readonly ILogger<ChatHistoryController> _logger;
        public ChatHistoryController(ConventionDbContext context, ILogger<ChatHistoryController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet("{conventionId}")]
        public async Task<IActionResult> GetChatHistory(int conventionId)
        {
            var userConventionIdClaim = User.FindFirst("ConventionId")?.Value;
            if (userConventionIdClaim == null || !int.TryParse(userConventionIdClaim, out var userConventionId) || userConventionId != conventionId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You do not have access to this convention's chat history.");
            }
            try
            {
                var messagesFromDb = await _context.ConventionChatMessages
                    .Where(m => m.ConventionId == conventionId)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();
                var result = messagesFromDb.Select(m => new ChatHistoryMessageDto
                {
                    guestId = m.GuestId,
                    guestName = m.IsAdmin 
                        ? $"[관리자] {m.GuestName ?? "Unknown User"}" 
                        : m.GuestName ?? "Unknown User",
                    message = m.Message,
                    createdAt = m.CreatedAt.ToString("o"),
                    isAdmin = m.IsAdmin
                });
                return Ok(result);
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId && g.ConventionId == conventionId);

            if (guest == null)
            {
                // This might happen if a user is an admin but not a guest, handle as needed.
                // For now, we just return Ok since there's nothing to update.
                return Ok();
            }

            guest.LastChatReadTimestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
public class ChatHistoryMessageDto
{
    public int guestId { get; set; }
    public required string guestName { get; set; }
    public required string message { get; set; }
    public required string createdAt { get; set; }
    public bool isAdmin { get; set; }
}