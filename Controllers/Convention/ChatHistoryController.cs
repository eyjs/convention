using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.DTOs.ChatModels;

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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var isUserInConvention = await _context.UserConventions
                .AnyAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

            var isAdmin = User.IsInRole("Admin");

            if (!isUserInConvention && !isAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You do not have access to this convention's chat history.");
            }
            try
            {
                var messages = await _context.ConventionChatMessages
                    .Where(m => m.ConventionId == conventionId)
                    .OrderBy(m => m.CreatedAt)
                    .Join( // Join with the Users table
                        _context.Users,
                        chatMessage => chatMessage.UserId, // Key from ConventionChatMessages
                        user => user.Id,                   // Key from Users
                        (chatMessage, user) => new ChatHistoryMessageDto // Project into the DTO
                        {
                            userId = chatMessage.UserId,
                            userName = chatMessage.IsAdmin
                                ? $"{user.Name}"
                                : user.Name,
                            profileImageUrl = user.ProfileImageUrl,
                            message = chatMessage.Message,
                            createdAt = chatMessage.CreatedAt.ToString("o"),
                            isAdmin = chatMessage.IsAdmin
                        })
                    .ToListAsync();

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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var userConvention = await _context.UserConventions
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

            if (userConvention == null)
            {
                // This might happen if a user is an admin but not in convention, handle as needed.
                // For now, we just return Ok since there's nothing to update.
                return Ok();
            }

            userConvention.LastChatReadTimestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}