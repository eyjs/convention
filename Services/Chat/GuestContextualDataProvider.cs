using LocalRAG.Data;
using LocalRAG.Models;
using GuestModel = LocalRAG.Models.Guest;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LocalRAG.Services.Chat;

public class GuestContextualDataProvider
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<GuestContextualDataProvider> _logger;

    public GuestContextualDataProvider(ConventionDbContext context, ILogger<GuestContextualDataProvider> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 사용자의 의도(Intent)에 따라 맞춤형 컨텍스트를 생성합니다.
    /// </summary>
    public async Task<string?> BuildGuestContextAsync(ChatUserContext? userContext, ChatIntentRouter.Intent intent)
    {
        if (userContext?.GuestId is null)
            return null;

        // DB에서 게스트 정보를 한 번만 조회합니다. (효율성)
        var guest = await _context.Guests
            .Include(g => g.GuestAttributes)
            .Include(g => g.GuestScheduleTemplates)
                .ThenInclude(gst => gst.ScheduleTemplate!)
                .ThenInclude(st => st.ScheduleItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == userContext.GuestId);

        if (guest == null)
        {
            _logger.LogWarning("No guest found for ID {GuestId}", userContext.GuestId);
            return null;
        }

        var sb = new StringBuilder();

        // 의도에 따라 필요한 컨텍스트만 선택적으로 빌드합니다.
        switch (intent)
        {
            case ChatIntentRouter.Intent.PersonalInfo:
                AppendPersonalInfo(sb, guest);
                AppendAttributesInfo(sb, guest);
                break;

            case ChatIntentRouter.Intent.PersonalSchedule:
                AppendScheduleInfo(sb, guest);
                break;

            // "내 모든 정보"와 같이 모든 정보가 필요한 경우를 위한 처리
            // Intent.Unknown 이나 General의 경우에도 전체 정보를 제공할 수 있습니다.
            default:
                AppendPersonalInfo(sb, guest);
                AppendScheduleInfo(sb, guest);
                AppendAttributesInfo(sb, guest);
                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// 개인 정보 섹션을 StringBuilder에 추가합니다.
    /// </summary>
    private void AppendPersonalInfo(StringBuilder sb, GuestModel guest)
    {
        sb.AppendLine($"# {guest.GuestName}님의 개인 정보");
        sb.AppendLine($"- 이름: {guest.GuestName}");
        if (!string.IsNullOrEmpty(guest.CorpPart)) sb.AppendLine($"- 부서: {guest.CorpPart}");
        if (!string.IsNullOrEmpty(guest.Telephone)) sb.AppendLine($"- 연락처: {guest.Telephone}");
        if (!string.IsNullOrEmpty(guest.Email)) sb.AppendLine($"- 이메일: {guest.Email}");
        if (!string.IsNullOrEmpty(guest.Affiliation)) sb.AppendLine($"- 소속: {guest.Affiliation}");
    }

    /// <summary>
    /// 상세한 일정 정보 섹션을 StringBuilder에 추가합니다. (가독성 개선)
    /// </summary>
    private void AppendScheduleInfo(StringBuilder sb, GuestModel guest)
    {
        if (!guest.GuestScheduleTemplates.Any()) return;

        var allItems = guest.GuestScheduleTemplates
            .Where(gst => gst.ScheduleTemplate != null)
            .SelectMany(gst => gst.ScheduleTemplate!.ScheduleItems)
            .OrderBy(si => si.ScheduleDate)
            .ThenBy(si => si.StartTime);

        if (!allItems.Any()) return;

        sb.AppendLine("\n## 나의 전체 일정");

        // 날짜별로 일정을 그룹화하여 보여줍니다.
        foreach (var group in allItems.GroupBy(item => item.ScheduleDate.Date))
        {
            sb.AppendLine($"\n### {group.Key:yyyy년 MM월 dd일 (ddd)}");
            foreach (var item in group)
            {
                // 시작 시간과 종료 시간을 모두 표시합니다.
                // ScheduleItem 모델에 EndTime 속성이 있다고 가정합니다. 없다면 추가해야 합니다.
                string timeRange = !string.IsNullOrEmpty(item.EndTime)
                    ? $"{item.StartTime} ~ {item.EndTime}"
                    : $"{item.StartTime}";
                sb.AppendLine($"- ({timeRange}) {item.Title}");
            }
        }
    }

    /// <summary>
    /// 추가 속성 정보 섹션을 StringBuilder에 추가합니다.
    /// </summary>
    private void AppendAttributesInfo(StringBuilder sb, GuestModel guest)
    {
        if (!guest.GuestAttributes.Any()) return;

        sb.AppendLine("\n## 추가 정보");
        foreach (var attr in guest.GuestAttributes)
        {
            sb.AppendLine($"- {attr.AttributeKey}: {attr.AttributeValue}");
        }
    }
}