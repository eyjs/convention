using LocalRAG.Extensions;
using LocalRAG.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/conventions/{conventionId}/travel-guide")]
[Authorize]
public class TravelGuideController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TravelGuideController> _logger;

    public TravelGuideController(
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<TravelGuideController> logger)
    {
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// 여행 가이드 통합 조회 (긴급연락처, 미팅장소, 도시/국가 정보)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTravelGuide(int conventionId)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null) return NotFound();

        return Ok(new
        {
            location = convention.Location,
            destinationCity = convention.DestinationCity,
            destinationCountryCode = convention.DestinationCountryCode,
            emergencyContacts = convention.EmergencyContactsJson,
            meetingPointInfo = convention.MeetingPointInfo,
            startDate = convention.StartDate,
            endDate = convention.EndDate
        });
    }

    /// <summary>
    /// 목적지 날씨 조회 (OpenWeatherMap 프록시)
    /// </summary>
    [HttpGet("weather")]
    public async Task<IActionResult> GetWeather(int conventionId)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null) return NotFound();

        var city = convention.DestinationCity;
        if (string.IsNullOrWhiteSpace(city))
            return Ok(new { available = false, message = "목적지 도시가 설정되지 않았습니다." });

        var apiKey = _configuration["ApiKeys:OpenWeather"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return Ok(new { available = false, message = "날씨 API 키가 설정되지 않았습니다." });

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(city)}&appid={apiKey}&units=metric&lang=kr";
            var json = await client.GetStringAsync(url);
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "날씨 조회 실패 — City: {City}", city);
            return Ok(new { available = false, message = "날씨 정보를 가져올 수 없습니다." });
        }
    }

    /// <summary>
    /// 환율 조회 (open.er-api.com 무료 API)
    /// </summary>
    [HttpGet("exchange-rate")]
    public async Task<IActionResult> GetExchangeRate(int conventionId)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null) return NotFound();

        var countryCode = convention.DestinationCountryCode?.ToUpper();
        if (string.IsNullOrWhiteSpace(countryCode))
            return Ok(new { available = false, message = "목적지 국가가 설정되지 않았습니다." });

        var currencyMap = new Dictionary<string, (string Code, string Name)>
        {
            { "JP", ("JPY", "일본 엔") }, { "CN", ("CNY", "중국 위안") },
            { "TW", ("TWD", "대만 달러") }, { "HK", ("HKD", "홍콩 달러") },
            { "TH", ("THB", "태국 바트") }, { "VN", ("VND", "베트남 동") },
            { "SG", ("SGD", "싱가포르 달러") }, { "MY", ("MYR", "말레이시아 링깃") },
            { "ID", ("IDR", "인도네시아 루피아") }, { "PH", ("PHP", "필리핀 페소") },
            { "US", ("USD", "미국 달러") }, { "CA", ("CAD", "캐나다 달러") },
            { "GB", ("GBP", "영국 파운드") }, { "FR", ("EUR", "유로") },
            { "DE", ("EUR", "유로") }, { "IT", ("EUR", "유로") },
            { "ES", ("EUR", "유로") }, { "AU", ("AUD", "호주 달러") },
            { "NZ", ("NZD", "뉴질랜드 달러") }, { "TR", ("TRY", "터키 리라") },
            { "AE", ("AED", "UAE 디르함") }, { "IN", ("INR", "인도 루피") },
            { "CH", ("CHF", "스위스 프랑") }, { "MX", ("MXN", "멕시코 페소") },
        };

        if (!currencyMap.TryGetValue(countryCode, out var currency))
            return Ok(new { available = false, message = $"지원하지 않는 국가코드: {countryCode}" });

        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = await client.GetStringAsync("https://open.er-api.com/v6/latest/KRW");
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var rates = doc.RootElement.GetProperty("rates");

            if (rates.TryGetProperty(currency.Code, out var rateElement))
            {
                var rate = rateElement.GetDouble();
                return Ok(new
                {
                    available = true,
                    currencyCode = currency.Code,
                    currencyName = currency.Name,
                    rate,
                    krwPer1Unit = rate > 0 ? Math.Round(1.0 / rate, 2) : 0,
                    description = $"1 {currency.Code} = {(rate > 0 ? Math.Round(1.0 / rate, 2) : 0):N0} KRW"
                });
            }

            return Ok(new { available = false, message = $"환율 데이터 없음: {currency.Code}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "환율 조회 실패 — Country: {Country}", countryCode);
            return Ok(new { available = false, message = "환율 정보를 가져올 수 없습니다." });
        }
    }

    /// <summary>
    /// 일정 캘린더 내보내기 (.ics)
    /// </summary>
    [HttpGet("calendar.ics")]
    public async Task<IActionResult> ExportCalendar(int conventionId)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null) return NotFound();

        var ics = new System.Text.StringBuilder();
        ics.AppendLine("BEGIN:VCALENDAR");
        ics.AppendLine("VERSION:2.0");
        ics.AppendLine($"PRODID:-//StarTour//Convention//{conventionId}//KR");
        ics.AppendLine("CALSCALE:GREGORIAN");
        ics.AppendLine("METHOD:PUBLISH");

        if (convention.StartDate.HasValue && convention.EndDate.HasValue)
        {
            ics.AppendLine("BEGIN:VEVENT");
            ics.AppendLine($"DTSTART;VALUE=DATE:{convention.StartDate.Value:yyyyMMdd}");
            ics.AppendLine($"DTEND;VALUE=DATE:{convention.EndDate.Value.AddDays(1):yyyyMMdd}");
            ics.AppendLine($"SUMMARY:{Esc(convention.Title)}");
            if (!string.IsNullOrEmpty(convention.Location))
                ics.AppendLine($"LOCATION:{Esc(convention.Location)}");
            ics.AppendLine($"UID:conv-{conventionId}@event.ifa.co.kr");
            ics.AppendLine("END:VEVENT");
        }

        ics.AppendLine("END:VCALENDAR");

        return File(
            System.Text.Encoding.UTF8.GetBytes(ics.ToString()),
            "text/calendar",
            $"{convention.Title}_일정.ics");
    }

    private static string Esc(string t) =>
        t.Replace("\\", "\\\\").Replace(",", "\\,").Replace(";", "\\;").Replace("\n", "\\n");
}
