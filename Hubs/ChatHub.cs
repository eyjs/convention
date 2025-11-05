using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using LocalRAG.Data;
using LocalRAG.Entities;

namespace LocalRAG.Hubs;

public class ParticipantInfo
{
    public string Name { get; set; } = string.Empty;
    public string Affiliation { get; set; } = string.Empty;
    public string CorpPart { get; set; } = string.Empty;
}

public class ChatHub : Hub
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ChatHub> _logger;
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ParticipantInfo>> _rooms = new();

    public ChatHub(ConventionDbContext context, ILogger<ChatHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetRoomName(string conventionId) => $"convention-{conventionId}";

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null) { Context.Abort(); return; }

        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("Connection aborted: User ID is missing or invalid.");
            Context.Abort();
            return;
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"Connection aborted: User with ID {userId} not found.");
            Context.Abort();
            return;
        }

        // 1. 개인 그룹에 자동 가입 (Unread count 알림용)
        var userGroup = $"user-{userId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, userGroup);
        _logger.LogInformation($"Client {Context.ConnectionId} ({user.Name}) joined personal group {userGroup}.");

        // 2. Convention 채팅방 그룹 가입 (conventionId가 있는 경우에만)
        var conventionIdStr = httpContext.Request.Query["conventionId"].ToString();
        if (!string.IsNullOrEmpty(conventionIdStr) && int.TryParse(conventionIdStr, out var conventionId))
        {
            var roomName = GetRoomName(conventionIdStr);
            var room = _rooms.GetOrAdd(roomName, new ConcurrentDictionary<string, ParticipantInfo>());

            var participant = new ParticipantInfo
            {
                Name = user.Name,
                Affiliation = user.Affiliation ?? "소속 정보 없음",
                CorpPart = user.CorpPart
            };
            room[Context.ConnectionId] = participant;

            Context.Items["ConventionId"] = conventionId; // Store conventionId for later use
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            _logger.LogInformation($"Client {Context.ConnectionId} ({user.Name}) connected to room {roomName}.");

            await Clients.Group(roomName).SendAsync("UpdateParticipantCount", room.Count);
            await Clients.Group(roomName).SendAsync("UpdateParticipantList", room.Values.ToList());
        }

        await base.OnConnectedAsync();
    }
    
    public async Task JoinUserGroup(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        _logger.LogInformation($"Client {Context.ConnectionId} explicitly joined user group: user-{userId}");
    }

    public async Task SendMessage(string message, int conventionId)
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("SendMessage aborted: User ID is missing or invalid.");
            return;
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        var chatMessage = new ConventionChatMessage
        {
            ConventionId = conventionId,
            UserId = userId,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsAdmin = user.Role == "Admin"
        };

        _context.ConventionChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        var roomName = GetRoomName(conventionId.ToString());

        // 1. 채팅방에 있는 모든 클라이언트에게 메시지 전송
        await Clients.Group(roomName).SendAsync("ReceiveMessage", new
        {
            userId = chatMessage.UserId,
            userName = user.Name,
            message = chatMessage.Message,
            createdAt = chatMessage.CreatedAt.ToString("o"), // ISO 8601 format
            isAdmin = chatMessage.IsAdmin
        });

        // 2. 실시간 unread count 업데이트: 해당 convention 참가자들에게 알림
        var participantUserIds = await _context.UserConventions
            .Where(uc => uc.ConventionId == conventionId)
            .Select(uc => uc.UserId)
            .Distinct()
            .ToListAsync();

        // 각 참가자에게 개인 알림 전송 (본인 제외)
        foreach (var participantId in participantUserIds)
        {
            if (participantId != userId) // 메시지 보낸 사람 제외
            {
                var userGroup = $"user-{participantId}";
                await Clients.Group(userGroup).SendAsync("UnreadCountIncrement", new
                {
                    conventionId = conventionId
                });
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue("ConventionId", out var conventionIdObj) && conventionIdObj is int conventionId)
        {
            var roomName = GetRoomName(conventionId.ToString());
            if (_rooms.TryGetValue(roomName, out var room))
            {
                if (room.TryRemove(Context.ConnectionId, out _))
                {
                    await Clients.Group(roomName).SendAsync("UpdateParticipantCount", room.Count);
                    await Clients.Group(roomName).SendAsync("UpdateParticipantList", room.Values.ToList());
                    _logger.LogInformation($"Client {Context.ConnectionId} disconnected from room {roomName}.");
                }
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}
