using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;

using System.Security.Claims;
using LocalRAG.Entities;
using LocalRAG.DTOs.GuestModels;
using LocalRAG.DTOs.ActionModels;

namespace LocalRAG.Controllers.Guest;

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
                    g.CorpName != null && g.CorpName.Contains(search) ||
                    g.CorpPart != null && g.CorpPart.Contains(search));
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

    /// <summary>
    /// 여행 정보 제출 (PROFILE_OVERSEAS 액션)
    /// </summary>
    [HttpPut("my-travel-info")]
    public async Task<IActionResult> UpdateMyTravelInfo([FromBody] TravelInfoDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // 사용자의 Guest 정보 조회
            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guest == null)
                return NotFound(new { message = "참석자 정보를 찾을 수 없습니다." });

            // 1. Guest 테이블 업데이트
            guest.EnglishName = dto.EnglishName;
            guest.PassportNumber = dto.PassportNumber;
            guest.PassportExpiryDate = dto.PassportExpiryDate;
            guest.VisaDocumentAttachmentId = dto.VisaDocumentAttachmentId;

            // 2. PROFILE_OVERSEAS 액션 찾기
            var action = await _context.ConventionActions
                .FirstOrDefaultAsync(a => 
                    a.ConventionId == guest.ConventionId && 
                    a.ActionType == "PROFILE_OVERSEAS");

            if (action != null)
            {
                // 3. GuestActionStatus 찾기 또는 생성
                var status = await _context.GuestActionStatuses
                    .FirstOrDefaultAsync(s => 
                        s.GuestId == guest.Id && 
                        s.ConventionActionId == action.Id);

                if (status == null)
                {
                    status = new GuestActionStatus
                    {
                        GuestId = guest.Id,
                        ConventionActionId = action.Id,
                        IsComplete = true,
                        CompletedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.GuestActionStatuses.Add(status);
                }
                else
                {
                    status.IsComplete = true;
                    status.CompletedAt = DateTime.UtcNow;
                    status.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Travel info updated for Guest {GuestId}, Action completed",
                guest.Id);

            return Ok(new { message = "여행 정보가 저장되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Travel info update failed");
            return StatusCode(500, new { message = "여행 정보 저장 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 현재 로그인한 참석자의 체크리스트 조회 (GuestId 기반)
    /// </summary>
    [HttpGet("my-checklist")]
    public async Task<IActionResult> GetMyChecklist([FromQuery] int? conventionId = null)
    {
        try
        {
            // 1. 토큰에서 GuestId 추출 (JWT에 GuestId가 있어야 함)
            var guestIdClaim = User.FindFirst("GuestId")?.Value;
            if (string.IsNullOrEmpty(guestIdClaim))
            {
                return BadRequest(new { message = "참석자 정보를 찾을 수 없습니다." });
            }

            int guestId = int.Parse(guestIdClaim);

            // 2. Guest 정보 조회
            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Id == guestId);

            if (guest == null)
            {
                return NotFound(new { message = "참석자 정보를 찾을 수 없습니다." });
            }

            // 3. ConventionId 결정 (파라미터 또는 Guest의 ConventionId)
            int targetConventionId = conventionId ?? guest.ConventionId;

            // 4. 체크리스트 구축
            var checklist = await BuildChecklistStatusAsync(guestId, targetConventionId);

            return Ok(checklist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get checklist failed");
            return StatusCode(500, new { message = "체크리스트 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 체크리스트 상태 구축
    /// </summary>
    private async Task<object?> BuildChecklistStatusAsync(int guestId, int conventionId)
    {
        // 1. 해당 행사의 활성 액션 목록 조회
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId && a.IsActive)
            .OrderBy(a => a.OrderNum)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();

        if (actions.Count == 0)
            return null; // 액션이 없으면 null 반환

        // 2. 해당 참여자의 액션 상태 조회
        var statuses = await _context.GuestActionStatuses
            .Where(s => s.GuestId == guestId)
            .ToListAsync();

        var statusDict = statuses.ToDictionary(s => s.ConventionActionId, s => s);

        // 3. 체크리스트 아이템 구축
        var items = new List<object>();
        int completedCount = 0;

        foreach (var action in actions)
        {
            var status = statusDict.GetValueOrDefault(action.Id);
            bool isComplete = status?.IsComplete ?? false;

            if (isComplete)
                completedCount++;

            items.Add(new
            {
                actionId = action.Id,
                actionType = action.ActionType,
                title = action.Title,
                isComplete,
                deadline = action.Deadline,
                navigateTo = action.MapsTo,
                orderNum = action.OrderNum
            });
        }

        // 4. 가장 가까운 미완료 액션의 마감일 찾기
        DateTime? overallDeadline = actions
            .Where(a => {
                var status = statusDict.GetValueOrDefault(a.Id);
                return !(status?.IsComplete ?? false) && a.Deadline.HasValue;
            })
            .OrderBy(a => a.Deadline)
            .FirstOrDefault()?.Deadline;

        // 5. 체크리스트 DTO 반환
        int totalItems = actions.Count;
        int progressPercentage = totalItems > 0 ? completedCount * 100 / totalItems : 0;

        return new
        {
            totalItems,
            completedItems = completedCount,
            progressPercentage,
            overallDeadline,
            items
        };
    }
}
