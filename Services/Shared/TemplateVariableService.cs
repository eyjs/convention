using LocalRAG.Interfaces;
using LocalRAG.Services.Shared.Models;
using System.Text.RegularExpressions;

namespace LocalRAG.Services.Shared;

public class TemplateVariableService : ITemplateVariableService
{
    private readonly ILogger<TemplateVariableService> _logger;

    public TemplateVariableService(ILogger<TemplateVariableService> logger)
    {
        _logger = logger;
    }

    public string ReplaceVariables(string templateContent, SmsTemplateContext context)
    {
        if (string.IsNullOrEmpty(templateContent)) return string.Empty;

        // #{key} 패턴을 찾아서 치환 (영문, 숫자, 한글, 언더스코어 허용)
        return Regex.Replace(templateContent, @"\#\{([a-zA-Z0-9_\uAC00-\uD7A3]+)\}", match =>
        {
            string key = match.Groups[1].Value;
            return GetValueByKey(key, context);
        });
    }

    private string GetValueByKey(string key, SmsTemplateContext context)
    {
        // 1. 고정 프로퍼티 매핑 (대소문자 무시)
        switch (key.ToLower())
        {
            case "guest_name": return context.GuestName;
            case "guest_phone": return context.GuestPhone;
            case "corp_part": return context.CorpPart;
            case "title": return context.ConventionTitle;
            case "start_date": return context.StartDate;
            case "end_date": return context.EndDate;
            case "pre_end_date": return context.PreEndDate;
            case "url": return context.Url;
            case "seat_link": return context.SeatLink;
            case "table_number": return context.TableNumber;
        }

        // 2. 동적 속성 (GuestAttributes) 검색
        if (context.GuestAttributes.TryGetValue(key, out var attrValue))
        {
            return attrValue;
        }
        
        // 매칭되는 값이 없으면 원본 유지 (#{key}) 또는 빈 문자열
        // 여기서는 빈 문자열로 처리하여 사용자에게 혼란을 줄임 (또는 로그 남김)
        return string.Empty;
    }
}