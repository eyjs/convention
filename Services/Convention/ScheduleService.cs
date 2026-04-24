using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LocalRAG.Services.Convention;

/// <summary>
/// 일정 관리 서비스
/// Admin/User 컨트롤러의 일정 비즈니스 로직을 통합
/// </summary>
public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(IUnitOfWork unitOfWork, ILogger<ScheduleService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ============================================================
    // Admin 일정 템플릿 관리
    // ============================================================

    public async Task<object> GetScheduleTemplatesAsync(int conventionId)
    {
        var templates = await _unitOfWork.ScheduleTemplates.Query
            .Where(st => st.ConventionId == conventionId)
            .Include(st => st.ScheduleItems).ThenInclude(si => si.Images)
            .Include(st => st.GuestScheduleTemplates)
            .OrderBy(st => st.OrderNum)
            .Select(st => new
            {
                st.Id, st.CourseName, st.Description, st.OrderNum, st.CreatedAt,
                GuestCount = st.GuestScheduleTemplates.Count,
                ScheduleItems = st.ScheduleItems
                    .OrderBy(si => si.ScheduleDate)
                    .ThenBy(si => si.StartTime)
                    .Select(si => new
                    {
                        si.Id, si.ScheduleDate, si.StartTime, si.EndTime,
                        si.Title, si.Location, si.Content, si.OrderNum,
                        Images = si.Images.OrderBy(img => img.OrderNum).Select(img => new
                        {
                            img.Id, img.ImageUrl, img.OrderNum
                        }).ToList()
                    }).ToList()
            })
            .ToListAsync();

        return templates;
    }

    public async Task<ScheduleTemplate> CreateScheduleTemplateAsync(int conventionId, ScheduleTemplateDto dto)
    {
        var template = new ScheduleTemplate
        {
            ConventionId = conventionId,
            CourseName = dto.CourseName,
            Description = dto.Description,
            OrderNum = dto.OrderNum
        };

        await _unitOfWork.ScheduleTemplates.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return template;
    }

    public async Task<ScheduleTemplate?> UpdateScheduleTemplateAsync(int id, ScheduleTemplateDto dto)
    {
        var template = await _unitOfWork.ScheduleTemplates.GetByIdAsync(id);
        if (template == null) return null;

        template.CourseName = dto.CourseName;
        template.Description = dto.Description;
        template.OrderNum = dto.OrderNum;

        await _unitOfWork.SaveChangesAsync();
        return template;
    }

    public async Task<bool> DeleteScheduleTemplateAsync(int id)
    {
        var template = await _unitOfWork.ScheduleTemplates.Query
            .Include(st => st.ScheduleItems).ThenInclude(si => si.Images)
            .Include(st => st.GuestScheduleTemplates)
            .FirstOrDefaultAsync(st => st.Id == id);

        if (template == null) return false;

        if (template.GuestScheduleTemplates.Any())
            _unitOfWork.GuestScheduleTemplates.RemoveRange(template.GuestScheduleTemplates);

        // 일정 항목의 이미지 삭제
        var allImages = template.ScheduleItems.SelectMany(si => si.Images).ToList();
        if (allImages.Any())
            _unitOfWork.ScheduleImages.RemoveRange(allImages);

        if (template.ScheduleItems.Any())
            _unitOfWork.ScheduleItems.RemoveRange(template.ScheduleItems);

        _unitOfWork.ScheduleTemplates.Remove(template);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // Admin 일정 항목 관리
    // ============================================================

    public async Task<ScheduleItem> CreateScheduleItemAsync(ScheduleItemDto dto)
    {
        var item = new ScheduleItem
        {
            ScheduleTemplateId = dto.ScheduleTemplateId,
            ScheduleDate = dto.ScheduleDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Title = dto.Title,
            Location = dto.Location,
            Content = dto.Content,
            OrderNum = dto.OrderNum
        };

        await _unitOfWork.ScheduleItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return item;
    }

    public async Task<ScheduleItem?> UpdateScheduleItemAsync(int id, ScheduleItemDto dto)
    {
        var item = await _unitOfWork.ScheduleItems.GetByIdAsync(id);
        if (item == null) return null;

        item.ScheduleDate = dto.ScheduleDate;
        item.StartTime = dto.StartTime;
        item.EndTime = dto.EndTime;
        item.Title = dto.Title;
        item.Location = dto.Location;
        item.Content = dto.Content;
        item.OrderNum = dto.OrderNum;

        await _unitOfWork.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteScheduleItemAsync(int id)
    {
        var item = await _unitOfWork.ScheduleItems.Query
            .Include(si => si.Images)
            .FirstOrDefaultAsync(si => si.Id == id);
        if (item == null) return false;

        if (item.Images.Any())
            _unitOfWork.ScheduleImages.RemoveRange(item.Images);

        _unitOfWork.ScheduleItems.Remove(item);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<(int Count, string Message)> BulkCreateScheduleItemsAsync(BulkScheduleItemsDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return (0, "복사할 일정이 없습니다.");

        var items = dto.Items.Select(itemDto => new ScheduleItem
        {
            ScheduleTemplateId = itemDto.ScheduleTemplateId,
            ScheduleDate = itemDto.ScheduleDate,
            StartTime = itemDto.StartTime,
            EndTime = itemDto.EndTime,
            Title = itemDto.Title,
            Location = itemDto.Location,
            Content = itemDto.Content,
            OrderNum = itemDto.OrderNum
        }).ToList();

        await _unitOfWork.ScheduleItems.AddRangeAsync(items);
        await _unitOfWork.SaveChangesAsync();

        return (items.Count, $"{items.Count}개 일정이 추가되었습니다.");
    }

    // ============================================================
    // Admin 게스트-일정 배정
    // ============================================================

    public async Task<object> GetTemplateGuestsAsync(int templateId)
    {
        var guests = await _unitOfWork.GuestScheduleTemplates.Query
            .Where(gst => gst.ScheduleTemplateId == templateId)
            .Include(gst => gst.User)
            .Select(gst => new
            {
                Id = gst.User!.Id,
                Name = gst.User.Name,
                Phone = gst.User.Phone,
                CorpPart = gst.User.CorpPart,
                Affiliation = gst.User.Affiliation,
                AssignedAt = gst.AssignedAt
            })
            .ToListAsync();

        return guests;
    }

    public async Task<(bool Success, string? Error)> AssignOptionToursToGuestAsync(
        int conventionId, int guestId, List<int> optionTourIds)
    {
        var userExists = await _unitOfWork.Users.ExistsAsync(u => u.Id == guestId);
        if (!userExists) return (false, "User not found");

        // 기존 옵션투어 배정 삭제
        var existing = await _unitOfWork.UserOptionTours.Query
            .Where(uot => uot.UserId == guestId && uot.ConventionId == conventionId)
            .ToListAsync();
        _unitOfWork.UserOptionTours.RemoveRange(existing);

        if (optionTourIds is { Count: > 0 })
        {
            // 유효한 옵션투어 ID만 필터링
            var validTourIds = await _unitOfWork.OptionTours.Query
                .Where(ot => optionTourIds.Contains(ot.Id) && ot.ConventionId == conventionId)
                .Select(ot => ot.Id)
                .ToListAsync();

            foreach (var optionTourId in validTourIds)
            {
                await _unitOfWork.UserOptionTours.AddAsync(new UserOptionTour
                {
                    UserId = guestId,
                    OptionTourId = optionTourId,
                    ConventionId = conventionId
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> AssignSchedulesToGuestAsync(
        int conventionId, int guestId, AssignSchedulesDto dto)
    {
        var userExists = await _unitOfWork.Users.ExistsAsync(u => u.Id == guestId);
        if (!userExists) return (false, "User not found");

        var existing = await _unitOfWork.GuestScheduleTemplates.Query
            .Where(gst => gst.UserId == guestId && gst.ScheduleTemplate.ConventionId == conventionId)
            .ToListAsync();
        _unitOfWork.GuestScheduleTemplates.RemoveRange(existing);

        foreach (var templateId in dto.ScheduleTemplateIds)
        {
            await _unitOfWork.GuestScheduleTemplates.AddAsync(new GuestScheduleTemplate
            {
                UserId = guestId,
                ScheduleTemplateId = templateId,
                ConventionId = conventionId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> RemoveGuestFromScheduleAsync(int userId, int templateId)
    {
        var assignment = await _unitOfWork.GuestScheduleTemplates.Query
            .FirstOrDefaultAsync(gst => gst.UserId == userId && gst.ScheduleTemplateId == templateId);

        if (assignment == null) return false;

        _unitOfWork.GuestScheduleTemplates.Remove(assignment);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetAllSchedulesAsync(int conventionId)
    {
        var items = await _unitOfWork.ScheduleItems.Query
            .Include(si => si.ScheduleTemplate)
            .Where(si => si.ScheduleTemplate!.ConventionId == conventionId)
            .OrderBy(si => si.ScheduleDate)
            .ThenBy(si => si.StartTime)
            .Select(si => new
            {
                si.Id, si.ScheduleDate, si.StartTime, si.EndTime,
                si.Title, si.Location, si.Content, si.OrderNum,
                Templates = new[] { new { si.ScheduleTemplate!.Id, si.ScheduleTemplate.CourseName } }
            })
            .ToListAsync();

        return items;
    }

    // ============================================================
    // User 일정 조회/관리
    // ============================================================

    public async Task<(List<ScheduleItemDto>? Result, string? Error, int StatusCode)> GetUserScheduleAsync(
        int userId, int conventionId)
    {
        try
        {
            var userInConvention = await _unitOfWork.UserConventions.Query
                .AnyAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

            if (!userInConvention)
                return (null, "User not found in this convention", 404);

            var user = await _unitOfWork.Users.Query
                .Include(u => u.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                        .ThenInclude(st => st!.ScheduleItems)
                            .ThenInclude(si => si.Images)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return (null, "User not found", 404);

            var schedules = user.GuestScheduleTemplates
                .Where(gst => gst.ScheduleTemplate is not null && gst.ScheduleTemplate.ConventionId == conventionId)
                .SelectMany(gst => gst.ScheduleTemplate!.ScheduleItems.Select(si => new
                {
                    ScheduleItem = si,
                    TemplateId = gst.ScheduleTemplate.Id,
                    gst.ScheduleTemplate.CourseName
                }))
                .OrderBy(x => x.ScheduleItem.ScheduleDate)
                .ThenBy(x => x.ScheduleItem.StartTime)
                .ToList();

            var templateIds = schedules.Select(s => s.TemplateId).Distinct().ToList();
            var participantCounts = await _unitOfWork.GuestScheduleTemplates.Query
                .Where(gst => templateIds.Contains(gst.ScheduleTemplateId))
                .GroupBy(gst => gst.ScheduleTemplateId)
                .Select(g => new { TemplateId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TemplateId, x => x.Count);

            var result = schedules.Select(s => new ScheduleItemDto
            {
                Id = s.ScheduleItem.Id,
                ScheduleTemplateId = s.TemplateId,
                ScheduleDate = s.ScheduleItem.ScheduleDate,
                StartTime = s.ScheduleItem.StartTime,
                EndTime = s.ScheduleItem.EndTime,
                Title = s.ScheduleItem.Title,
                Content = s.ScheduleItem.Content,
                Location = s.ScheduleItem.Location,
                OrderNum = s.ScheduleItem.OrderNum,
                CourseName = s.CourseName,
                ParticipantCount = participantCounts.GetValueOrDefault(s.TemplateId, 0),
                VisibleAttributes = s.ScheduleItem.VisibleAttributes,
                MapUrl = s.ScheduleItem.MapUrl,
                Images = s.ScheduleItem.Images.OrderBy(img => img.OrderNum).Select(img => new ScheduleImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    OrderNum = img.OrderNum
                }).ToList()
            }).ToList();

            return (result, null, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "일정 조회 중 오류 발생: userId={UserId}, conventionId={ConventionId}", userId, conventionId);
            return (null, "Internal server error", 500);
        }
    }

    public async Task<(GuestScheduleTemplate? Result, string? Error, int StatusCode)> AddUserScheduleAsync(
        UserScheduleDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
        if (user is null)
            return (null, "User not found.", 404);

        var scheduleTemplate = await _unitOfWork.ScheduleTemplates.GetByIdAsync(dto.ScheduleTemplateId);
        if (scheduleTemplate is null)
            return (null, "Schedule Template not found.", 404);

        var existing = await _unitOfWork.GuestScheduleTemplates.Query
            .FirstOrDefaultAsync(gst => gst.UserId == dto.UserId && gst.ScheduleTemplateId == dto.ScheduleTemplateId);

        if (existing is not null)
            return (null, "Schedule template is already assigned to this user.", 409);

        var userSchedule = new GuestScheduleTemplate
        {
            UserId = dto.UserId,
            ScheduleTemplateId = dto.ScheduleTemplateId,
            ConventionId = scheduleTemplate.ConventionId
        };

        await _unitOfWork.GuestScheduleTemplates.AddAsync(userSchedule);
        await _unitOfWork.SaveChangesAsync();

        return (userSchedule, null, 200);
    }

    public async Task<bool> RemoveUserScheduleAsync(int userId, int scheduleTemplateId)
    {
        var userSchedule = await _unitOfWork.GuestScheduleTemplates.Query
            .FirstOrDefaultAsync(gst => gst.UserId == userId && gst.ScheduleTemplateId == scheduleTemplateId);

        if (userSchedule is null) return false;

        _unitOfWork.GuestScheduleTemplates.Remove(userSchedule);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<(object? Result, bool Found)> GetAssignedTemplatesAsync(int userId)
    {
        var user = await _unitOfWork.Users.Query
            .Include(u => u.GuestScheduleTemplates)
            .ThenInclude(gst => gst.ScheduleTemplate)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return (null, false);

        var templates = user.GuestScheduleTemplates
            .Select(gst => gst.ScheduleTemplate)
            .Where(st => st is not null)
            .ToList();

        return (templates, true);
    }

    public async Task<(object? Result, string? Error, int StatusCode)> GetOptionToursAsync(
        int userId, int conventionId)
    {
        try
        {
            var userInConvention = await _unitOfWork.UserConventions.Query
                .AnyAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

            if (!userInConvention)
                return (null, "User not found in this convention", 404);

            var optionToursData = await _unitOfWork.UserOptionTours.Query
                .Where(uot => uot.UserId == userId && uot.ConventionId == conventionId)
                .Include(uot => uot.OptionTour)
                    .ThenInclude(ot => ot!.Images)
                .OrderBy(uot => uot.OptionTour!.Date)
                .ThenBy(uot => uot.OptionTour!.StartTime)
                .ToListAsync();

            var optionTours = optionToursData.Select(uot => new
            {
                id = uot.OptionTour!.Id,
                date = uot.OptionTour.Date.ToString("yyyy-MM-dd"),
                startTime = uot.OptionTour.StartTime,
                endTime = uot.OptionTour.EndTime,
                name = uot.OptionTour.Name,
                location = uot.OptionTour.Location,
                mapUrl = uot.OptionTour.MapUrl,
                content = uot.OptionTour.Content,
                customOptionId = uot.OptionTour.CustomOptionId,
                images = uot.OptionTour.Images.OrderBy(img => img.OrderNum).Select(img => new
                {
                    id = img.Id,
                    imageUrl = img.ImageUrl,
                    orderNum = img.OrderNum
                }).ToList()
            }).ToList();

            return (optionTours, null, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "옵션투어 조회 중 오류 발생: userId={UserId}, conventionId={ConventionId}", userId, conventionId);
            return (null, "Internal server error", 500);
        }
    }

    public async Task<(object? Result, string? Error, int StatusCode)> GetScheduleParticipantsAsync(
        int scheduleTemplateId, string? userRole)
    {
        try
        {
            var template = await _unitOfWork.ScheduleTemplates.Query
                .FirstOrDefaultAsync(st => st.Id == scheduleTemplateId);

            if (template is null)
                return (null, "Schedule template not found", 404);

            var isAdmin = userRole == "Admin";

            var participants = await _unitOfWork.GuestScheduleTemplates.Query
                .Where(gst => gst.ScheduleTemplateId == scheduleTemplateId)
                .Include(gst => gst.User)
                .Select(gst => new
                {
                    id = gst.User.Id,
                    name = gst.User.Name,
                    organization = gst.User.CorpName ?? gst.User.Affiliation,
                    department = gst.User.CorpPart,
                    phone = isAdmin ? gst.User.Phone : null,
                    email = isAdmin ? gst.User.Email : null,
                    groupName = gst.User.Affiliation
                })
                .OrderBy(p => p.name)
                .ToListAsync();

            return (new
            {
                scheduleTemplateId,
                courseName = template.CourseName,
                totalCount = participants.Count,
                participants
            }, null, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참석자 목록 조회 중 오류 발생: scheduleTemplateId={TemplateId}", scheduleTemplateId);
            return (null, "Internal server error", 500);
        }
    }

    // ============================================================
    // Admin 옵션투어 관리
    // ============================================================

    public async Task<object> GetOptionToursByConventionAsync(int conventionId)
    {
        var optionTours = await _unitOfWork.OptionTours.Query
            .Where(ot => ot.ConventionId == conventionId)
            .OrderBy(ot => ot.Date)
            .ThenBy(ot => ot.StartTime)
            .Select(ot => new
            {
                ot.Id, ot.ConventionId,
                Date = ot.Date.ToString("yyyy-MM-dd"),
                ot.StartTime, ot.EndTime, ot.Name,
                ot.Location, ot.MapUrl,
                ot.CustomOptionId, ot.Content, ot.CreatedAt,
                ParticipantCount = ot.UserOptionTours.Count,
                Images = ot.Images.OrderBy(img => img.OrderNum).Select(img => new
                {
                    img.Id, img.ImageUrl, img.OrderNum
                }).ToList()
            })
            .ToListAsync();

        return optionTours;
    }

    public async Task<object> CreateOptionTourAsync(int conventionId, OptionTourAdminDto dto)
    {
        var optionTour = new OptionTour
        {
            ConventionId = conventionId,
            Date = DateTime.ParseExact(dto.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Name = dto.Name,
            Location = dto.Location,
            MapUrl = dto.MapUrl,
            CustomOptionId = dto.CustomOptionId,
            Content = dto.Content
        };

        await _unitOfWork.OptionTours.AddAsync(optionTour);
        await _unitOfWork.SaveChangesAsync();

        return new
        {
            optionTour.Id, optionTour.ConventionId,
            Date = optionTour.Date.ToString("yyyy-MM-dd"),
            optionTour.StartTime, optionTour.EndTime, optionTour.Name,
            optionTour.Location, optionTour.MapUrl,
            optionTour.CustomOptionId, optionTour.Content, optionTour.CreatedAt,
            ParticipantCount = 0
        };
    }

    public async Task<object?> UpdateOptionTourAsync(int id, OptionTourAdminDto dto)
    {
        var optionTour = await _unitOfWork.OptionTours.GetByIdAsync(id);
        if (optionTour == null) return null;

        optionTour.Date = DateTime.ParseExact(dto.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        optionTour.StartTime = dto.StartTime;
        optionTour.EndTime = dto.EndTime;
        optionTour.Name = dto.Name;
        optionTour.Location = dto.Location;
        optionTour.MapUrl = dto.MapUrl;
        optionTour.CustomOptionId = dto.CustomOptionId;
        optionTour.Content = dto.Content;

        await _unitOfWork.SaveChangesAsync();

        var participantCount = await _unitOfWork.UserOptionTours.Query
            .CountAsync(uot => uot.OptionTourId == id);

        return new
        {
            optionTour.Id, optionTour.ConventionId,
            Date = optionTour.Date.ToString("yyyy-MM-dd"),
            optionTour.StartTime, optionTour.EndTime, optionTour.Name,
            optionTour.Location, optionTour.MapUrl,
            optionTour.CustomOptionId, optionTour.Content, optionTour.CreatedAt,
            ParticipantCount = participantCount
        };
    }

    public async Task<bool> DeleteOptionTourAsync(int id)
    {
        var optionTour = await _unitOfWork.OptionTours.Query
            .Include(ot => ot.UserOptionTours)
            .Include(ot => ot.Images)
            .FirstOrDefaultAsync(ot => ot.Id == id);

        if (optionTour == null) return false;

        if (optionTour.UserOptionTours.Any())
            _unitOfWork.UserOptionTours.RemoveRange(optionTour.UserOptionTours);

        if (optionTour.Images.Any())
            _unitOfWork.ScheduleImages.RemoveRange(optionTour.Images);

        _unitOfWork.OptionTours.Remove(optionTour);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<object> GetOptionTourParticipantsAsync(int optionTourId)
    {
        var participants = await _unitOfWork.UserOptionTours.Query
            .Where(uot => uot.OptionTourId == optionTourId)
            .Include(uot => uot.User)
            .Select(uot => new
            {
                Id = uot.User!.Id,
                Name = uot.User.Name,
                Phone = uot.User.Phone,
                CorpPart = uot.User.CorpPart,
                Affiliation = uot.User.Affiliation,
                CreatedAt = uot.CreatedAt
            })
            .OrderBy(p => p.Name)
            .ToListAsync();

        return participants;
    }

    public async Task<(bool Success, object Result, int StatusCode)> AddParticipantsToOptionTourAsync(
        int optionTourId, List<int> userIds)
    {
        var optionTour = await _unitOfWork.OptionTours.GetByIdAsync(optionTourId);
        if (optionTour == null)
            return (false, new { message = "옵션투어를 찾을 수 없습니다." }, 404);

        var existingUserIds = await _unitOfWork.UserOptionTours.Query
            .Where(uot => uot.OptionTourId == optionTourId)
            .Select(uot => uot.UserId)
            .ToListAsync();

        var newUserIds = userIds.Except(existingUserIds).ToList();
        if (newUserIds.Count == 0)
            return (true, new { message = "모든 참석자가 이미 등록되어 있습니다.", addedCount = 0 }, 200);

        var validUserIds = await _unitOfWork.Users.Query
            .Where(u => newUserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync();

        if (validUserIds.Count == 0)
            return (false, new { message = "유효한 참석자가 없습니다." }, 400);

        foreach (var userId in validUserIds)
        {
            await _unitOfWork.UserOptionTours.AddAsync(new UserOptionTour
            {
                UserId = userId,
                OptionTourId = optionTourId,
                ConventionId = optionTour.ConventionId
            });
        }

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = $"{validUserIds.Count}명의 참석자가 추가되었습니다.", addedCount = validUserIds.Count }, 200);
    }

    public async Task<bool> RemoveParticipantFromOptionTourAsync(int optionTourId, int userId)
    {
        var mapping = await _unitOfWork.UserOptionTours.Query
            .FirstOrDefaultAsync(uot => uot.OptionTourId == optionTourId && uot.UserId == userId);

        if (mapping == null) return false;

        _unitOfWork.UserOptionTours.Remove(mapping);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // 이미지 갤러리 관리
    // ============================================================

    public async Task<object> AddScheduleItemImageAsync(int scheduleItemId, string imageUrl)
    {
        var maxOrder = await _unitOfWork.ScheduleImages.Query
            .Where(si => si.ScheduleItemId == scheduleItemId)
            .Select(si => (int?)si.OrderNum)
            .MaxAsync() ?? 0;

        var image = new ScheduleImage
        {
            ScheduleItemId = scheduleItemId,
            ImageUrl = imageUrl,
            OrderNum = maxOrder + 1
        };

        await _unitOfWork.ScheduleImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();

        return new { image.Id, image.ImageUrl, image.OrderNum };
    }

    public async Task<bool> RemoveScheduleImageAsync(int imageId)
    {
        var image = await _unitOfWork.ScheduleImages.GetByIdAsync(imageId);
        if (image == null) return false;

        _unitOfWork.ScheduleImages.Remove(image);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<object> AddOptionTourImageAsync(int optionTourId, string imageUrl)
    {
        var maxOrder = await _unitOfWork.ScheduleImages.Query
            .Where(si => si.OptionTourId == optionTourId)
            .Select(si => (int?)si.OrderNum)
            .MaxAsync() ?? 0;

        var image = new ScheduleImage
        {
            OptionTourId = optionTourId,
            ImageUrl = imageUrl,
            OrderNum = maxOrder + 1
        };

        await _unitOfWork.ScheduleImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();

        return new { image.Id, image.ImageUrl, image.OrderNum };
    }
}
