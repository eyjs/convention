using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;

using System.Security.Claims;
using LocalRAG.Entities;
using LocalRAG.DTOs.UserModels; // Changed from GuestModels
using LocalRAG.DTOs.ActionModels;

namespace LocalRAG.Controllers.User; // Changed from Guest

[ApiController]
[Route("api/users")] // Changed from api/guest
[Authorize]
public class UserController : ControllerBase // Changed from GuestController
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(ConventionDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 내 일정 조회 (로그인한 사용자)
    /// </summary>
    [HttpGet("my-schedules")]
    public async Task<IActionResult> GetMySchedules([FromQuery] int conventionId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // 사용자의 정보 조회
            var user = await _context.Users
                .Include(u => u.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                        .ThenInclude(st => st!.ScheduleItems)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });

            // 해당 컨벤션에 할당된 모든 일정 항목 가져오기
            var schedules = user.GuestScheduleTemplates
                .Where(gst => gst.ScheduleTemplate != null && gst.ScheduleTemplate.ConventionId == conventionId)
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
            var query = _context.UserConventions
                .Where(uc => uc.ConventionId == conventionId)
                .Select(uc => uc.User);

            // 검색
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(u => 
                    u.Name.Contains(search) || 
                    u.CorpName != null && u.CorpName.Contains(search) ||
                    u.CorpPart != null && u.CorpPart.Contains(search));
            }

            var participants = await query
                .Include(u => u.GuestAttributes)
                .OrderBy(u => u.Name)
                .Select(u => new
                {
                    u.Id,
                    Name = u.Name,
                    u.CorpName,
                    u.CorpPart,
                    Phone = u.Phone,
                    Attributes = u.GuestAttributes.ToDictionary(
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
            var participant = await _context.Users
                .Include(u => u.GuestAttributes)
                .Include(u => u.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (participant == null)
                return NotFound(new { message = "참가자를 찾을 수 없습니다." });

            var result = new
            {
                participant.Id,
                Name = participant.Name,
                participant.CorpName,
                participant.CorpPart,
                Phone = participant.Phone,
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
        if (dto == null || dto.UserMappings == null || !dto.UserMappings.Any())
        {
            return BadRequest(new { message = "요청 데이터가 비어있습니다." });
        }

        _logger.LogInformation("일괄 속성 할당 시작: ConventionId={ConventionId}, Count={Count}명", 
            dto.ConventionId, dto.UserMappings.Count);

        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var result = new BulkAssignResult
                {
                    TotalProcessed = dto.UserMappings.Count
                };
                
                var userIds = dto.UserMappings.Select(m => m.UserId).ToList();
                
                var existingUsersInConvention = await _context.UserConventions
                    .Where(uc => uc.ConventionId == dto.ConventionId && userIds.Contains(uc.UserId))
                    .Select(uc => uc.UserId)
                    .ToListAsync();
                
                var invalidUserIds = userIds.Except(existingUsersInConvention).ToList();
                if (invalidUserIds.Any())
                {
                    result.Errors.Add($"컨벤션에 속하지 않거나 존재하지 않는 사용자 ID: {string.Join(", ", invalidUserIds)}");
                }
                
                var validUserIds = userIds.Except(invalidUserIds).ToList();

                var existingAttributes = await _context.GuestAttributes
                    .Where(ga => validUserIds.Contains(ga.UserId))
                    .ToListAsync();
                
                var newAttributeKeys = dto.UserMappings
                    .SelectMany(m => m.Attributes.Keys)
                    .Distinct()
                    .ToList();
                
                var attributesToRemove = existingAttributes
                    .Where(ea => newAttributeKeys.Contains(ea.AttributeKey))
                    .ToList();
                
                _context.GuestAttributes.RemoveRange(attributesToRemove);
                
                var newAttributes = new List<GuestAttribute>();
                
                foreach (var mapping in dto.UserMappings)
                {
                    if (!validUserIds.Contains(mapping.UserId))
                    {
                        result.FailCount++;
                        continue;
                    }
                    
                    foreach (var attr in mapping.Attributes)
                    {
                        if (string.IsNullOrWhiteSpace(attr.Value))
                            continue;
                        
                        newAttributes.Add(new GuestAttribute
                        {
                            UserId = mapping.UserId,
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
            var users = await _context.UserConventions
                .Where(uc => uc.ConventionId == conventionId)
                .Select(uc => uc.User)
                .Include(u => u.GuestAttributes)
                .OrderBy(u => u.Name)
                .ToListAsync();
            
            var result = users.Select(u => new UserWithAttributesDto
            {
                Id = u.Id,
                Name = u.Name,
                CorpName = u.CorpName,
                CorpPart = u.CorpPart,
                Phone = u.Phone,
                Email = u.Email,
                CurrentAttributes = u.GuestAttributes.ToDictionary(
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
    public async Task<IActionResult> UpdateMyTravelInfo([FromQuery] int conventionId, [FromBody] TravelInfoDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });

            // 1. User 테이블 업데이트
            user.EnglishName = dto.EnglishName;
            user.PassportNumber = dto.PassportNumber;
            user.PassportExpiryDate = dto.PassportExpiryDate;

            // 2. PROFILE_OVERSEAS 액션 찾기
            var action = await _context.ConventionActions
                .FirstOrDefaultAsync(a => 
                    a.ConventionId == conventionId && 
                    a.ActionType == "PROFILE_OVERSEAS");

            if (action != null)
            {
                // 3. GuestActionStatus 찾기 또는 생성
                var status = await _context.UserActionStatuses
                    .FirstOrDefaultAsync(s => 
                        s.UserId == user.Id && 
                        s.ConventionActionId == action.Id);

                if (status == null)
                {
                    status = new UserActionStatus
                    {
                        UserId = user.Id,
                        ConventionActionId = action.Id,
                        IsComplete = true,
                        CompletedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserActionStatuses.Add(status);
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
                "Travel info updated for User {UserId}, Action completed",
                user.Id);

            return Ok(new { message = "여행 정보가 저장되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Travel info update failed");
            return StatusCode(500, new { message = "여행 정보 저장 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 현재 로그인한 사용자의 체크리스트 조회
    /// </summary>
    [HttpGet("my-checklist")]
    public async Task<IActionResult> GetMyChecklist([FromQuery] int conventionId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "사용자 정보를 찾을 수 없습니다." });
            }

            var userExistsInConvention = await _context.UserConventions
                .AnyAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

            if (!userExistsInConvention)
            {
                return NotFound(new { message = "해당 행사의 참석자가 아닙니다." });
            }

            var checklist = await BuildChecklistStatusAsync(userId, conventionId);

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
    private async Task<object?> BuildChecklistStatusAsync(int userId, int conventionId)
    {
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId && a.IsActive)
            .OrderBy(a => a.OrderNum)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();

        if (actions.Count == 0)
            return null;

        var statuses = await _context.UserActionStatuses
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var statusDict = statuses.ToDictionary(s => s.ConventionActionId, s => s);

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

        DateTime? overallDeadline = actions
            .Where(a => {
                var status = statusDict.GetValueOrDefault(a.Id);
                return !(status?.IsComplete ?? false) && a.Deadline.HasValue;
            })
            .OrderBy(a => a.Deadline)
            .FirstOrDefault()?.Deadline;

        int totalItems = actions.Count;
        int progressPercentage = totalItems > 0 ? completedCount * 100 / totalItems : 0;

        return new
        {
            totalItems,
            completedItems = completedCount,
            progressPercentage,
            overallDeadline,
            items = items
        };
    }
}
