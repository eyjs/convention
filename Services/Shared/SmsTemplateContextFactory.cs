using LocalRAG.Entities;
using LocalRAG.Services.Shared.Models;

namespace LocalRAG.Services.Shared;

public class SmsTemplateContextFactory
{
    private const string BaseUrl = "https://event.ifa.co.kr";

    /// <summary>
    /// SMS/알림톡 템플릿 변수 컨텍스트 생성
    /// </summary>
    /// <param name="seatLayoutId">좌석 배치도 ID (선택, 있으면 딥링크 생성)</param>
    /// <param name="accessToken">게스트 AccessToken (자동 로그인용)</param>
    public SmsTemplateContext Create(
        LocalRAG.Entities.User user,
        LocalRAG.Entities.Convention convention,
        int? seatLayoutId = null,
        string? accessToken = null)
    {
        var context = new SmsTemplateContext
        {
            GuestName = user.Name ?? string.Empty,
            GuestPhone = user.Phone ?? string.Empty,
            CorpPart = user.CorpPart ?? string.Empty,

            ConventionTitle = convention.Title ?? string.Empty,
            StartDate = convention.StartDate?.ToString("yyyy.MM.dd") ?? string.Empty,
            EndDate = convention.EndDate?.ToString("yyyy.MM.dd") ?? string.Empty,

            Url = $"{BaseUrl}/invite/{convention.Id}",
            PreEndDate = convention.PreEndDate?.ToString("yyyy.MM.dd") ?? string.Empty,

            // 좌석 배치도 딥링크 (accessToken 포함 시 자동 로그인)
            SeatLink = seatLayoutId.HasValue
                ? (!string.IsNullOrEmpty(accessToken)
                    ? $"{BaseUrl}/auto-login?token={accessToken}&conventionId={convention.Id}&redirect=/conventions/{convention.Id}/my-seat?layout={seatLayoutId.Value}"
                    : $"{BaseUrl}/conventions/{convention.Id}/my-seat?layout={seatLayoutId.Value}")
                : string.Empty,
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
