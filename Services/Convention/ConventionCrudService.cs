using LocalRAG.Constants;
using LocalRAG.DTOs.ConventionModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using ConventionModel = LocalRAG.Entities.Convention;

namespace LocalRAG.Services.Convention;

/// <summary>
/// 행사(Convention) CRUD 비즈니스 로직 구현
/// </summary>
public class ConventionCrudService : IConventionCrudService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConventionCrudService> _logger;

    public ConventionCrudService(IUnitOfWork unitOfWork, ILogger<ConventionCrudService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<object> GetConventionsAsync(bool includeDeleted = false)
    {
        IEnumerable<ConventionModel> conventions;

        if (includeDeleted)
        {
            conventions = await _unitOfWork.Conventions.GetAllAsync();
        }
        else
        {
            conventions = await _unitOfWork.Conventions.FindAsync(c => c.DeleteYn == DeleteStatus.Active);
        }

        var conventionIds = conventions.Select(c => c.Id).ToList();

        // 각 행사별 게스트 수와 일정 수를 조회
        var result = new List<object>();
        foreach (var c in conventions.OrderByDescending(c => c.RegDtm))
        {
            var guestCount = await _unitOfWork.UserConventions.CountAsync(uc => uc.ConventionId == c.Id);
            var scheduleCount = await _unitOfWork.ScheduleTemplates.CountAsync(st => st.ConventionId == c.Id);

            result.Add(new
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
                c.DestinationCountryCode,
                GuestCount = guestCount,
                ScheduleCount = scheduleCount
            });
        }

        return result;
    }

    public async Task<object> GetMyConventionsAsync(int userId)
    {
        var userConventions = await _unitOfWork.UserConventions.GetByUserIdAsync(userId);
        var conventionIds = userConventions.Select(uc => uc.ConventionId).ToList();

        if (!conventionIds.Any())
        {
            return new List<object>();
        }

        var conventions = await _unitOfWork.Conventions.FindAsync(
            c => conventionIds.Contains(c.Id) && c.DeleteYn == DeleteStatus.Active);

        var result = new List<object>();
        foreach (var c in conventions.OrderByDescending(c => c.RegDtm))
        {
            var guestCount = await _unitOfWork.UserConventions.CountAsync(uc => uc.ConventionId == c.Id);
            var scheduleCount = await _unitOfWork.ScheduleTemplates.CountAsync(st => st.ConventionId == c.Id);

            result.Add(new
            {
                c.Id,
                c.Title,
                c.ConventionType,
                c.StartDate,
                c.EndDate,
                c.BrandColor,
                c.CompleteYn,
                c.ConventionImg,
                c.DestinationCountryCode,
                GuestCount = guestCount,
                ScheduleCount = scheduleCount
            });
        }

        return result;
    }

    public async Task<object?> GetConventionAsync(int id)
    {
        // 가벼운 쿼리 사용 — UserConventions→User→GuestAttributes 로딩 제외
        var convention = await _unitOfWork.Conventions.GetConventionSummaryAsync(id);

        if (convention == null)
        {
            return null;
        }

        var guestCount = await _unitOfWork.UserConventions.CountAsync(uc => uc.ConventionId == id);

        return new
        {
            convention.Id,
            convention.Title,
            convention.ConventionType,
            convention.RenderType,
            convention.StartDate,
            convention.EndDate,
            convention.BrandColor,
            convention.ThemePreset,
            convention.ConventionImg,
            convention.Location,
            convention.DestinationCity,
            convention.DestinationCountryCode,
            convention.EmergencyContactsJson,
            convention.MeetingPointInfo,
            convention.DeleteYn,
            convention.CompleteYn,
            convention.RegDtm,
            GuestCount = guestCount,
            ScheduleCount = convention.ScheduleTemplates.Count,
            Features = convention.Features.Select(f => new { f.MenuName, f.IsActive }),
            Owners = convention.Owners.Select(o => new { o.Name, o.Telephone })
        };
    }

    public async Task<object> CreateConventionAsync(CreateConventionRequest request, string userId)
    {
        var convention = new ConventionModel
        {
            MemberId = userId,
            Title = request.Title,
            ConventionType = request.ConventionType,
            RenderType = request.RenderType ?? "STANDARD",
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BrandColor = request.BrandColor ?? "#6366f1",
            ThemePreset = request.ThemePreset ?? "default",
            ConventionImg = request.ConventionImg,
            Location = request.Location,
            // 국내 행사는 KR 강제, 도시 없음
            DestinationCity = request.ConventionType == "DOMESTIC" ? null : request.DestinationCity,
            DestinationCountryCode = request.ConventionType == "DOMESTIC" ? "KR" : request.DestinationCountryCode,
            EmergencyContactsJson = request.EmergencyContactsJson,
            MeetingPointInfo = request.MeetingPointInfo,
            RegDtm = DateTime.Now,
            DeleteYn = DeleteStatus.Active,
            CompleteYn = DeleteStatus.Active
        };

        await _unitOfWork.Conventions.AddAsync(convention);
        await _unitOfWork.SaveChangesAsync();

        // 기본 Features 추가
        var defaultFeatures = new[]
        {
            new Feature { ConventionId = convention.Id, MenuName = "일정", MenuUrl = "schedule", IsActive = true, IconUrl = "📅" },
            new Feature { ConventionId = convention.Id, MenuName = "채팅", MenuUrl = "chat", IsActive = true, IconUrl = "💬" },
            new Feature { ConventionId = convention.Id, MenuName = "갤러리", MenuUrl = "gallery", IsActive = true, IconUrl = "📷" },
            new Feature { ConventionId = convention.Id, MenuName = "게시판", MenuUrl = "board", IsActive = true, IconUrl = "📋" }
        };

        await _unitOfWork.Features.AddRangeAsync(defaultFeatures);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("행사 생성: {ConventionId} - {Title} (by {UserId})", convention.Id, convention.Title, userId);

        return new
        {
            convention.Id,
            convention.Title,
            convention.ConventionType,
            convention.StartDate,
            convention.EndDate,
            Message = "행사가 생성되었습니다."
        };
    }

    public async Task<object?> UpdateConventionAsync(int id, UpdateConventionRequest request)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(id);
        if (convention == null)
        {
            return null;
        }

        convention.Title = request.Title;
        convention.ConventionType = request.ConventionType;
        convention.RenderType = request.RenderType ?? convention.RenderType;
        convention.StartDate = request.StartDate;
        convention.EndDate = request.EndDate;
        convention.BrandColor = request.BrandColor ?? convention.BrandColor;
        convention.ThemePreset = request.ThemePreset ?? convention.ThemePreset;
        convention.ConventionImg = request.ConventionImg ?? convention.ConventionImg;
        convention.Location = request.Location ?? convention.Location;
        // 국내 행사는 KR 강제, 도시 없음
        if (request.ConventionType == "DOMESTIC")
        {
            convention.DestinationCountryCode = "KR";
            convention.DestinationCity = null;
        }
        else
        {
            convention.DestinationCity = request.DestinationCity ?? convention.DestinationCity;
            convention.DestinationCountryCode = request.DestinationCountryCode ?? convention.DestinationCountryCode;
        }
        convention.EmergencyContactsJson = request.EmergencyContactsJson ?? convention.EmergencyContactsJson;
        convention.MeetingPointInfo = request.MeetingPointInfo ?? convention.MeetingPointInfo;

        _unitOfWork.Conventions.Update(convention);
        await _unitOfWork.SaveChangesAsync();

        return new
        {
            convention.Id,
            convention.Title,
            Message = "행사 정보가 수정되었습니다."
        };
    }

    public async Task<bool> SoftDeleteConventionAsync(int id)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(id);
        if (convention == null)
        {
            return false;
        }

        convention.DeleteYn = DeleteStatus.Deleted;
        _unitOfWork.Conventions.Update(convention);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<object?> ToggleCompleteAsync(int id)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(id);
        if (convention == null)
        {
            return null;
        }

        convention.CompleteYn = convention.CompleteYn == DeleteStatus.Deleted ? DeleteStatus.Active : DeleteStatus.Deleted;
        _unitOfWork.Conventions.Update(convention);
        await _unitOfWork.SaveChangesAsync();

        return new
        {
            convention.Id,
            convention.CompleteYn,
            Message = convention.CompleteYn == DeleteStatus.Deleted ? "행사가 종료되었습니다." : "행사가 재개되었습니다."
        };
    }

    public async Task<bool> RestoreConventionAsync(int id)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(id);
        if (convention == null)
        {
            return false;
        }

        convention.DeleteYn = DeleteStatus.Active;
        _unitOfWork.Conventions.Update(convention);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
