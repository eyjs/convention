using LocalRAG.DTOs.ActionModels;
using LocalRAG.DTOs.UserModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.UserProfile;

/// <summary>
/// 사용자 프로필 및 참가자 관련 비즈니스 로직 구현
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChecklistService _checklistService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUnitOfWork unitOfWork,
        IChecklistService checklistService,
        IFileUploadService fileUploadService,
        ILogger<UserProfileService> logger)
    {
        _unitOfWork = unitOfWork;
        _checklistService = checklistService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<object> GetMySchedulesAsync(int userId, int conventionId)
    {
        var user = await _unitOfWork.Users.Query
            .Include(u => u.GuestScheduleTemplates)
                .ThenInclude(gst => gst.ScheduleTemplate)
                    .ThenInclude(st => st!.ScheduleItems)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("사용자 정보를 찾을 수 없습니다.");

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

        return schedules;
    }

    public async Task<object> GetParticipantsAsync(int conventionId, string? search)
    {
        var query = _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Select(uc => uc.User);

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

        return participants;
    }

    public async Task<object?> GetParticipantDetailAsync(int id)
    {
        var participant = await _unitOfWork.Users.Query
            .Include(u => u.GuestAttributes)
            .Include(u => u.GuestScheduleTemplates)
                .ThenInclude(gst => gst.ScheduleTemplate)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (participant == null)
            return null;

        return new
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
    }

    public async Task<BulkAssignResult> BulkAssignAttributesAsync(BulkAssignAttributesDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = new BulkAssignResult
            {
                TotalProcessed = dto.UserMappings.Count
            };

            var userIds = dto.UserMappings.Select(m => m.UserId).ToList();

            var existingUsersInConvention = await _unitOfWork.UserConventions.Query
                .Where(uc => uc.ConventionId == dto.ConventionId && userIds.Contains(uc.UserId))
                .Select(uc => uc.UserId)
                .ToListAsync();

            var invalidUserIds = userIds.Except(existingUsersInConvention).ToList();
            if (invalidUserIds.Any())
            {
                result.Errors.Add($"컨벤션에 속하지 않거나 존재하지 않는 사용자 ID: {string.Join(", ", invalidUserIds)}");
            }

            var validUserIds = userIds.Except(invalidUserIds).ToList();

            var existingAttributes = await _unitOfWork.GuestAttributes.Query
                .Where(ga => validUserIds.Contains(ga.UserId))
                .ToListAsync();

            var newAttributeKeys = dto.UserMappings
                .SelectMany(m => m.Attributes.Keys)
                .Distinct()
                .ToList();

            var attributesToRemove = existingAttributes
                .Where(ea => newAttributeKeys.Contains(ea.AttributeKey))
                .ToList();

            _unitOfWork.GuestAttributes.RemoveRange(attributesToRemove);

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

            await _unitOfWork.GuestAttributes.AddRangeAsync(newAttributes);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            result.Success = true;
            result.Message = $"{result.SuccessCount}명의 참석자에게 속성이 성공적으로 할당되었습니다.";

            _logger.LogInformation("일괄 속성 할당 완료: 성공 {Success}명, 실패 {Fail}명",
                result.SuccessCount, result.FailCount);

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "일괄 속성 할당 실패");

            return new BulkAssignResult
            {
                Success = false,
                Message = "속성 할당 중 오류가 발생했습니다.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<List<UserWithAttributesDto>> GetParticipantsWithAttributesAsync(int conventionId)
    {
        var users = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Select(uc => uc.User)
            .Include(u => u.GuestAttributes)
            .OrderBy(u => u.Name)
            .ToListAsync();

        return users.Select(u => new UserWithAttributesDto
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
    }

    public async Task<(bool Success, string Message)> SubmitTravelInfoAsync(int userId, int conventionId, TravelInfoDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
            return (false, "사용자 정보를 찾을 수 없습니다.");

        user.EnglishName = dto.EnglishName;
        user.PassportNumber = dto.PassportNumber;
        user.PassportExpiryDate = dto.PassportExpiryDate;

        var action = await _unitOfWork.ConventionActions.Query
            .FirstOrDefaultAsync(a =>
                a.ConventionId == conventionId &&
                a.MapsTo == "/feature/travel-info");

        if (action != null)
        {
            var status = await _unitOfWork.UserActionStatuses.Query
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
                await _unitOfWork.UserActionStatuses.AddAsync(status);
            }
            else
            {
                status.IsComplete = true;
                status.CompletedAt = DateTime.UtcNow;
                status.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Travel info updated for User {UserId}, Action completed",
            user.Id);

        return (true, "여행 정보가 저장되었습니다.");
    }

    public async Task<(bool Exists, object? Checklist)> GetMyChecklistAsync(int userId, int conventionId)
    {
        var userExistsInConvention = await _unitOfWork.UserConventions
            .ExistsAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

        if (!userExistsInConvention)
            return (false, null);

        var checklist = await _checklistService.BuildChecklistStatusAsync(userId, conventionId);
        return (true, checklist);
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _unitOfWork.Users.Query
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        return new UserProfileDto
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
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(int userId, UpdateUserProfileDto dto)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);

        if (user == null)
            return (false, "사용자 정보를 찾을 수 없습니다.");

        user.Phone = dto.Phone;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Affiliation = dto.Affiliation;

        // 여권 관련 필드 변경 시 승인 초기화
        var passportChanged = user.PassportNumber != dto.PassportNumber;
        user.PassportNumber = dto.PassportNumber;

        if (!string.IsNullOrEmpty(dto.PassportExpiryDate))
        {
            if (DateOnly.TryParseExact(dto.PassportExpiryDate, "yyyy-MM-dd", out var parsedDate))
            {
                if (user.PassportExpiryDate != parsedDate) passportChanged = true;
                user.PassportExpiryDate = parsedDate;
            }
            else
            {
                return (false, "여권 만료일 형식이 올바르지 않습니다. (yyyy-MM-dd)");
            }
        }
        else
        {
            if (user.PassportExpiryDate != null) passportChanged = true;
            user.PassportExpiryDate = null;
        }

        if (passportChanged && user.PassportVerified)
        {
            user.PassportVerified = false;
            user.PassportVerifiedAt = null;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateProfileFieldAsync(int userId, UpdateProfileFieldRequest request)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);

        if (user == null)
            return (false, "사용자 정보를 찾을 수 없습니다.");

        switch (request.FieldName)
        {
            case "phone":
                user.Phone = request.FieldValue;
                break;
            case "email":
                if (!string.IsNullOrEmpty(request.FieldValue) && !request.FieldValue.Contains("@"))
                    return (false, "유효하지 않은 이메일 형식입니다.");
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
                user.PassportVerified = false;
                user.PassportVerifiedAt = null;
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
                        return (false, "여권 만료일 형식이 올바르지 않습니다. (yyyy-MM-dd)");
                    }
                }
                else
                {
                    user.PassportExpiryDate = null;
                }
                user.PassportVerified = false;
                user.PassportVerifiedAt = null;
                break;
            default:
                return (false, $"알 수 없는 필드명입니다: {request.FieldName}");
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);

        if (user == null)
            return (false, "사용자 정보를 찾을 수 없습니다.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return (false, "현재 비밀번호가 올바르지 않습니다.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage, string? Url)> UploadProfilePhotoAsync(int userId, IFormFile file)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);

        if (user == null)
            return (false, "사용자 정보를 찾을 수 없습니다.", null);

        var uploadResult = await _fileUploadService.UploadImageAsync(file, "profile_photos");

        if (string.IsNullOrEmpty(uploadResult.Url))
            return (false, "프로필 사진 업로드에 실패했습니다.", null);

        user.ProfileImageUrl = uploadResult.Url;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, null, user.ProfileImageUrl);
    }

    public async Task<(bool Success, string? ErrorMessage, string? Url)> UploadPassportImageAsync(int userId, IFormFile file)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);

        if (user == null)
            return (false, "사용자 정보를 찾을 수 없습니다.", null);

        var uploadResult = await _fileUploadService.UploadImageAsync(file, "passport_images");

        if (string.IsNullOrEmpty(uploadResult.Url))
            return (false, "여권 사진 업로드에 실패했습니다.", null);

        user.PassportImageUrl = uploadResult.Url;
        user.PassportVerified = false;
        user.PassportVerifiedAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, null, user.PassportImageUrl);
    }

    public async Task<object> GetUserConventionsAsync(int userId)
    {
        var conventions = await _unitOfWork.UserConventions.Query
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

        return conventions;
    }

    public async Task<object?> GetMyConventionInfoAsync(int userId, int conventionId)
    {
        var user = await _unitOfWork.Users.Query
            .Include(u => u.GuestAttributes)
            .Include(u => u.GuestScheduleTemplates)
                .ThenInclude(gst => gst.ScheduleTemplate)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        // UserConvention 정보
        var uc = await _unitOfWork.UserConventions.GetByUserAndConventionAsync(userId, conventionId);

        // 동반자 목록
        var companions = await _unitOfWork.CompanionRelations.Query
            .Where(cr => cr.UserId == userId && cr.ConventionId == conventionId)
            .Include(cr => cr.CompanionUser)
            .Select(cr => new
            {
                id = cr.Id,
                name = cr.CompanionUser.Name,
                relationType = cr.RelationType,
                passport = new
                {
                    hasNumber = !string.IsNullOrEmpty(cr.CompanionUser.PassportNumber),
                    hasImage = !string.IsNullOrEmpty(cr.CompanionUser.PassportImageUrl),
                    verified = cr.CompanionUser.PassportVerified
                }
            })
            .ToListAsync();

        // 옵션투어
        var optionTours = await _unitOfWork.UserOptionTours.Query
            .Where(uot => uot.UserId == userId && uot.ConventionId == conventionId)
            .Include(uot => uot.OptionTour)
            .Select(uot => new
            {
                id = uot.OptionTour!.Id,
                name = uot.OptionTour.Name,
                date = uot.OptionTour.Date.ToString("yyyy-MM-dd"),
                startTime = uot.OptionTour.StartTime
            })
            .ToListAsync();

        // 배정된 일정 코스
        var scheduleCourses = user.GuestScheduleTemplates
            .Where(gst => gst.ScheduleTemplate != null && gst.ScheduleTemplate.ConventionId == conventionId)
            .Select(gst => new
            {
                id = gst.ScheduleTemplate!.Id,
                courseName = gst.ScheduleTemplate.CourseName
            })
            .ToList();

        // 게스트 속성 (조, 호차 등)
        var attributes = user.GuestAttributes
            .Select(ga => new
            {
                key = ga.AttributeKey,
                value = ga.AttributeValue
            })
            .ToList();

        // 설문조사 상태
        var surveys = await _unitOfWork.Surveys.Query
            .Where(s => s.ConventionId == conventionId && s.IsActive)
            .Select(s => new
            {
                id = s.Id,
                title = s.Title,
                completed = s.Responses.Any(r => r.UserId == userId)
            })
            .ToListAsync();

        return new
        {
            profile = new
            {
                name = user.Name,
                phone = user.Phone,
                affiliation = user.Affiliation,
                corpName = user.CorpName,
                corpPart = user.CorpPart,
                profileImageUrl = user.ProfileImageUrl,
                passportNumber = user.PassportNumber,
                passportFirstName = user.FirstName,
                passportLastName = user.LastName,
                passportExpiryDate = user.PassportExpiryDate?.ToString("yyyy-MM-dd"),
                passportImageUrl = user.PassportImageUrl
            },
            passport = new
            {
                hasNumber = !string.IsNullOrEmpty(user.PassportNumber),
                hasExpiry = user.PassportExpiryDate.HasValue,
                hasImage = !string.IsNullOrEmpty(user.PassportImageUrl),
                verified = user.PassportVerified,
                verifiedAt = user.PassportVerifiedAt
            },
            groupName = uc?.GroupName,
            attributes,
            scheduleCourses,
            optionTours,
            companions,
            surveys
        };
    }
}
