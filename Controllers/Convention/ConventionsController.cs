using LocalRAG.Data;
using LocalRAG.Models;
using ConventionModel = LocalRAG.Models.Convention;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/conventions")]
//[Authorize]
public class ConventionsController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ConventionsController> _logger;

    public ConventionsController(ConventionDbContext context, ILogger<ConventionsController> _logger)
    {
        _context = context;
        this._logger = _logger;
    }

    // GET: api/conventions
    [HttpGet]
    public async Task<IActionResult> GetConventions([FromQuery] bool includeDeleted = false)
    {
        var query = _context.Conventions.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(c => c.DeleteYn == "N");
        }

        var conventions = await query
            .OrderByDescending(c => c.RegDtm)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.ConventionType,
                c.StartDate,
                c.EndDate,
                c.BrandColor,
                c.DeleteYn,
                c.CompleteYn,
                c.RegDtm,
                GuestCount = c.Guests.Count,
                ScheduleCount = c.ScheduleTemplates.Count
            })
            .ToListAsync();

        return Ok(conventions);
    }

    [HttpGet("my-conventions")]
    public async Task<IActionResult> GetUserConventions()
    {
        var conventions = await _context.Conventions
            .Where(c => c.DeleteYn == "N" && c.CompleteYn == "N")
            .OrderByDescending(c => c.RegDtm)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.ConventionType,
                c.StartDate,
                c.EndDate,
                c.BrandColor,
                c.CompleteYn,
                GuestCount = c.Guests.Count,
                ScheduleCount = c.ScheduleTemplates.Count
            })
            .ToListAsync();

        return Ok(conventions);
    }


    // GET: api/conventions/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConvention(int id)
    {
        var convention = await _context.Conventions
            .Include(c => c.Guests)
            .Include(c => c.ScheduleTemplates)
            .Include(c => c.Features)
            .Include(c => c.Owners)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (convention == null)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        return Ok(new
        {
            convention.Id,
            convention.Title,
            convention.ConventionType,
            convention.RenderType,
            convention.StartDate,
            convention.EndDate,
            convention.BrandColor,
            convention.ThemePreset,
            convention.DeleteYn,
            convention.CompleteYn,
            convention.RegDtm,
            GuestCount = convention.Guests.Count,
            ScheduleCount = convention.ScheduleTemplates.Count,
            Features = convention.Features.Select(f => new { f.FeatureName, f.IsEnabled }),
            Owners = convention.Owners.Select(o => new { o.Name, o.Telephone })
        });
    }

    // POST: api/conventions
    [HttpPost]
    public async Task<IActionResult> CreateConvention([FromBody] CreateConventionRequest request)
    {
        var convention = new ConventionModel
        {
            MemberId = "admin", // TODO: 실제 사용자 ID
            Title = request.Title,
            ConventionType = request.ConventionType,
            RenderType = request.RenderType ?? "STANDARD",
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BrandColor = request.BrandColor ?? "#6366f1",
            ThemePreset = request.ThemePreset ?? "default",
            RegDtm = DateTime.Now,
            DeleteYn = "N",
            CompleteYn = "N"
        };

        _context.Conventions.Add(convention);
        await _context.SaveChangesAsync();

        // 기본 Features 추가
        var defaultFeatures = new[]
        {
            new Feature { ConventionId = convention.Id, FeatureName = "Schedule", IsEnabled = "Y" },
            new Feature { ConventionId = convention.Id, FeatureName = "Chat", IsEnabled = "Y" },
            new Feature { ConventionId = convention.Id, FeatureName = "Gallery", IsEnabled = "Y" },
            new Feature { ConventionId = convention.Id, FeatureName = "Board", IsEnabled = "Y" }
        };

        _context.Features.AddRange(defaultFeatures);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetConvention), new { id = convention.Id }, new
        {
            convention.Id,
            convention.Title,
            convention.ConventionType,
            convention.StartDate,
            convention.EndDate,
            message = "행사가 생성되었습니다."
        });
    }

    // PUT: api/conventions/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConvention(int id, [FromBody] UpdateConventionRequest request)
    {
        var convention = await _context.Conventions.FindAsync(id);
        if (convention == null)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        convention.Title = request.Title;
        convention.ConventionType = request.ConventionType;
        convention.RenderType = request.RenderType ?? convention.RenderType;
        convention.StartDate = request.StartDate;
        convention.EndDate = request.EndDate;
        convention.BrandColor = request.BrandColor ?? convention.BrandColor;
        convention.ThemePreset = request.ThemePreset ?? convention.ThemePreset;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            convention.Id,
            convention.Title,
            message = "행사 정보가 수정되었습니다."
        });
    }

    // DELETE: api/conventions/5 (Soft Delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteConvention(int id)
    {
        var convention = await _context.Conventions.FindAsync(id);
        if (convention == null)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        convention.DeleteYn = "Y";
        await _context.SaveChangesAsync();

        return Ok(new { message = "행사가 삭제되었습니다." });
    }

    // POST: api/conventions/5/complete
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteConvention(int id)
    {
        var convention = await _context.Conventions.FindAsync(id);
        if (convention == null)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        convention.CompleteYn = convention.CompleteYn == "Y" ? "N" : "Y";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            convention.Id,
            convention.CompleteYn,
            message = convention.CompleteYn == "Y" ? "행사가 종료되었습니다." : "행사가 재개되었습니다."
        });
    }

    // POST: api/conventions/5/restore
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreConvention(int id)
    {
        var convention = await _context.Conventions.FindAsync(id);
        if (convention == null)
        {
            return NotFound(new { message = "행사를 찾을 수 없습니다." });
        }

        convention.DeleteYn = "N";
        await _context.SaveChangesAsync();

        return Ok(new { message = "행사가 복원되었습니다." });
    }
}

public class CreateConventionRequest
{
    public string Title { get; set; } = string.Empty;
    public string ConventionType { get; set; } = "DOMESTIC";
    public string? RenderType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? BrandColor { get; set; }
    public string? ThemePreset { get; set; }
}

public class UpdateConventionRequest
{
    public string Title { get; set; } = string.Empty;
    public string ConventionType { get; set; } = "DOMESTIC";
    public string? RenderType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? BrandColor { get; set; }
    public string? ThemePreset { get; set; }
}
