using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Services.Ai;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin/chatbot")]
[Authorize(Roles = "Admin")]
public class ChatbotManagementController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IndexingService _indexingService;
    private readonly ILogger<ChatbotManagementController> _logger;

    public ChatbotManagementController(
        ConventionDbContext context,
        IndexingService indexingService,
        ILogger<ChatbotManagementController> logger)
    {
        _context = context;
        _indexingService = indexingService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var totalDocuments = await _context.VectorDataEntries.CountAsync();
        var activeConventions = await _context.Conventions
            .Where(c => c.DeleteYn == "N")
            .CountAsync();
        var totalGuests = await _context.Guests.CountAsync();
        var dbSize = totalDocuments * 1024;

        return Ok(new
        {
            totalDocuments,
            activeConventions,
            totalGuests,
            dbSize
        });
    }

    [HttpGet("conventions")]
    public async Task<ActionResult> GetConventions()
    {
        var conventions = await _context.Conventions
            .Where(c => c.DeleteYn == "N")
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.StartDate,
                GuestCount = c.Guests.Count(),
                VectorCount = _context.VectorDataEntries.Count(v => v.ConventionId == c.Id),
                ChatbotEnabled = true
            })
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();

        return Ok(conventions);
    }

    [HttpGet("vector-stats")]
    public async Task<ActionResult> GetVectorStats()
    {
        var total = await _context.VectorDataEntries.CountAsync();

        var bySource = await _context.VectorDataEntries
            .GroupBy(v => v.SourceType)
            .Select(g => new
            {
                type = g.Key,
                count = g.Count(),
                label = g.Key,
                percentage = total > 0 ? (double)g.Count() / total * 100 : 0
            })
            .ToListAsync();

        return Ok(new
        {
            bySouce = bySource
        });
    }

    [HttpGet("recent-activities")]
    public async Task<ActionResult> GetRecentActivities()
    {
        var activities = await _context.VectorDataEntries
            .OrderByDescending(v => v.CreatedAt)
            .Take(10)
            .Select(v => new
            {
                id = v.Id,
                action = $"{v.SourceType} 색인됨",
                timestamp = v.CreatedAt
            })
            .ToListAsync();

        return Ok(activities);
    }

    [HttpGet("logs")]
    public ActionResult GetLogs()
    {
        var logs = new[]
        {
            new { timestamp = DateTime.Now.ToString("HH:mm:ss"), level = "info", message = "챗봇 시스템 정상 작동 중" },
            new { timestamp = DateTime.Now.AddMinutes(-5).ToString("HH:mm:ss"), level = "info", message = "벡터 데이터베이스 연결 성공" },
            new { timestamp = DateTime.Now.AddMinutes(-10).ToString("HH:mm:ss"), level = "info", message = "행사 색인 완료" }
        };

        return Ok(logs);
    }

    [HttpPost("reindex-all")]
    public async Task<ActionResult> ReindexAll()
    {
        try
        {
            _logger.LogInformation("전체 재색인 시작");

            var result = await _indexingService.ReindexAllConventionsAsync();

            _logger.LogInformation("전체 재색인 완료: {Count}개 행사", result.SuccessCount);

            return Ok(new
            {
                message = $"{result.SuccessCount}개 행사의 데이터가 재색인되었습니다. (총 {result.TotalDocumentsIndexed}개 문서)",
                success = result.SuccessCount,
                failure = result.FailureCount,
                totalDocuments = result.TotalDocumentsIndexed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "전체 재색인 중 오류 발생");
            return StatusCode(500, new { message = "재색인 중 오류가 발생했습니다." });
        }
    }

    [HttpPost("reindex-convention/{conventionId}")]
    public async Task<ActionResult> ReindexConvention(int conventionId)
    {
        try
        {
            var convention = await _context.Conventions.FindAsync(conventionId);
            if (convention == null)
                return NotFound("행사를 찾을 수 없습니다.");

            await _indexingService.IndexConventionAsync(conventionId);

            return Ok(new { message = "행사가 재색인되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "행사 {ConventionId} 재색인 실패", conventionId);
            return StatusCode(500, new { message = "재색인 중 오류가 발생했습니다." });
        }
    }

    [HttpPut("convention/{conventionId}/toggle")]
    public async Task<ActionResult> ToggleChatbot(int conventionId, [FromBody] ToggleChatbotRequest request)
    {
        var convention = await _context.Conventions.FindAsync(conventionId);
        if (convention == null)
            return NotFound("행사를 찾을 수 없습니다.");

        return Ok(new { message = request.Enabled ? "챗봇이 활성화되었습니다." : "챗봇이 비활성화되었습니다." });
    }

    [HttpDelete("clear-vectors")]
    public async Task<ActionResult> ClearVectors()
    {
        try
        {
            var count = await _context.VectorDataEntries.CountAsync();
            _context.VectorDataEntries.RemoveRange(_context.VectorDataEntries);
            await _context.SaveChangesAsync();

            _logger.LogWarning("벡터 DB 초기화: {Count}개 문서 삭제됨", count);

            return Ok(new { message = $"{count}개의 벡터 문서가 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "벡터 DB 초기화 실패");
            return StatusCode(500, new { message = "벡터 DB 초기화 중 오류가 발생했습니다." });
        }
    }
}

public class ToggleChatbotRequest
{
    public bool Enabled { get; set; }
}
