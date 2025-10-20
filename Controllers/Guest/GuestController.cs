using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Models;
using LocalRAG.Models.DTOs;
using System.Security.Claims;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/guest")]
[Authorize]
public class GuestController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<GuestController> _logger;

    public GuestController(ConventionDbContext context, ILogger<GuestController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 내 일정 조회 (로그인한 사용자)
    /// </summary>
    [HttpGet("my-schedules")]
    public async Task<IActionResult> GetMySchedules()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // 사용자의 Guest 정보 조회
            var guest = await _context.Guests
                .Include(g => g.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                        .ThenInclude(st => st!.ScheduleItems)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guest == null)
                return NotFound(new { message = "참석자 정보를 찾을 수 없습니다." });

            // 할당된 모든 일정 항목 가져오기
            var schedules = guest.GuestScheduleTemplates
                .Where(gst => gst.ScheduleTemplate != null)
                .SelectMany(gst => gst.ScheduleTemplate!.ScheduleItems)
                .OrderBy(si => si.ScheduleDate)
                .ThenBy(si => si.StartTime)
                .Select(si => new
                {
                    si.Id,
                    ScheduleDate = si.ScheduleDate.ToString("yyyy-MM-dd"),
                    si.StartTime,
                    si.Title,
                    si.Location,
                    si.Content
                })
                .ToList();

            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "일정 조회 실패");
            return StatusCode(500, new { message = "일정을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 참가자 목록 조회
    /// </summary>
    [HttpGet("participants")]
    public async Task<IActionResult> GetParticipants([FromQuery] int conventionId, [FromQuery] string? search = null)
    {
        try
        {
            var query = _context.Guests
                .Where(g => g.ConventionId == conventionId)
                .Include(g => g.GuestAttributes)
                .AsQueryable();

            // 검색
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(g => 
                    g.GuestName.Contains(search) || 
                    (g.CorpName != null && g.CorpName.Contains(search)) ||
                    (g.CorpPart != null && g.CorpPart.Contains(search)));
            }

            var participants = await query
                .OrderBy(g => g.GuestName)
                .Select(g => new
                {
                    g.Id,
                    g.GuestName,
                    g.CorpName,
                    g.CorpPart,
                    g.Telephone,
                    Attributes = g.GuestAttributes.ToDictionary(
                        ga => ga.AttributeKey,
                        ga => ga.AttributeValue
                    )
                })
                .ToListAsync();

            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참가자 목록 조회 실패");
            return StatusCode(500, new { message = "참가자 목록을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 참가자 상세 조회
    /// </summary>
    [HttpGet("participants/{id}")]
    public async Task<IActionResult> GetParticipant(int id)
    {
        try
        {
            var participant = await _context.Guests
                .Include(g => g.GuestAttributes)
                .Include(g => g.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (participant == null)
                return NotFound(new { message = "참가자를 찾을 수 없습니다." });

            var result = new
            {
                participant.Id,
                participant.GuestName,
                participant.CorpName,
                participant.CorpPart,
                participant.Telephone,
                participant.Email,
                Attributes = participant.GuestAttributes.ToDictionary(
                    ga => ga.AttributeKey,
                    ga => ga.AttributeValue
                ),
                ScheduleTemplates = participant.GuestScheduleTemplates
                    .Where(gst => gst.ScheduleTemplate != null)
                    .Select(gst => new
                    {
                        gst.ScheduleTemplate!.Id,
                        gst.ScheduleTemplate.CourseName,
                        gst.ScheduleTemplate.Description
                    })
                    .ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참가자 상세 조회 실패");
            return StatusCode(500, new { message = "참가자 정보를 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 참석자 다중 속성 일괄 할당
    /// </summary>
    [HttpPost("bulk-assign-attributes")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkAssignAttributes([FromBody] BulkAssignAttributesDto dto)
    {
        // DTO 검증
        if (dto == null)
        {
            _logger.LogWarning("비어있는 DTO");
            return BadRequest(new { message = "요청 데이터가 비어있습니다." });
        }

        if (dto.GuestMappings == null || !dto.GuestMappings.Any())
        {
            _logger.LogWarning("참석자 매핑이 비어있음");
            return BadRequest(new { message = "참석자 데이터가 비어있습니다." });
        }

        _logger.LogInformation("일괄 속성 할당 시작: ConventionId={ConventionId}, Count={Count}명", 
            dto.ConventionId, dto.GuestMappings.Count);

        // ExecutionStrategy를 사용한 트랜잭션 처리
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var result = new BulkAssignResult
                {
                    TotalProcessed = dto.GuestMappings.Count
                };
                
                // 모든 참석자 ID 수집
                var guestIds = dto.GuestMappings.Select(m => m.GuestId).ToList();
                
                // 참석자 검증
                var existingGuests = await _context.Guests
                    .Where(g => guestIds.Contains(g.Id) && g.ConventionId == dto.ConventionId)
                    .Select(g => g.Id)
                    .ToListAsync();
                
                var invalidGuestIds = guestIds.Except(existingGuests).ToList();
                if (invalidGuestIds.Any())
                {
                    result.Errors.Add($"존재하지 않는 참석자 ID: {string.Join(", ", invalidGuestIds)}");
                }
                
                // 기존 속성 삭제 (업데이트를 위해)
                var existingAttributes = await _context.GuestAttributes
                    .Where(ga => guestIds.Contains(ga.GuestId))
                    .ToListAsync();
                
                // 새로 설정할 속성 키만 가져오기
                var newAttributeKeys = dto.GuestMappings
                    .SelectMany(m => m.Attributes.Keys)
                    .Distinct()
                    .ToList();
                
                // 기존 속성 중 새로 설정할 키와 겹치는 것만 삭제
                var attributesToRemove = existingAttributes
                    .Where(ea => newAttributeKeys.Contains(ea.AttributeKey))
                    .ToList();
                
                _context.GuestAttributes.RemoveRange(attributesToRemove);
                
                // 새 속성 추가
                var newAttributes = new List<GuestAttribute>();
                
                foreach (var mapping in dto.GuestMappings)
                {
                    // 유효한 참석자만 처리
                    if (!existingGuests.Contains(mapping.GuestId))
                    {
                        result.FailCount++;
                        continue;
                    }
                    
                    foreach (var attr in mapping.Attributes)
                    {
                        // 빈 값은 건너뛰기
                        if (string.IsNullOrWhiteSpace(attr.Value))
                            continue;
                        
                        newAttributes.Add(new GuestAttribute
                        {
                            GuestId = mapping.GuestId,
                            AttributeKey = attr.Key,
                            AttributeValue = attr.Value
                        });
                    }
                    
                    result.SuccessCount++;
                }
                
                await _context.GuestAttributes.AddRangeAsync(newAttributes);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                result.Success = true;
                result.Message = $"{result.SuccessCount}명의 참석자에게 속성이 성공적으로 할당되었습니다.";
                
                _logger.LogInformation("일괄 속성 할당 완료: 성공 {Success}명, 실패 {Fail}명", 
                    result.SuccessCount, result.FailCount);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "일괄 속성 할당 실패");
                
                return StatusCode(500, new BulkAssignResult
                {
                    Success = false,
                    Message = "속성 할당 중 오류가 발생했습니다.",
                    Errors = new List<string> { ex.Message }
                });
            }
        });
    }

    /// <summary>
    /// 속성 템플릿으로 참석자 목록 조회 (속성 포함)
    /// </summary>
    [HttpGet("participants-with-attributes")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetParticipantsWithAttributes([FromQuery] int conventionId)
    {
        try
        {
            var guests = await _context.Guests
                .Where(g => g.ConventionId == conventionId)
                .Include(g => g.GuestAttributes)
                .OrderBy(g => g.GuestName)
                .ToListAsync();
            
            var result = guests.Select(g => new GuestWithAttributesDto
            {
                Id = g.Id,
                GuestName = g.GuestName,
                CorpName = g.CorpName,
                CorpPart = g.CorpPart,
                Telephone = g.Telephone,
                Email = g.Email,
                CurrentAttributes = g.GuestAttributes.ToDictionary(
                    ga => ga.AttributeKey,
                    ga => ga.AttributeValue
                )
            }).ToList();
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참석자 목록 조회 실패");
            return StatusCode(500, new { message = "참석자 목록을 불러오는데 실패했습니다." });
        }
    }
}
