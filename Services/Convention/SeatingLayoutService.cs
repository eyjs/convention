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

            // 레거시: tables.seats
            if (root.TryGetProperty("tables", out var tablesArr) && tablesArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in tablesArr.EnumerateArray())
                {
                    if (t.TryGetProperty("seats", out var seats) && seats.ValueKind == JsonValueKind.Array)
                    {
                        pins += seats.GetArrayLength();
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

            // 레거시: tables.seats
            if (root.TryGetProperty("tables", out var tablesArr) && tablesArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in tablesArr.EnumerateArray())
                {
                    if (!t.TryGetProperty("seats", out var seats)) continue;
                    foreach (var s in seats.EnumerateArray())
                    {
                        if (s.TryGetProperty("userId", out var uid) && uid.ValueKind == JsonValueKind.Number && uid.GetInt32() == userId)
                            return true;
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
}
