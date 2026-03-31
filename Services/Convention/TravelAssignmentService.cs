using System.Text.Json;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Convention;

public class TravelAssignmentService : ITravelAssignmentService
{
    private const string TRAVEL_INFO_KEY = "travel_info";
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TravelAssignmentService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public TravelAssignmentService(IUnitOfWork unitOfWork, ILogger<TravelAssignmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TravelInfoResponse?> GetMyTravelInfoAsync(int conventionId, int userId)
    {
        var attribute = await _unitOfWork.GuestAttributes.GetAttributeByKeyAsync(userId, TRAVEL_INFO_KEY);
        if (attribute == null) return null;

        var days = ParseDays(attribute.AttributeValue);
        if (days == null || days.Count == 0) return null;

        // 룸메이트 조회: 같은 행사의 참석자 ID를 한번만 조회
        var conventionUserIds = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Select(uc => uc.UserId)
            .ToListAsync();

        var allTravelAttrs = await GetTravelAttributesByUserIds(conventionUserIds);
        var userNames = await GetUserNamesByIds(conventionUserIds);

        var result = new TravelInfoResponse();
        foreach (var day in days)
        {
            var info = new TravelDayInfo
            {
                Date = day.Date,
                Bus = day.Bus,
                Hotel = day.Hotel,
                Room = day.Room,
                Memo = day.Memo,
            };

            // 방번호가 있으면 같은 날짜+같은 방의 다른 유저 찾기
            if (!string.IsNullOrEmpty(day.Room))
            {
                info.Roommates = FindRoommates(allTravelAttrs, userNames, userId, day.Date, day.Room);
            }

            result.Days.Add(info);
        }

        return result;
    }

    public async Task<List<UserTravelAssignment>> GetAllAssignmentsAsync(int conventionId)
    {
        // 행사 참석자 전체 조회
        var userConventions = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Select(uc => new { uc.UserId, uc.GroupName })
            .ToListAsync();

        var userIds = userConventions.Select(uc => uc.UserId).ToList();

        var users = await _unitOfWork.Users.Query
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name })
            .ToListAsync();

        // travel_info 속성 조회
        var attributes = await _unitOfWork.GuestAttributes.Query
            .Where(ga => userIds.Contains(ga.UserId) && ga.AttributeKey == TRAVEL_INFO_KEY)
            .ToListAsync();

        var attrMap = attributes.ToDictionary(a => a.UserId, a => a.AttributeValue);
        var groupMap = userConventions.ToDictionary(uc => uc.UserId, uc => uc.GroupName);

        return users.Select(u => new UserTravelAssignment
        {
            UserId = u.Id,
            UserName = u.Name,
            GroupName = groupMap.GetValueOrDefault(u.Id),
            Days = attrMap.TryGetValue(u.Id, out var json) ? ParseDays(json) ?? new() : new(),
        }).OrderBy(u => u.GroupName).ThenBy(u => u.UserName).ToList();
    }

    public async Task UpdateTravelDayAsync(int conventionId, int userId, TravelDayUpdateRequest request)
    {
        // 기존 데이터 로드
        var attribute = await _unitOfWork.GuestAttributes.GetAttributeByKeyAsync(userId, TRAVEL_INFO_KEY);
        var days = attribute != null ? ParseDays(attribute.AttributeValue) ?? new() : new();

        // 해당 날짜 찾아서 업데이트 (없으면 추가)
        var existing = days.FirstOrDefault(d => d.Date == request.Date);
        if (existing != null)
        {
            existing.Bus = request.Bus;
            existing.Hotel = request.Hotel;
            existing.Room = request.Room;
            existing.Memo = request.Memo;
        }
        else
        {
            days.Add(new TravelDayData
            {
                Date = request.Date,
                Bus = request.Bus,
                Hotel = request.Hotel,
                Room = request.Room,
                Memo = request.Memo,
            });
        }

        // 날짜 순 정렬 후 저장
        days = days.OrderBy(d => d.Date).ToList();
        var json = JsonSerializer.Serialize(new { days }, JsonOptions);
        await _unitOfWork.GuestAttributes.UpsertAttributeAsync(userId, TRAVEL_INFO_KEY, json);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<BulkTravelUpdateResult> BulkUpdateAsync(int conventionId, List<TravelDayUpdateRequest> assignments)
    {
        var result = new BulkTravelUpdateResult();

        // 관련 유저의 기존 데이터 일괄 로드
        var userIds = assignments.Select(a => a.UserId).Distinct().ToList();
        var attributes = await _unitOfWork.GuestAttributes.Query
            .Where(ga => userIds.Contains(ga.UserId) && ga.AttributeKey == TRAVEL_INFO_KEY)
            .ToListAsync();

        var attrMap = attributes.ToDictionary(a => a.UserId);

        // 유저별로 그룹핑하여 처리
        var grouped = assignments.GroupBy(a => a.UserId);
        foreach (var group in grouped)
        {
            var userId = group.Key;
            var days = attrMap.TryGetValue(userId, out var attr)
                ? ParseDays(attr.AttributeValue) ?? new()
                : new();

            foreach (var req in group)
            {
                var existing = days.FirstOrDefault(d => d.Date == req.Date);
                if (existing != null)
                {
                    existing.Bus = req.Bus;
                    existing.Hotel = req.Hotel;
                    existing.Room = req.Room;
                    existing.Memo = req.Memo;
                }
                else
                {
                    days.Add(new TravelDayData
                    {
                        Date = req.Date,
                        Bus = req.Bus,
                        Hotel = req.Hotel,
                        Room = req.Room,
                        Memo = req.Memo,
                    });
                }
            }

            days = days.OrderBy(d => d.Date).ToList();
            var json = JsonSerializer.Serialize(new { days }, JsonOptions);
            await _unitOfWork.GuestAttributes.UpsertAttributeAsync(userId, TRAVEL_INFO_KEY, json);
            result.Updated++;
        }

        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    // --- Private Helpers ---

    private List<TravelDayData>? ParseDays(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("days", out var daysElement))
            {
                return JsonSerializer.Deserialize<List<TravelDayData>>(daysElement.GetRawText(), JsonOptions);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse travel_info JSON");
            return null;
        }
    }

    private async Task<Dictionary<int, List<TravelDayData>>> GetTravelAttributesByUserIds(List<int> userIds)
    {
        var attrs = await _unitOfWork.GuestAttributes.Query
            .Where(ga => userIds.Contains(ga.UserId) && ga.AttributeKey == TRAVEL_INFO_KEY)
            .Select(ga => new { ga.UserId, ga.AttributeValue })
            .ToListAsync();

        var result = new Dictionary<int, List<TravelDayData>>();
        foreach (var a in attrs)
        {
            var days = ParseDays(a.AttributeValue);
            if (days != null) result[a.UserId] = days;
        }
        return result;
    }

    private async Task<Dictionary<int, string>> GetUserNamesByIds(List<int> userIds)
    {
        return await _unitOfWork.Users.Query
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);
    }

    private List<string> FindRoommates(
        Dictionary<int, List<TravelDayData>> allTravel,
        Dictionary<int, string> userNames,
        int currentUserId,
        string date,
        string room)
    {
        var roommates = new List<string>();
        foreach (var (userId, days) in allTravel)
        {
            if (userId == currentUserId) continue;
            var day = days.FirstOrDefault(d => d.Date == date && d.Room == room);
            if (day != null && userNames.TryGetValue(userId, out var name))
            {
                roommates.Add(name);
            }
        }
        return roommates;
    }
}
