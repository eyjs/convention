using LocalRAG.Models.Convention;
using LocalRAG.Repositories;
using LocalRAG.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers;

/// <summary>
/// Convention API 컨트롤러 예시
/// 
/// Repository 패턴을 컨트롤러에서 사용하는 방법을 보여줍니다.
/// 실제로는 Service Layer를 통해 사용하는 것이 권장됩니다.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConventionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ConventionService _conventionService;

    /// <summary>
    /// 생성자 주입
    /// 
    /// 두 가지 방식 모두 사용 가능:
    /// 1. IUnitOfWork 직접 주입 (간단한 CRUD)
    /// 2. Service 주입 (복잡한 비즈니스 로직)
    /// </summary>
    public ConventionsController(
        IUnitOfWork unitOfWork,
        ConventionService conventionService)
    {
        _unitOfWork = unitOfWork;
        _conventionService = conventionService;
    }

    // ============================================================
    // GET 요청 예시
    // ============================================================

    /// <summary>
    /// 모든 활성 행사를 조회합니다.
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// GET /api/conventions
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Convention>>> GetActiveConventions()
    {
        try
        {
            // Service Layer 사용
            var conventions = await _conventionService.GetActiveConventionsAsync();
            return Ok(conventions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "서버 오류", error = ex.Message });
        }
    }

    /// <summary>
    /// 특정 행사의 상세 정보를 조회합니다.
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// GET /api/conventions/1
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<ActionResult<Convention>> GetConventionById(int id)
    {
        try
        {
            // Repository 직접 사용 (간단한 조회)
            var convention = await _unitOfWork.Conventions.GetConventionWithDetailsAsync(id);

            if (convention == null)
            {
                return NotFound(new { message = $"행사 ID {id}를 찾을 수 없습니다." });
            }

            return Ok(convention);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "서버 오류", error = ex.Message });
        }
    }

    /// <summary>
    /// 페이징된 행사 목록을 조회합니다.
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// GET /api/conventions/paged?pageNumber=1&amp;pageSize=10&amp;conventionType=DOMESTIC
    /// </remarks>
    [HttpGet("paged")]
    public async Task<ActionResult> GetConventionsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? conventionType = null)
    {
        try
        {
            var (items, totalCount, totalPages) = await _conventionService.GetConventionsPagedAsync(
                pageNumber,
                pageSize,
                conventionType);

            return Ok(new
            {
                items,
                pagination = new
                {
                    currentPage = pageNumber,
                    pageSize,
                    totalCount,
                    totalPages
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "서버 오류", error = ex.Message });
        }
    }

    // ============================================================
    // POST 요청 예시
    // ============================================================

    /// <summary>
    /// 새로운 행사를 생성합니다.
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// POST /api/conventions
    /// {
    ///   "title": "2025 해외 워크샵",
    ///   "conventionType": "OVERSEAS",
    ///   "renderType": "STANDARD",
    ///   "startDate": "2025-03-15",
    ///   "endDate": "2025-03-18"
    /// }
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<Convention>> CreateConvention(
        [FromBody] ConventionCreateDto dto)
    {
        try
        {
            // DTO에서 엔티티로 변환
            var convention = new Convention
            {
                Title = dto.Title,
                ConventionType = dto.ConventionType,
                RenderType = dto.RenderType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ConventionImg = dto.ConventionImg
            };

            // Service를 통한 생성
            var created = await _conventionService.CreateConventionAsync(convention);

            // 201 Created 응답 (Location 헤더 포함)
            return CreatedAtAction(
                nameof(GetConventionById),
                new { id = created.Id },
                created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "생성 실패", error = ex.Message });
        }
    }

    /// <summary>
    /// 행사와 담당자를 함께 생성합니다.
    /// </summary>
    [HttpPost("with-owners")]
    public async Task<ActionResult<Convention>> CreateConventionWithOwners(
        [FromBody] ConventionWithOwnersDto dto)
    {
        try
        {
            var convention = new Convention
            {
                Title = dto.Title,
                ConventionType = dto.ConventionType,
                RenderType = dto.RenderType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            var owners = dto.Owners.Select(o => new Owner
            {
                Name = o.Name,
                Telephone = o.Telephone
            }).ToList();

            var created = await _conventionService.CreateConventionWithOwnersAsync(
                convention,
                owners);

            return CreatedAtAction(
                nameof(GetConventionById),
                new { id = created.Id },
                created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "생성 실패", error = ex.Message });
        }
    }

    // ============================================================
    // PUT 요청 예시
    // ============================================================

    /// <summary>
    /// 행사 정보를 수정합니다.
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// PUT /api/conventions/1
    /// {
    ///   "title": "수정된 행사명",
    ///   "conventionType": "DOMESTIC",
    ///   "startDate": "2025-04-01"
    /// }
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateConvention(
        int id,
        [FromBody] ConventionUpdateDto dto)
    {
        try
        {
            var updatedData = new Convention
            {
                Title = dto.Title,
                ConventionType = dto.ConventionType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ConventionImg = dto.ConventionImg
            };

            var success = await _conventionService.UpdateConventionAsync(id, updatedData);

            if (!success)
            {
                return NotFound(new { message = $"행사 ID {id}를 찾을 수 없습니다." });
            }

            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "수정 실패", error = ex.Message });
        }
    }

    // ============================================================
    // DELETE 요청 예시
    // ============================================================

    /// <summary>
    /// 행사를 소프트 삭제합니다.
    /// </summary>
    /// <remarks>
    /// 예시 요청:
    /// DELETE /api/conventions/1
    /// </remarks>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteConvention(int id)
    {
        try
        {
            var success = await _conventionService.SoftDeleteConventionAsync(id);

            if (!success)
            {
                return NotFound(new { message = $"행사 ID {id}를 찾을 수 없습니다." });
            }

            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "삭제 실패", error = ex.Message });
        }
    }

    // ============================================================
    // 통계 및 검색 예시
    // ============================================================

    /// <summary>
    /// 행사 통계를 조회합니다.
    /// </summary>
    [HttpGet("{id}/statistics")]
    public async Task<ActionResult<ConventionStatistics>> GetStatistics(int id)
    {
        try
        {
            var stats = await _conventionService.GetConventionStatisticsAsync(id);
            return Ok(stats);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "서버 오류", error = ex.Message });
        }
    }

    /// <summary>
    /// 참석자를 검색합니다.
    /// </summary>
    [HttpGet("{conventionId}/guests/search")]
    public async Task<ActionResult<IEnumerable<Guest>>> SearchGuests(
        int conventionId,
        [FromQuery] string keyword)
    {
        try
        {
            var guests = await _conventionService.SearchGuestsAsync(keyword, conventionId);
            return Ok(guests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "검색 실패", error = ex.Message });
        }
    }
}

// ============================================================
// DTO 클래스들
// ============================================================

/// <summary>
/// 행사 생성 DTO
/// </summary>
public class ConventionCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string ConventionType { get; set; } = string.Empty;
    public string RenderType { get; set; } = "STANDARD";
    public string? ConventionImg { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// 행사 수정 DTO
/// </summary>
public class ConventionUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string ConventionType { get; set; } = string.Empty;
    public string? ConventionImg { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// 담당자 포함 행사 생성 DTO
/// </summary>
public class ConventionWithOwnersDto
{
    public string Title { get; set; } = string.Empty;
    public string ConventionType { get; set; } = string.Empty;
    public string RenderType { get; set; } = "STANDARD";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<OwnerDto> Owners { get; set; } = new();
}

public class OwnerDto
{
    public string Name { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
}
