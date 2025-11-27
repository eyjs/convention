using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;

using System.Security.Claims;
using LocalRAG.Entities;
using LocalRAG.DTOs.UserModels; // Changed from GuestModels
using LocalRAG.DTOs.ActionModels;
using LocalRAG.Interfaces;

namespace LocalRAG.Controllers.User; // Changed from Guest

[ApiController]
[Route("api/users")] // Changed from api/guest
[Authorize]
public class UserController : ControllerBase // Changed from GuestController
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<UserController> _logger;
    private readonly IFileUploadService _fileUploadService;

    public UserController(ConventionDbContext context, ILogger<UserController> logger, IFileUploadService fileUploadService)
    {
        _context = context;
        _logger = logger;
        _fileUploadService = fileUploadService;
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
                    ProfileImageUrl = u.ProfileImageUrl,
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

            // 2. 여행 정보 액션 찾기 (MapsTo 경로로 식별)
            var action = await _context.ConventionActions
                .FirstOrDefaultAsync(a =>
                    a.ConventionId == conventionId &&
                    a.MapsTo == "/feature/travel-info");

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

    /// <summary>
    /// 내 정보 조회
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });
        }

        var profileDto = new UserProfileDto
        {
            Id = user.Id,
            LoginId = user.LoginId,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PassportNumber = user.PassportNumber,
            PassportExpiryDate = user.PassportExpiryDate?.ToString("yyyy-MM-dd"),
            Affiliation = user.Affiliation,
            CorpName = user.CorpName,
            CorpPart = user.CorpPart,
            ProfileImageUrl = user.ProfileImageUrl
        };

        return Ok(profileDto);
    }

    /// <summary>
    /// 내 정보 수정
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });
        }

        // 업데이트
        user.Phone = dto.Phone;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PassportNumber = dto.PassportNumber;
        user.Affiliation = dto.Affiliation;

        // PassportExpiryDate 파싱 및 유효성 검사
        if (!string.IsNullOrEmpty(dto.PassportExpiryDate))
        {
            if (DateOnly.TryParseExact(dto.PassportExpiryDate, "yyyy-MM-dd", out var parsedDate))
            {
                user.PassportExpiryDate = parsedDate;
            }
            else
            {
                return BadRequest(new { message = "여권 만료일 형식이 올바르지 않습니다. (yyyy-MM-dd)" });
            }
        }
        else
        {
            user.PassportExpiryDate = null;
        }
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "정보가 수정되었습니다." });
    }

    /// <summary>
    /// 내 정보 단일 필드 수정
    /// </summary>
    [HttpPatch("profile/field")]
    public async Task<IActionResult> UpdateProfileField([FromBody] UpdateProfileFieldRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });
        }

        switch (request.FieldName)
        {
            case "phone":
                user.Phone = request.FieldValue;
                break;
            case "email":
                // 이메일 유효성 검사 (간단한 예시)
                if (!string.IsNullOrEmpty(request.FieldValue) && !request.FieldValue.Contains("@"))
                {
                    return BadRequest(new { message = "유효하지 않은 이메일 형식입니다." });
                }
                user.Email = request.FieldValue;
                break;
            case "affiliation":
                user.Affiliation = request.FieldValue;
                break;
            case "firstName":
                user.FirstName = request.FieldValue;
                break;
            case "lastName":
                user.LastName = request.FieldValue;
                break;
            case "passportNumber":
                user.PassportNumber = request.FieldValue;
                break;
            case "passportExpiryDate":
                if (!string.IsNullOrEmpty(request.FieldValue))
                {
                    if (DateOnly.TryParseExact(request.FieldValue, "yyyy-MM-dd", out var parsedDate))
                    {
                        user.PassportExpiryDate = parsedDate;
                    }
                    else
                    {
                        return BadRequest(new { message = "여권 만료일 형식이 올바르지 않습니다. (yyyy-MM-dd)" });
                    }
                }
                else
                {
                    user.PassportExpiryDate = null;
                }
                break;
            default:
                return BadRequest(new { message = $"알 수 없는 필드명입니다: {request.FieldName}" });
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "정보가 수정되었습니다." });
    }

    /// <summary>
    /// 비밀번호 변경
    /// </summary>
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });
        }

        // 현재 비밀번호 확인
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "현재 비밀번호가 올바르지 않습니다." });
        }

        // 새 비밀번호로 업데이트
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "비밀번호가 변경되었습니다." });
    }

    /// <summary>
    /// 프로필 사진 업로드
    /// </summary>
    [HttpPost("profile/photo")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "파일이 비어있습니다." });
        }

        try
        {
            // 기존 프로필 사진이 있다면 삭제 (선택 사항, 여기서는 새 파일로 대체)
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                // 실제 파일 시스템에서 삭제하는 로직이 필요할 수 있으나,
                // 현재 _fileUploadService.DeleteFileAsync는 상대 경로를 받으므로
                // ProfileImageUrl에서 상대 경로를 추출해야 함.
                // 여기서는 간단히 URL만 업데이트하는 것으로 처리.
                // TODO: 기존 파일 삭제 로직 추가 고려
            }

            // 새 프로필 사진 업로드
            var uploadResult = await _fileUploadService.UploadImageAsync(file, "profile_photos");

            if (string.IsNullOrEmpty(uploadResult.Url))
            {
                return StatusCode(500, new { message = "프로필 사진 업로드에 실패했습니다." });
            }

            user.ProfileImageUrl = uploadResult.Url;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "프로필 사진이 성공적으로 업로드되었습니다.", profileImageUrl = user.ProfileImageUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "프로필 사진 업로드 실패");
            return StatusCode(500, new { message = "프로필 사진 업로드 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 여권 사진 업로드
    /// </summary>
    [HttpPost("profile/passport-image")]
    public async Task<IActionResult> UploadPassportImage(IFormFile file)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "사용자 정보를 찾을 수 없습니다." });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "파일이 비어있습니다." });
        }

        try
        {
            var uploadResult = await _fileUploadService.UploadImageAsync(file, "passport_images");

            if (string.IsNullOrEmpty(uploadResult.Url))
            {
                return StatusCode(500, new { message = "여권 사진 업로드에 실패했습니다." });
            }

            user.PassportImageUrl = uploadResult.Url;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "여권 사진이 성공적으로 업로드되었습니다.", passportImageUrl = user.PassportImageUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "여권 사진 업로드 실패");
            return StatusCode(500, new { message = "여권 사진 업로드 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 내가 참여 중인 행사 목록 조회
    /// </summary>
    [HttpGet("conventions")]
    public async Task<IActionResult> GetMyConventions()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var conventions = await _context.UserConventions
                .Where(uc => uc.UserId == userId)
                .Select(uc => new
                {
                    id = uc.Convention.Id,
                    title = uc.Convention.Title,
                    startDate = uc.Convention.StartDate,
                    endDate = uc.Convention.EndDate,
                    conventionType = uc.Convention.ConventionType,
                    conventionImg = uc.Convention.ConventionImg
                })
                .OrderByDescending(c => c.startDate)
                .ToListAsync();

            return Ok(conventions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "행사 목록 조회 실패");
            return StatusCode(500, new { message = "행사 목록 조회 중 오류가 발생했습니다." });
        }
    }
}
