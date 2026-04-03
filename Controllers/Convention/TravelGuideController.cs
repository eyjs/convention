using System.Text;
using LocalRAG.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/conventions/{conventionId}/travel-guide")]
[Authorize]
public class TravelGuideController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TravelGuideController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
            convention.Location,
            convention.DestinationCity,
            convention.DestinationCountryCode,
            EmergencyContacts = convention.EmergencyContactsJson,
            convention.MeetingPointInfo,
            convention.StartDate,
            convention.EndDate
        });
    }

    /// <summary>
    /// 일정 캘린더 내보내기 (.ics)
    /// </summary>
    [HttpGet("calendar.ics")]
    public async Task<IActionResult> ExportCalendar(int conventionId)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null) return NotFound();

        var ics = new StringBuilder();
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
            Encoding.UTF8.GetBytes(ics.ToString()),
            "text/calendar",
            $"{convention.Title}_일정.ics");
    }

    private static string Esc(string t) =>
        t.Replace("\\", "\\\\").Replace(",", "\\,").Replace(";", "\\;").Replace("\n", "\\n");
}
