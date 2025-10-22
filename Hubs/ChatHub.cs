using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using LocalRAG.Data;
using LocalRAG.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Hubs
{
    public class ParticipantInfo
    {
        public required string Name { get; set; }
        public required string Affiliation { get; set; }
    }

    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ConventionDbContext _context;
        private readonly ILogger<ChatHub> _logger;
        // RoomName -> (ConnectionId -> ParticipantInfo)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ParticipantInfo>> _rooms = new ConcurrentDictionary<string, ConcurrentDictionary<string, ParticipantInfo>>();

        public ChatHub(ConventionDbContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null) { Context.Abort(); return; }

            var conventionIdStr = httpContext.Request.Query["conventionId"].ToString();
            if (string.IsNullOrEmpty(conventionIdStr) || !int.TryParse(conventionIdStr, out var conventionId)) 
            { 
                Context.Abort(); 
                return; 
            }

            var guestIdStr = Context.User?.FindFirstValue("GuestId");
            if (string.IsNullOrEmpty(guestIdStr) || !int.TryParse(guestIdStr, out var guestId))
            {
                Context.Abort();
                return;
            }

            var guest = await _context.Guests.FindAsync(guestId);
            if (guest == null)
            {
                Context.Abort();
                return;
            }

            var roomName = GetRoomName(conventionIdStr);
            var room = _rooms.GetOrAdd(roomName, new ConcurrentDictionary<string, ParticipantInfo>());
            
            var participant = new ParticipantInfo 
            {
                Name = guest.GuestName,
                Affiliation = guest.CorpName ?? "소속 정보 없음"
            };
            room[Context.ConnectionId] = participant;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            
            _logger.LogInformation($"Client {Context.ConnectionId} ({guest.GuestName}) connected to room {roomName}.");

            await Clients.Group(roomName).SendAsync("UpdateParticipantCount", room.Count);
            await Clients.Group(roomName).SendAsync("UpdateParticipantList", room.Values.ToList());
            
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string message)
        {
            var guestIdClaim = Context.User?.FindFirst("GuestId");
            var guestNameClaim = Context.User?.FindFirst(ClaimTypes.Name);
            var conventionIdClaim = Context.User?.FindFirst("ConventionId");
            var isAdminClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value == "Admin";

            if (guestIdClaim == null || conventionIdClaim == null || guestNameClaim == null)
            {
                _logger.LogWarning($"SendMessage aborted: Missing claims for user {Context.UserIdentifier}.");
                return;
            }

            var guestId = int.Parse(guestIdClaim.Value);
            var conventionId = int.Parse(conventionIdClaim.Value);
            var guestName = guestNameClaim.Value;

            var chatMessage = new ConventionChatMessage
            {
                ConventionId = conventionId,
                GuestId = guestId,
                GuestName = guestName,
                Message = message,
                IsAdmin = isAdminClaim,
                CreatedAt = DateTime.UtcNow
            };

            _context.ConventionChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var displayName = isAdminClaim ? $"[관리자] {guestName}" : guestName;
            var roomName = GetRoomName(conventionId.ToString());

            await Clients.Group(roomName).SendAsync("ReceiveMessage", new 
            {
                guestId = chatMessage.GuestId,
                guestName = displayName,
                message = chatMessage.Message,
                createdAt = chatMessage.CreatedAt.ToString("o"), // ISO 8601 format
                isAdmin = chatMessage.IsAdmin
            });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var roomPair = _rooms.FirstOrDefault(p => p.Value.ContainsKey(Context.ConnectionId));
            if (!roomPair.Equals(default(KeyValuePair<string, ConcurrentDictionary<string, ParticipantInfo>>)))
            {
                var roomName = roomPair.Key;
                var room = roomPair.Value;
                if (room.TryRemove(Context.ConnectionId, out _))
                {
                    _logger.LogInformation($"Client {Context.ConnectionId} disconnected from room {roomName}.");
                    await Clients.Group(roomName).SendAsync("UpdateParticipantCount", room.Count);
                    await Clients.Group(roomName).SendAsync("UpdateParticipantList", room.Values.ToList());
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private string GetRoomName(string conventionId) => $"convention-{conventionId}";
    }
}
