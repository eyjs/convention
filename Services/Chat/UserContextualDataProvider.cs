using LocalRAG.Data;
﻿using LocalRAG.Entities;
﻿using Microsoft.EntityFrameworkCore;
﻿using System.Text;
﻿using LocalRAG.DTOs.ChatModels;
﻿
﻿namespace LocalRAG.Services.Chat;
﻿
﻿public class UserContextualDataProvider
﻿{
﻿    private readonly ConventionDbContext _context;
﻿    private readonly ILogger<UserContextualDataProvider> _logger;
﻿
﻿    public UserContextualDataProvider(ConventionDbContext context, ILogger<UserContextualDataProvider> logger)
﻿    {
﻿        _context = context;
﻿        _logger = logger;
﻿    }
﻿
﻿    /// <summary>
﻿    /// 사용자의 의도(Intent)에 따라 맞춤형 컨텍스트를 생성합니다.
﻿    /// </summary>
﻿    public async Task<string?> BuildUserContextAsync(ChatUserContext? userContext, ChatIntentRouter.Intent intent)
﻿    {
﻿        if (userContext?.UserId is null)
﻿            return null;
﻿
﻿        // DB에서 사용자 정보를 한 번만 조회합니다. (효율성)
﻿        var user = await _context.Users
﻿            .Include(u => u.GuestAttributes)
﻿            .Include(u => u.GuestScheduleTemplates)
﻿                .ThenInclude(gst => gst.ScheduleTemplate!)
﻿                .ThenInclude(st => st.ScheduleItems)
﻿            .AsNoTracking()
﻿            .FirstOrDefaultAsync(u => u.Id == userContext.UserId);
﻿
﻿        if (user == null)
﻿        {
﻿            _logger.LogWarning("No user found for ID {UserId}", userContext.UserId);
﻿            return null;
﻿        }
﻿
﻿        var sb = new StringBuilder();
﻿
﻿        // 의도에 따라 필요한 컨텍스트만 선택적으로 빌드합니다.
﻿        switch (intent)
﻿        {
﻿            case ChatIntentRouter.Intent.PersonalInfo:
﻿                AppendPersonalInfo(sb, user);
﻿                AppendAttributesInfo(sb, user);
﻿                break;
﻿
﻿            case ChatIntentRouter.Intent.PersonalSchedule:
﻿                AppendScheduleInfo(sb, user);
﻿                break;
﻿
﻿            // "내 모든 정보"와 같이 모든 정보가 필요한 경우를 위한 처리
﻿            // Intent.Unknown 이나 General의 경우에도 전체 정보를 제공할 수 있습니다.
﻿            default:
﻿                AppendPersonalInfo(sb, user);
﻿                AppendScheduleInfo(sb, user);
﻿                AppendAttributesInfo(sb, user);
﻿                break;
﻿        }
﻿
﻿        return sb.ToString();
﻿    }
﻿
﻿    /// <summary>
﻿    /// 개인 정보 섹션을 StringBuilder에 추가합니다.
﻿    /// </summary>
﻿    private void AppendPersonalInfo(StringBuilder sb, User user)
﻿    {
﻿        sb.AppendLine($"# {user.Name}님의 개인 정보");
﻿        sb.AppendLine($"- 이름: {user.Name}");
﻿        if (!string.IsNullOrEmpty(user.CorpPart)) sb.AppendLine($"- 부서: {user.CorpPart}");
﻿        if (!string.IsNullOrEmpty(user.Phone)) sb.AppendLine($"- 연락처: {user.Phone}");
﻿        if (!string.IsNullOrEmpty(user.Email)) sb.AppendLine($"- 이메일: {user.Email}");
﻿        if (!string.IsNullOrEmpty(user.Affiliation)) sb.AppendLine($"- 소속: {user.Affiliation}");
﻿    }
﻿
﻿    /// <summary>
﻿    /// 상세한 일정 정보 섹션을 StringBuilder에 추가합니다. (가독성 개선)
﻿    /// </summary>
﻿    private void AppendScheduleInfo(StringBuilder sb, User user)
﻿    {
﻿        if (!user.GuestScheduleTemplates.Any()) return;
﻿
﻿        var allItems = user.GuestScheduleTemplates
﻿            .Where(gst => gst.ScheduleTemplate != null)
﻿            .SelectMany(gst => gst.ScheduleTemplate!.ScheduleItems)
﻿            .OrderBy(si => si.ScheduleDate)
﻿            .ThenBy(si => si.StartTime);
﻿
﻿        if (!allItems.Any()) return;
﻿
﻿        sb.AppendLine("\n## 나의 전체 일정");
﻿
﻿        // 날짜별로 일정을 그룹화하여 보여줍니다.
﻿        foreach (var group in allItems.GroupBy(item => item.ScheduleDate.Date))
﻿        {
﻿            sb.AppendLine($"\n### {group.Key:yyyy년 MM월 dd일 (ddd)}");
﻿            foreach (var item in group)
﻿            {
﻿                // 시작 시간과 종료 시간을 모두 표시합니다.
﻿                // ScheduleItem 모델에 EndTime 속성이 있다고 가정합니다. 없다면 추가해야 합니다.
﻿                string timeRange = !string.IsNullOrEmpty(item.EndTime)
﻿                    ? $"{item.StartTime} ~ {item.EndTime}"
﻿                    : $"{item.StartTime}";
﻿                sb.AppendLine($"- ({timeRange}) {item.Title}");
﻿            }
﻿        }
﻿    }
﻿
﻿    /// <summary>
﻿    /// 추가 속성 정보 섹션을 StringBuilder에 추가합니다.
﻿    /// </summary>
﻿    private void AppendAttributesInfo(StringBuilder sb, User user)
﻿    {
﻿        if (!user.GuestAttributes.Any()) return;
﻿
﻿        sb.AppendLine("\n## 추가 정보");
﻿        foreach (var attr in user.GuestAttributes)
﻿        {
﻿            sb.AppendLine($"- {attr.AttributeKey}: {attr.AttributeValue}");
﻿        }
﻿    }
﻿}
﻿