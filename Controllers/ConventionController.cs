using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Models.Convention;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConventionController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ConventionController> _logger;

    public ConventionController(ConventionDbContext context, ILogger<ConventionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // 연결 테스트
    [HttpGet("test-connection")]
    public async Task<ActionResult<object>> TestConnection()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            var totalCount = await _context.CorpConventions.CountAsync();
            
            return Ok(new
            {
                CanConnect = canConnect,
                DatabaseName = _context.Database.GetDbConnection().Database,
                TotalConventions = totalCount,
                TestTime = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Convention database connection test failed");
            return BadRequest(new { Error = ex.Message });
        }
    }

    // 컨벤션 목록 조회 (페이징, 필터링)
    [HttpGet]
    public async Task<ActionResult<object>> GetConventions(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? memberId = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? title = null,
        [FromQuery] string? orderBy = "reg_dtm",
        [FromQuery] string? orderDir = "desc")
    {
        try
        {
            var query = _context.CorpConventions.AsQueryable();

            // 필터링
            if (!string.IsNullOrEmpty(memberId))
                query = query.Where(c => c.MemberId == memberId);

            if (isDeleted.HasValue)
                query = query.Where(c => (c.DeleteYn == "Y") == isDeleted.Value);
            else
                query = query.Where(c => c.DeleteYn != "Y"); // 기본적으로 삭제되지 않은 것만

            if (fromDate.HasValue)
                query = query.Where(c => c.StartDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(c => c.StartDate <= toDate.Value);

            if (!string.IsNullOrEmpty(title))
                query = query.Where(c => c.Title!.Contains(title) || c.SubTitle!.Contains(title));

            // 정렬
            query = orderBy?.ToLower() switch
            {
                "title" => orderDir?.ToLower() == "desc" ? 
                    query.OrderByDescending(c => c.Title) : 
                    query.OrderBy(c => c.Title),
                "start_date" => orderDir?.ToLower() == "desc" ? 
                    query.OrderByDescending(c => c.StartDate) : 
                    query.OrderBy(c => c.StartDate),
                "member_id" => orderDir?.ToLower() == "desc" ? 
                    query.OrderByDescending(c => c.MemberId) : 
                    query.OrderBy(c => c.MemberId),
                _ => orderDir?.ToLower() == "desc" ? 
                    query.OrderByDescending(c => c.RegDtm) : 
                    query.OrderBy(c => c.RegDtm)
            };

            // 페이징
            var totalCount = await query.CountAsync();
            var skip = (page - 1) * size;
            var conventions = await query
                .Skip(skip)
                .Take(size)
                .ToListAsync();

            return Ok(new
            {
                Data = conventions,
                Page = page,
                Size = size,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / size),
                Filters = new 
                { 
                    memberId, 
                    isDeleted, 
                    fromDate, 
                    toDate, 
                    title,
                    orderBy,
                    orderDir
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get conventions");
            return BadRequest(new { Error = ex.Message });
        }
    }

    // 특정 컨벤션 조회
    [HttpGet("{id}")]
    public async Task<ActionResult<CorpConvention>> GetConvention(int id)
    {
        try
        {
            var convention = await _context.CorpConventions.FindAsync(id);
            if (convention == null)
                return NotFound(new { Message = $"Convention with ID {id} not found" });

            return Ok(convention);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get convention {ConventionId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    // 통계 정보
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        try
        {
            var totalCount = await _context.CorpConventions.CountAsync();
            var activeCount = await _context.CorpConventions.CountAsync(c => c.DeleteYn != "Y");
            var deletedCount = await _context.CorpConventions.CountAsync(c => c.DeleteYn == "Y");
            
            var thisYear = DateTime.Now.Year;
            var thisYearCount = await _context.CorpConventions
                .CountAsync(c => c.StartDate!.Value.Year == thisYear && c.DeleteYn != "Y");
            
            var upcomingCount = await _context.CorpConventions
                .CountAsync(c => c.StartDate > DateTime.Now && c.DeleteYn != "Y");
                
            var ongoingCount = await _context.CorpConventions
                .CountAsync(c => c.StartDate <= DateTime.Now && c.EndDate >= DateTime.Now && c.DeleteYn != "Y");

            // 월별 통계 (최근 12개월)
            var monthlyStats = new List<object>();
            for (int i = 11; i >= 0; i--)
            {
                var targetMonth = DateTime.Now.AddMonths(-i);
                var count = await _context.CorpConventions
                    .CountAsync(c => c.StartDate!.Value.Year == targetMonth.Year && 
                               c.StartDate.Value.Month == targetMonth.Month && 
                               c.DeleteYn != "Y");
                
                monthlyStats.Add(new
                {
                    Year = targetMonth.Year,
                    Month = targetMonth.Month,
                    MonthName = targetMonth.ToString("yyyy-MM"),
                    Count = count
                });
            }

            return Ok(new
            {
                Total = totalCount,
                Active = activeCount,
                Deleted = deletedCount,
                ThisYear = thisYearCount,
                Upcoming = upcomingCount,
                Ongoing = ongoingCount,
                MonthlyStats = monthlyStats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get convention stats");
            return BadRequest(new { Error = ex.Message });
        }
    }

    // 회원별 컨벤션 통계
    [HttpGet("member-stats")]
    public async Task<ActionResult<object>> GetMemberStats([FromQuery] int top = 10)
    {
        try
        {
            var memberStats = await _context.CorpConventions
                .Where(c => !string.IsNullOrEmpty(c.MemberId) && c.DeleteYn != "Y")
                .GroupBy(c => c.MemberId)
                .Select(g => new
                {
                    MemberId = g.Key,
                    TotalConventions = g.Count(),
                    LatestConvention = g.Max(c => c.StartDate),
                    EarliestConvention = g.Min(c => c.StartDate),
                    WithAudio = g.Count(c => c.AudioYn == "Y"),
                    WithPhoto = g.Count(c => c.PhotoYn == "Y"),
                    CustomConventions = g.Count(c => c.CustomYn == "Y")
                })
                .OrderByDescending(s => s.TotalConventions)
                .Take(top)
                .ToListAsync();

            return Ok(new
            {
                TopMembers = memberStats,
                TotalUniqueMembers = memberStats.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get member stats");
            return BadRequest(new { Error = ex.Message });
        }
    }

    // 검색
    [HttpGet("search")]
    public async Task<ActionResult<object>> SearchConventions(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { Error = "Keyword is required" });

            var query = _context.CorpConventions
                .Where(c => c.DeleteYn != "Y" &&
                           (c.Title!.Contains(keyword) ||
                            c.SubTitle!.Contains(keyword) ||
                            c.QuestionTitle!.Contains(keyword) ||
                            c.MemberId!.Contains(keyword) ||
                            c.Owner1Name!.Contains(keyword) ||
                            c.Owner2Name!.Contains(keyword)));

            var totalCount = await query.CountAsync();
            var skip = (page - 1) * size;
            var results = await query
                .OrderByDescending(c => c.RegDtm)
                .Skip(skip)
                .Take(size)
                .ToListAsync();

            return Ok(new
            {
                Keyword = keyword,
                Data = results,
                Page = page,
                Size = size,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / size)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search conventions with keyword: {Keyword}", keyword);
            return BadRequest(new { Error = ex.Message });
        }
    }
}
