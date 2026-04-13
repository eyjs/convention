using System.Text.Json;
using LocalRAG.DTOs.SeatingModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Convention;

public class SeatingLayoutService : ISeatingLayoutService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeatingLayoutService> _logger;

    public SeatingLayoutService(IUnitOfWork unitOfWork, ILogger<SeatingLayoutService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<SeatingLayoutListItemDto>> GetByConventionAsync(int conventionId)
    {
        var layouts = await _unitOfWork.SeatingLayouts.Query
            .Where(l => l.ConventionId == conventionId && !l.IsDeleted)
            .OrderByDescending(l => l.UpdatedAt ?? l.CreatedAt)
            .ToListAsync();

        return layouts.Select(ToListItem).ToList();
    }

    public async Task<SeatingLayoutDto?> GetByIdAsync(int id)
    {
        var layout = await _unitOfWork.SeatingLayouts.GetByIdAsync(id);
        if (layout == null || layout.IsDeleted) return null;
        return ToDto(layout);
    }

    public async Task<SeatingLayoutDto> CreateAsync(int conventionId, CreateSeatingLayoutRequest request)
    {
        var layout = new SeatingLayout
        {
            ConventionId = conventionId,
            Name = request.Name,
            Description = request.Description,
            CanvasWidth = request.CanvasWidth,
            CanvasHeight = request.CanvasHeight,
            LayoutJson = "{\"tables\":[],\"decors\":[],\"lines\":[]}",
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.SeatingLayouts.AddAsync(layout);
        await _unitOfWork.SaveChangesAsync();
        return ToDto(layout);
    }

    public async Task<SeatingLayoutDto?> UpdateAsync(int id, UpdateSeatingLayoutRequest request)
    {
        var layout = await _unitOfWork.SeatingLayouts.GetByIdAsync(id);
        if (layout == null || layout.IsDeleted) return null;

        if (request.Name != null) layout.Name = request.Name;
        if (request.Description != null) layout.Description = request.Description;
        if (request.CanvasWidth.HasValue) layout.CanvasWidth = request.CanvasWidth.Value;
        if (request.CanvasHeight.HasValue) layout.CanvasHeight = request.CanvasHeight.Value;
        if (request.LayoutJson != null) layout.LayoutJson = request.LayoutJson;
        if (request.BackgroundImageUrl != null) layout.BackgroundImageUrl = request.BackgroundImageUrl;
        layout.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.SeatingLayouts.Update(layout);
        await _unitOfWork.SaveChangesAsync();
        return ToDto(layout);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var layout = await _unitOfWork.SeatingLayouts.GetByIdAsync(id);
        if (layout == null) return false;
        layout.IsDeleted = true;
        layout.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.SeatingLayouts.Update(layout);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<SeatingLayoutDto?> SetBackgroundAsync(int id, string url)
    {
        var layout = await _unitOfWork.SeatingLayouts.GetByIdAsync(id);
        if (layout == null || layout.IsDeleted) return null;
        layout.BackgroundImageUrl = url;
        layout.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.SeatingLayouts.Update(layout);
        await _unitOfWork.SaveChangesAsync();
        return ToDto(layout);
    }

    public async Task<SeatingLayoutDto?> DuplicateAsync(int id)
    {
        var src = await _unitOfWork.SeatingLayouts.GetByIdAsync(id);
        if (src == null || src.IsDeleted) return null;

        var copy = new SeatingLayout
        {
            ConventionId = src.ConventionId,
            Name = src.Name + " (복사본)",
            Description = src.Description,
            BackgroundImageUrl = src.BackgroundImageUrl,
            CanvasWidth = src.CanvasWidth,
            CanvasHeight = src.CanvasHeight,
            LayoutJson = src.LayoutJson,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.SeatingLayouts.AddAsync(copy);
        await _unitOfWork.SaveChangesAsync();
        return ToDto(copy);
    }

    public async Task<List<SeatingLayoutListItemDto>> GetMyLayoutsAsync(int conventionId, int userId)
    {
        var layouts = await _unitOfWork.SeatingLayouts.Query
            .Where(l => l.ConventionId == conventionId && !l.IsDeleted)
            .OrderByDescending(l => l.UpdatedAt ?? l.CreatedAt)
            .ToListAsync();

        // 해당 사용자가 배정된 레이아웃만 필터 (LayoutJson 파싱)
        var result = new List<SeatingLayoutListItemDto>();
        foreach (var l in layouts)
        {
            if (HasUserAssigned(l.LayoutJson, userId))
            {
                result.Add(ToListItem(l));
            }
        }
        return result;
    }

    // ===== JSON 분석 헬퍼 (pins 구조 + 레거시 tables 구조 모두 지원) =====
    private static (int pinCount, int assignedCount) AnalyzeLayout(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            int pins = 0, assigned = 0;

            // 새 구조: pins 배열
            if (root.TryGetProperty("pins", out var pinsArr) && pinsArr.ValueKind == JsonValueKind.Array)
            {
                pins = pinsArr.GetArrayLength();
                foreach (var p in pinsArr.EnumerateArray())
                {
                    if (p.TryGetProperty("userId", out var uid) && uid.ValueKind != JsonValueKind.Null)
                        assigned++;
                }
                return (pins, assigned);
            }

            // tables 구조 (새: members / 레거시: seats)
            if (root.TryGetProperty("tables", out var tablesArr) && tablesArr.ValueKind == JsonValueKind.Array)
            {
                pins = tablesArr.GetArrayLength(); // 테이블 수
                foreach (var t in tablesArr.EnumerateArray())
                {
                    // members (새 구조)
                    if (t.TryGetProperty("members", out var members) && members.ValueKind == JsonValueKind.Array)
                    {
                        assigned += members.GetArrayLength();
                    }
                    // seats (레거시)
                    else if (t.TryGetProperty("seats", out var seats) && seats.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var s in seats.EnumerateArray())
                        {
                            if (s.TryGetProperty("userId", out var uid) && uid.ValueKind != JsonValueKind.Null)
                                assigned++;
                        }
                    }
                }
            }
            return (pins, assigned);
        }
        catch { return (0, 0); }
    }

    private static bool HasUserAssigned(string json, int userId)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 새 구조: pins
            if (root.TryGetProperty("pins", out var pinsArr) && pinsArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in pinsArr.EnumerateArray())
                {
                    if (p.TryGetProperty("userId", out var uid) && uid.ValueKind == JsonValueKind.Number && uid.GetInt32() == userId)
                        return true;
                }
                return false;
            }

            // tables.members (새 테이블 구조) 또는 tables.seats (레거시)
            if (root.TryGetProperty("tables", out var tablesArr) && tablesArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in tablesArr.EnumerateArray())
                {
                    // members (새 구조)
                    if (t.TryGetProperty("members", out var members) && members.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var m in members.EnumerateArray())
                        {
                            if (m.TryGetProperty("userId", out var uid) && uid.ValueKind == JsonValueKind.Number && uid.GetInt32() == userId)
                                return true;
                        }
                    }
                    // seats (레거시)
                    if (t.TryGetProperty("seats", out var seats) && seats.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var s in seats.EnumerateArray())
                        {
                            if (s.TryGetProperty("userId", out var uid2) && uid2.ValueKind == JsonValueKind.Number && uid2.GetInt32() == userId)
                                return true;
                        }
                    }
                }
            }
            return false;
        }
        catch { return false; }
    }

    private static SeatingLayoutDto ToDto(SeatingLayout l) => new()
    {
        Id = l.Id,
        ConventionId = l.ConventionId,
        Name = l.Name,
        Description = l.Description,
        BackgroundImageUrl = l.BackgroundImageUrl,
        CanvasWidth = l.CanvasWidth,
        CanvasHeight = l.CanvasHeight,
        LayoutJson = l.LayoutJson,
        CreatedAt = l.CreatedAt,
        UpdatedAt = l.UpdatedAt
    };

    private static SeatingLayoutListItemDto ToListItem(SeatingLayout l)
    {
        var (tables, assigned) = AnalyzeLayout(l.LayoutJson);
        return new SeatingLayoutListItemDto
        {
            Id = l.Id,
            Name = l.Name,
            Description = l.Description,
            BackgroundImageUrl = l.BackgroundImageUrl,
            TableCount = tables,
            AssignedCount = assigned,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        };
    }

    // ===== 엑셀 다운로드/업로드 (테이블 배정) =====
    public async Task<(byte[] bytes, string filename)?> DownloadMembersExcelAsync(int layoutId)
    {
        var layout = await _unitOfWork.SeatingLayouts.GetByIdAsync(layoutId);
        if (layout == null || layout.IsDeleted) return null;

        // 행사 참석자 조회
        var guests = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == layout.ConventionId)
            .Include(uc => uc.User)
            .OrderBy(uc => uc.User.Name)
            .ToListAsync();

        // 현재 배정 정보 파싱
        var tableMap = new Dictionary<int, string>(); // userId → tableNumber
        try
        {
            using var doc = JsonDocument.Parse(layout.LayoutJson ?? "{}");
            if (doc.RootElement.TryGetProperty("tables", out var tables))
            {
                foreach (var t in tables.EnumerateArray())
                {
                    var num = t.TryGetProperty("number", out var n) ? n.GetString() ?? "" : "";
                    if (t.TryGetProperty("members", out var members))
                    {
                        foreach (var m in members.EnumerateArray())
                        {
                            if (m.TryGetProperty("userId", out var uid) && uid.ValueKind == JsonValueKind.Number)
                                tableMap[uid.GetInt32()] = num;
                        }
                    }
                }
            }
        }
        catch { }

        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using var package = new OfficeOpenXml.ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("좌석배정");
        sheet.Cells[1, 1].Value = "번호";
        sheet.Cells[1, 2].Value = "이름";
        sheet.Cells[1, 3].Value = "전화번호";
        sheet.Cells[1, 4].Value = "테이블번호";

        var row = 2;
        var seq = 1;
        foreach (var uc in guests)
        {
            sheet.Cells[row, 1].Value = seq++;
            sheet.Cells[row, 2].Value = uc.User.Name;
            sheet.Cells[row, 3].Value = uc.User.Phone;
            sheet.Cells[row, 4].Value = tableMap.GetValueOrDefault(uc.User.Id, "");
            row++;
        }
        sheet.Cells[sheet.Dimension?.Address ?? "A1"].AutoFitColumns();

        return (package.GetAsByteArray(), $"좌석배정_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    public async Task<object> UploadMembersExcelAsync(int layoutId, Stream stream)
    {
        var layout = await _unitOfWork.SeatingLayouts.GetByIdAsync(layoutId);
        if (layout == null || layout.IsDeleted) return new { success = false, message = "레이아웃을 찾을 수 없습니다." };

        // 참석자 조회 (이름+전화번호로 매칭)
        var guests = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == layout.ConventionId)
            .Include(uc => uc.User)
            .ToListAsync();

        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using var package = new OfficeOpenXml.ExcelPackage(stream);
        var sheet = package.Workbook.Worksheets[0];
        if (sheet?.Dimension == null) return new { success = false, message = "시트가 비어있습니다." };

        // 기존 테이블 구조 파싱
        var layoutData = new Dictionary<string, JsonElement>();
        List<JsonElement> existingTables = new();
        try
        {
            using var doc = JsonDocument.Parse(layout.LayoutJson ?? "{}");
            if (doc.RootElement.TryGetProperty("tables", out var tables))
                existingTables = tables.EnumerateArray().ToList();
        }
        catch { }

        // 엑셀 파싱: 이름(2열), 전화(3열), 테이블번호(4열)
        var assignments = new Dictionary<string, List<(int userId, string name)>>(); // tableNumber → members
        int matched = 0, unmatched = 0;

        for (int r = 2; r <= sheet.Dimension.Rows; r++)
        {
            var name = sheet.Cells[r, 2].Text?.Trim();
            var phone = sheet.Cells[r, 3].Text?.Trim()?.Replace("-", "");
            var tableNum = sheet.Cells[r, 4].Text?.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(tableNum)) continue;

            // 매칭
            var uc = guests.FirstOrDefault(g =>
                g.User.Name == name && (g.User.Phone?.Replace("-", "") == phone || string.IsNullOrEmpty(phone)));

            if (uc == null) { unmatched++; continue; }

            if (!assignments.ContainsKey(tableNum)) assignments[tableNum] = new();
            assignments[tableNum].Add((uc.User.Id, uc.User.Name));
            matched++;
        }

        // 기존 테이블 구조에 members 업데이트
        var tablesJson = new List<object>();
        foreach (var et in existingTables)
        {
            var num = et.TryGetProperty("number", out var n) ? n.GetString() ?? "" : "";
            var members = assignments.ContainsKey(num)
                ? assignments[num].Select(m => (object)new { userId = m.userId, name = m.name }).ToList()
                : new List<object>();
            // 기존 속성 유지 + members 교체
            tablesJson.Add(new
            {
                id = et.TryGetProperty("id", out var id) ? id.GetString() : $"t{num}",
                number = num,
                label = et.TryGetProperty("label", out var lbl) ? lbl.GetString() : $"{num}번",
                shape = et.TryGetProperty("shape", out var sh) ? sh.GetString() : "circle",
                x = et.TryGetProperty("x", out var x) ? x.GetDouble() : 100,
                y = et.TryGetProperty("y", out var y) ? y.GetDouble() : 100,
                width = et.TryGetProperty("width", out var w) ? w.GetDouble() : 80,
                height = et.TryGetProperty("height", out var h) ? h.GetDouble() : 80,
                color = et.TryGetProperty("color", out var c) ? c.GetString() : "#3b82f6",
                members,
            });
            assignments.Remove(num); // 처리됨
        }

        // 엑셀에만 있고 마커가 없는 테이블번호 → 새 테이블 자동 생성
        foreach (var (num, members) in assignments)
        {
            tablesJson.Add(new
            {
                id = $"t_auto_{num}",
                number = num,
                label = $"{num}번",
                shape = "circle",
                x = 100 + tablesJson.Count * 100,
                y = 200,
                width = 80,
                height = 80,
                color = "#3b82f6",
                members = members.Select(m => new { userId = m.userId, name = m.name }).ToList(),
            });
        }

        layout.LayoutJson = System.Text.Json.JsonSerializer.Serialize(new { tables = tablesJson });
        layout.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.SeatingLayouts.Update(layout);
        await _unitOfWork.SaveChangesAsync();

        return new { success = true, matched, unmatched, tableCount = tablesJson.Count };
    }
}
