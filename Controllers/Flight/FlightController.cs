using LocalRAG.Services.Flight;
using Microsoft.AspNetCore.Mvc;
using System.Globalization; // Added for CultureInfo.InvariantCulture

namespace LocalRAG.Controllers.Flight
{
    [ApiController]
    [Route("api/flight")]
    public class FlightController : ControllerBase
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightController> _logger;

        public FlightController(IFlightService flightService, ILogger<FlightController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        /// <summary>
        /// DB에서 항공편 번호와 날짜로 항공편 조회 (리팩토링된 버전)
        /// </summary>
        /// <param name="flightNumber">항공편명 (예: KE123)</param>
        /// <param name="date">조회 날짜 (YYYY-MM-DD 또는 YYYYMMDD)</param>
        /// <param name="forceRefresh">true 시 캐시 무시하고 API에서 실시간 조회</param>
        /// <returns>항공편 정보 목록</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchFlights(
            [FromQuery(Name = "flightNumber")] string flightNumber,
            [FromQuery(Name = "date")] string date,
            [FromQuery(Name = "forceRefresh")] bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(flightNumber) || string.IsNullOrWhiteSpace(date))
            {
                return BadRequest(new { message = "항공편 번호와 날짜를 입력해주세요." });
            }

            // 날짜 형식 유연하게 처리 (YYYY-MM-DD 또는 YYYYMMDD)
            DateTime parsedDate;
            bool isParsed = DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate) ||
                            DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);

            if (!isParsed)
            {
                return BadRequest(new { message = "올바른 날짜 형식(YYYY-MM-DD 또는 YYYYMMDD)을 입력해주세요." });
            }

            try
            {
                _logger.LogInformation("항공편 검색 요청: {flightNumber}, {date}, forceRefresh: {forceRefresh}", flightNumber, date, forceRefresh);
                var flights = await _flightService.SearchFlightsFromDb(flightNumber, date, forceRefresh);

                if (flights == null || !flights.Any())
                {
                    return NotFound(new { message = "해당 항공편 정보를 찾을 수 없습니다." });
                }

                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "항공편 검색 중 오류 발생");
                return StatusCode(500, new { message = "항공편 검색 중 서버 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 인천공항 운항정보를 조회합니다. 편명(flightId) 또는 날짜/시간(searchDate/from/to) 기준으로 조회합니다.
        /// (레거시 엔드포인트 - 기존 호환성 유지)
        /// </summary>
        /// <param name="searchDate">조회 날짜 (YYYY-MM-DD 또는 YYYYMMDD)</param>
        /// <param name="flightId">항공편명 (옵션, 예: KE123)</param>
        /// <param name="flightType">'departure' 또는 'arrival' (옵션, flightId 없을 시 사용)</param>
        /// <param name="fromTime">조회 시작 시간 (HH:mm, 옵션)</param>
        /// <param name="toTime">조회 종료 시간 (HH:mm, 옵션)</param>
        /// <returns>항공편 정보 또는 목록</returns>
        [HttpGet("incheon")]
        public async Task<IActionResult> GetIncheonData(
            [FromQuery(Name = "date")] string searchDate, // 매개변수 이름을 "date"로 변경하여 클라이언트 요청과 일치시킴
            [FromQuery(Name = "flightNumber")] string? flightId = null, // 매개변수 이름을 "flightNumber"로 변경하여 클라이언트 요청과 일치시킴
            [FromQuery] string flightType = "departure",
            [FromQuery] string? fromTime = null,
            [FromQuery] string? toTime = null)
        {
            if (string.IsNullOrWhiteSpace(searchDate))
            {
                return BadRequest(new { message = "날짜를 입력해주세요." });
            }

            // 날짜 형식 유연하게 처리 (YYYY-MM-DD 또는 YYYYMMDD)
            DateTime parsedDate;
            bool isParsed = DateTime.TryParseExact(searchDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate) ||
                            DateTime.TryParseExact(searchDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);

            if (!isParsed)
            {
                return BadRequest(new { message = "올바른 날짜 형식(YYYY-MM-DD 또는 YYYYMMDD)을 입력해주세요." });
            }

            try
            {
                // Case 1: Search by Flight ID
                if (!string.IsNullOrWhiteSpace(flightId))
                {
                    _logger.LogInformation("단일 항공편 조회 요청: {flightId}, {searchDate}, {flightType}", flightId, searchDate, flightType);
                    var flight = await _flightService.GetFlight(flightId, searchDate, flightType);
                    if (flight == null)
                    {
                        return NotFound(new { message = "해당 항공편 정보를 찾을 수 없습니다." });
                    }
                    return Ok(flight);
                }
                
                // Case 2: Search by Time Range (or full day)
                _logger.LogInformation("시간 범위 항공편 조회 요청: {searchDate} {fromTime}-{toTime}", searchDate, fromTime, toTime);
                
                // If no time range is provided, default to full day
                if (string.IsNullOrWhiteSpace(fromTime) && string.IsNullOrWhiteSpace(toTime))
                {
                    fromTime = "00:00";
                    toTime = "23:59";
                }

                var flights = await _flightService.GetFlights(flightType, searchDate, fromTime, toTime);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "항공편 정보 조회 중 오류 발생");
                return StatusCode(500, new { message = "항공편 정보 조회 중 서버 오류가 발생했습니다." });
            }
        }
    }
}