using System.Text.Json;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Utilities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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

        // 빈 날짜 제거 (모든 필드가 비어있으면 삭제)
        days.RemoveAll(d =>
            string.IsNullOrEmpty(d.Bus) &&
            string.IsNullOrEmpty(d.Hotel) &&
            string.IsNullOrEmpty(d.Room) &&
            string.IsNullOrEmpty(d.Memo));

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

            // 빈 날짜 제거
            days.RemoveAll(d =>
                string.IsNullOrEmpty(d.Bus) &&
                string.IsNullOrEmpty(d.Hotel) &&
                string.IsNullOrEmpty(d.Room) &&
                string.IsNullOrEmpty(d.Memo));

            days = days.OrderBy(d => d.Date).ToList();
            var json = JsonSerializer.Serialize(new { days }, JsonOptions);
            await _unitOfWork.GuestAttributes.UpsertAttributeAsync(userId, TRAVEL_INFO_KEY, json);
            result.Updated++;
        }

        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    public async Task<int> RemoveDateAsync(int conventionId, string date)
    {
        var userIds = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Select(uc => uc.UserId)
            .ToListAsync();

        var attributes = await _unitOfWork.GuestAttributes.Query
            .Where(ga => userIds.Contains(ga.UserId) && ga.AttributeKey == TRAVEL_INFO_KEY)
            .ToListAsync();

        var updated = 0;
        foreach (var attr in attributes)
        {
            var days = ParseDays(attr.AttributeValue);
            if (days == null) continue;

            var before = days.Count;
            days.RemoveAll(d => d.Date == date);
            if (days.Count == before) continue;

            var json = JsonSerializer.Serialize(new { days }, JsonOptions);
            attr.AttributeValue = json;
            updated++;
        }

        if (updated > 0)
            await _unitOfWork.SaveChangesAsync();

        return updated;
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

    /// <summary>
    /// 엑셀 업로드: 시트명=날짜(YYYY-MM-DD 또는 M/D), 행=이름|전화번호|호차|호텔|방번호|메모
    /// </summary>
    public async Task<TravelUploadResult> UploadFromExcelAsync(int conventionId, Stream excelStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var result = new TravelUploadResult();

        try
        {
            using var package = new ExcelPackage(excelStream);
            if (package.Workbook.Worksheets.Count == 0)
            {
                result.Errors.Add("Excel 파일에 시트가 없습니다.");
                return result;
            }

            // 행사 참석자 로드 (이름+전화번호 매칭용)
            var userConventions = await _unitOfWork.UserConventions.Query
                .Where(uc => uc.ConventionId == conventionId)
                .Select(uc => uc.UserId)
                .ToListAsync();

            var users = await _unitOfWork.Users.Query
                .Where(u => userConventions.Contains(u.Id))
                .Select(u => new { u.Id, u.Name, u.Phone })
                .ToListAsync();

            var allUpdates = new List<TravelDayUpdateRequest>();

            foreach (var sheet in package.Workbook.Worksheets)
            {
                if (sheet.Dimension == null) continue;

                // 시트명에서 날짜 파싱
                var date = ParseSheetDate(sheet.Name);
                if (date == null)
                {
                    result.Warnings.Add($"시트 '{sheet.Name}': 날짜 형식을 인식할 수 없습니다. (YYYY-MM-DD 또는 M/D 형식)");
                    continue;
                }

                result.SheetsProcessed++;
                var rowCount = sheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var name = sheet.Cells[row, 1].Text?.Trim();
                    var phone = sheet.Cells[row, 2].Text?.Trim();
                    var bus = sheet.Cells[row, 3].Text?.Trim();
                    var hotel = sheet.Cells[row, 4].Text?.Trim();
                    var room = sheet.Cells[row, 5].Text?.Trim();
                    var memo = sheet.Cells[row, 6].Text?.Trim();

                    if (string.IsNullOrEmpty(name)) continue;

                    // 이름 + 전화번호로 매칭
                    var matched = users.FirstOrDefault(u =>
                    {
                        if (u.Name != name) return false;
                        if (string.IsNullOrEmpty(phone)) return true; // 이름만으로 유일 매칭
                        var normalizedPhone = PhoneNumberFormatter.Normalize(phone);
                        return !string.IsNullOrEmpty(u.Phone) &&
                               PhoneNumberFormatter.Normalize(u.Phone) == normalizedPhone;
                    });

                    if (matched == null)
                    {
                        result.UsersNotFound++;
                        result.Warnings.Add($"시트 '{sheet.Name}' Row {row}: '{name}' 참석자를 찾을 수 없습니다.");
                        continue;
                    }

                    result.UsersMatched++;
                    allUpdates.Add(new TravelDayUpdateRequest
                    {
                        UserId = matched.Id,
                        Date = date,
                        Bus = string.IsNullOrEmpty(bus) ? null : bus,
                        Hotel = string.IsNullOrEmpty(hotel) ? null : hotel,
                        Room = string.IsNullOrEmpty(room) ? null : room,
                        Memo = string.IsNullOrEmpty(memo) ? null : memo,
                    });
                }
            }

            if (allUpdates.Count > 0)
            {
                await BulkUpdateAsync(conventionId, allUpdates);
            }

            result.Success = true;
            _logger.LogInformation(
                "Travel upload: {Sheets} sheets, {Matched} matched, {NotFound} not found",
                result.SheetsProcessed, result.UsersMatched, result.UsersNotFound);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Travel assignment Excel upload failed");
            result.Errors.Add($"업로드 실패: {ex.Message}");
        }

        return result;
    }

    private static string? ParseSheetDate(string sheetName)
    {
        // "2026-04-01" 형태
        if (DateTime.TryParse(sheetName, out var parsed))
            return parsed.ToString("yyyy-MM-dd");

        // "4/1", "04/01", "4월1일" 등 → 올해 기준
        var cleaned = sheetName.Replace("월", "/").Replace("일", "").Trim();
        if (DateTime.TryParse(cleaned, out var parsed2))
            return new DateTime(DateTime.Now.Year, parsed2.Month, parsed2.Day).ToString("yyyy-MM-dd");

        return null;
    }
}
