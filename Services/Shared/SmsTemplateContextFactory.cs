using LocalRAG.Entities;
using LocalRAG.Services.Shared.Models;

namespace LocalRAG.Services.Shared;

public class SmsTemplateContextFactory
{
    public SmsTemplateContext Create(LocalRAG.Entities.User user, LocalRAG.Entities.Convention convention)
    {
        var context = new SmsTemplateContext
        {
            GuestName = user.Name ?? string.Empty,
            GuestPhone = user.Phone ?? string.Empty,
            CorpPart = user.CorpPart ?? string.Empty,
            
            ConventionTitle = convention.Title ?? string.Empty,
            StartDate = convention.StartDate?.ToString("yyyy.MM.dd") ?? string.Empty,
            EndDate = convention.EndDate?.ToString("yyyy.MM.dd") ?? string.Empty,
            
            // URL 생성 로직
            Url = $"https://startour.ifa.co.kr/invite/{convention.Id}",
            
            // 실제 엔티티의 PreEndDate 사용
            PreEndDate = convention.PreEndDate?.ToString("yyyy.MM.dd") ?? string.Empty
        };

        // GuestAttributes 매핑
        if (user.GuestAttributes != null)
        {
            foreach (var attr in user.GuestAttributes)
            {
                if (!context.GuestAttributes.ContainsKey(attr.AttributeKey))
                {
                    context.GuestAttributes[attr.AttributeKey] = attr.AttributeValue;
                }
            }
        }

        return context;
    }
}
