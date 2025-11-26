using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LocalRAG.DTOs.FlightModels;
using LocalRAG.Data;
using LocalRAG.Entities.Flight;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Flight
{
    public class FlightService : IFlightService
    {
        // 인천공항 API 오퍼레이션 상수 정의
        private const string DEPARTURE_API_OPERATION = "getPassengerDeparturesDeOdp";
        private const string ARRIVAL_API_OPERATION = "getPassengerArrivalsDeOdp";
    
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FlightService> _logger;
        private readonly ConventionDbContext _dbContext;

        public FlightService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<FlightService> logger,
            ConventionDbContext dbContext)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<List<FlightDto>> GetFlights(string flightType, string searchDate, string? fromTime = null, string? toTime = null)
        {
            var operation = flightType.ToUpper() == "DEPARTURE" ? DEPARTURE_API_OPERATION : ARRIVAL_API_OPERATION;
            var url = BuildApiUrl(operation, searchDate, fromTime, toTime, null);

            _logger.LogInformation("Fetching flights from URL: {url}", url);

            var apiResponse = await GetApiResponse<IncheonApiResponse>(url);

            return apiResponse?.Response?.Body?.Items?
                .Select(item => MapToDto(item, flightType.ToUpper(), searchDate))
                .ToList() ?? new List<FlightDto>();
        }

        public async Task<FlightDto?> GetFlight(string flightId, string searchDate, string flightType)
        {
            var formattedFlightId = ParseAndPadFlightId(flightId);
            if (string.IsNullOrEmpty(formattedFlightId))
            {
                _logger.LogWarning("Invalid flightId format: {flightId}", flightId);
                return null;
            }

            var operation = flightType.ToUpper() == "DEPARTURE" ? DEPARTURE_API_OPERATION : ARRIVAL_API_OPERATION;
            var url = BuildApiUrl(operation, searchDate, null, null, formattedFlightId);

            _logger.LogInformation("Fetching single flight from URL: {url}", url);

            var apiResponse = await GetApiResponse<IncheonApiResponse>(url);
            var item = apiResponse?.Response?.Body?.Items?.FirstOrDefault();

            if (item != null)
            {
                return MapToDto(item, flightType.ToUpper(), searchDate);
            }

            return null;
        }

        private string BuildApiUrl(string operation, string searchDate, string? fromTime, string? toTime, string? flightId)
        {
            var serviceKey = _configuration["ApiKeys:IncheonAirport"] ?? throw new InvalidOperationException("Incheon Airport API Key is not configured");
            var baseUrl = "http://apis.data.go.kr/B551177/StatusOfPassengerFlightsDeOdp";
            
            var urlBuilder = new StringBuilder();
            urlBuilder.Append($"{baseUrl}/{operation}?serviceKey={serviceKey}&type=json&lang=K&pageNo=1&numOfRows=10");

            // searchday must be in YYYYMMDD format.
            if (DateTime.TryParse(searchDate, out var parsedDate))
            {
                urlBuilder.Append($"&searchday={parsedDate:yyyyMMdd}");
            }
            else
            {
                // Fallback for safety, though controller should validate.
                 urlBuilder.Append($"&searchday={searchDate.Replace("-", "")}");
            }
            
            // fromTime과 toTime이 null이거나 공백인 경우 기본값 설정
            fromTime = string.IsNullOrWhiteSpace(fromTime) ? "0000" : fromTime;
            toTime = string.IsNullOrWhiteSpace(toTime) ? "2359" : toTime;

            if (!string.IsNullOrWhiteSpace(fromTime))
                urlBuilder.Append($"&from_time={fromTime.Replace(":", "")}");
            
            if (!string.IsNullOrWhiteSpace(toTime))
                urlBuilder.Append($"&to_time={toTime.Replace(":", "")}");

            if (!string.IsNullOrWhiteSpace(flightId))
                urlBuilder.Append($"&flight_id={flightId}");

            return urlBuilder.ToString();
        }

        private async Task<T?> GetApiResponse<T>(string url) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<T>(jsonString, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch data from API. URL: {url}", url);
                return null;
            }
        }

        private FlightDto MapToDto(IncheonFlightItem item, string type, string dateStr)
        {
            return new FlightDto
            {
                Type = type,
                FlightId = item.FlightId,
                Airline = item.Airline,
                Airport = item.Airport,
                AirportCode = item.AirportCode,
                ScheduleDate = dateStr,
                ScheduleTime = FormatTime(item.ScheduleDateTime),
                EstimatedTime = FormatTime(item.EstimatedDateTime),
                Terminal = MapTerminal(item.Terminalid),
                Gate = item.Gatenumber ?? "미정",
                CheckInCounter = item.Chkinrange ?? "-",
                Status = item.Remark ?? "예정",
                MasterFlightId = item.Codeshare == "Slave" ? item.Masterflightid : null
            };
        }

        private string? ParseAndPadFlightId(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var cleaned = input.Replace(" ", "").ToUpper();
            var match = Regex.Match(cleaned, @"^([A-Z0-9]{2,3})(\d{1,4})$");
            if (!match.Success) return null;
            var airlineCode = match.Groups[1].Value;
            var flightNumber = match.Groups[2].Value;
            return $"{airlineCode}{flightNumber}";
        }

        private string MapTerminal(string? id) => id switch { "P01" => "T1", "P02" => "탑승동", "P03" => "T2", _ => id ?? "미정" };
        private string? FormatTime(string? dt) => (string.IsNullOrEmpty(dt) || dt.Length < 12) ? dt : $"{dt.Substring(8, 2)}:{dt.Substring(10, 2)}";

        // ========== 새로운 DB 기반 메서드들 ==========

        /// <summary>
        /// 인천공항 API에서 항공편 데이터를 가져와 DB에 저장
        /// </summary>
        public async Task SyncFlightDataFromApi(string searchDate, string flightType)
        {
            try
            {
                var operation = flightType.ToUpper() == "DEPARTURE" ? DEPARTURE_API_OPERATION : ARRIVAL_API_OPERATION;

                var url = BuildApiUrl(operation, searchDate, null, null, null);
                _logger.LogInformation("Syncing flight data from API: {url}", url);

                var apiResponse = await GetApiResponse<IncheonApiResponse>(url);
                if (apiResponse?.Response?.Body?.Items == null)
                {
                    _logger.LogWarning("No flight data received from API for date: {searchDate}, type: {flightType}", searchDate, flightType);
                    return;
                }

                // 날짜 포맷 변환 (YYYY-MM-DD -> YYYYMMDD)
                var formattedDate = searchDate.Replace("-", "");

                foreach (var item in apiResponse.Response.Body.Items)
                {
                    // 중복 체크: FlightId + ScheduleDate + FlightType 조합
                    var exists = await _dbContext.IncheonFlightData
                        .AnyAsync(f => f.FlightId == item.FlightId
                                    && f.ScheduleDate == formattedDate
                                    && f.FlightType == flightType.ToUpper());

                    if (!exists)
                    {
                        var flightData = new IncheonFlightData
                        {
                            FlightId = item.FlightId ?? "",
                            Airline = item.Airline ?? "",
                            Airport = item.Airport ?? "",
                            AirportCode = item.AirportCode ?? "",
                            FlightType = flightType.ToUpper(),
                            ScheduleDate = formattedDate,
                            ScheduleTime = FormatTime(item.ScheduleDateTime),
                            EstimatedTime = FormatTime(item.EstimatedDateTime),
                            Terminal = MapTerminal(item.Terminalid),
                            Gate = item.Gatenumber,
                            CheckInCounter = item.Chkinrange,
                            Status = item.Remark,
                            MasterFlightId = item.Codeshare == "Slave" ? item.Masterflightid : null
                        };

                        _dbContext.IncheonFlightData.Add(flightData);
                    }
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully synced {count} flights to database", apiResponse.Response.Body.Items.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing flight data from API");
                throw;
            }
        }

        /// <summary>
        /// DB에서 항공편 번호와 날짜로 검색
        /// </summary>
        public async Task<List<IncheonFlightData>> SearchFlightsFromDb(string flightId, string searchDate, bool forceRefresh = false)
        {
            var formattedFlightId = ParseAndPadFlightId(flightId);
            if (string.IsNullOrEmpty(formattedFlightId))
            {
                _logger.LogWarning("Invalid flightId format: {flightId}", flightId);
                return new List<IncheonFlightData>();
            }

            var formattedDate = searchDate.Replace("-", "");

            var flights = await _dbContext.IncheonFlightData
                .Where(f => (f.FlightId == formattedFlightId || f.MasterFlightId == formattedFlightId) && f.ScheduleDate == formattedDate)
                .OrderBy(f => f.FlightType)
                .ToListAsync();

            // 강제 갱신 또는 캐시 만료된 경우 API에서 재동기화
            bool shouldRefresh = forceRefresh || !flights.Any() || IsCacheExpired(flights.FirstOrDefault(), formattedDate);

            if (shouldRefresh)
            {
                _logger.LogInformation("Refreshing flight data from API for flight: {flightId}, date: {searchDate}, forceRefresh: {forceRefresh}",
                    flightId, searchDate, forceRefresh);

                // 기존 데이터 삭제 (갱신을 위해)
                if (flights.Any())
                {
                    _dbContext.IncheonFlightData.RemoveRange(flights);
                    await _dbContext.SaveChangesAsync();
                }

                await SyncFlightDataFromApi(searchDate, "DEPARTURE");
                await SyncFlightDataFromApi(searchDate, "ARRIVAL");

                // 재조회
                flights = await _dbContext.IncheonFlightData
                    .Where(f => f.FlightId == formattedFlightId && f.ScheduleDate == formattedDate)
                    .OrderBy(f => f.FlightType)
                    .ToListAsync();
            }

            return flights;
        }

        /// <summary>
        /// DB에서 날짜와 타입별 항공편 목록 조회
        /// </summary>
        public async Task<List<IncheonFlightData>> GetFlightsByDateFromDb(string searchDate, string flightType, bool forceRefresh = false)
        {
            var formattedDate = searchDate.Replace("-", "");

            var flights = await _dbContext.IncheonFlightData
                .Where(f => f.ScheduleDate == formattedDate && f.FlightType == flightType.ToUpper())
                .OrderBy(f => f.ScheduleTime)
                .ToListAsync();

            // 강제 갱신 또는 캐시 만료된 경우 API에서 재동기화
            bool shouldRefresh = forceRefresh || !flights.Any() || IsCacheExpired(flights.FirstOrDefault(), formattedDate);

            if (shouldRefresh)
            {
                _logger.LogInformation("Refreshing flight data from API for date: {searchDate}, type: {flightType}, forceRefresh: {forceRefresh}",
                    searchDate, flightType, forceRefresh);

                // 기존 데이터 삭제 (갱신을 위해)
                if (flights.Any())
                {
                    _dbContext.IncheonFlightData.RemoveRange(flights);
                    await _dbContext.SaveChangesAsync();
                }

                await SyncFlightDataFromApi(searchDate, flightType);

                // 재조회
                flights = await _dbContext.IncheonFlightData
                    .Where(f => f.ScheduleDate == formattedDate && f.FlightType == flightType.ToUpper())
                    .OrderBy(f => f.ScheduleTime)
                    .ToListAsync();
            }

            return flights;
        }

        /// <summary>
        /// 캐시 만료 여부 확인 (TTL 기반)
        /// - 과거 항공편: 영구 캐시
        /// - 당일 항공편: 1시간 TTL
        /// - 미래 1-3일: 6시간 TTL
        /// - 미래 4일 이상: 24시간 TTL
        /// </summary>
        private bool IsCacheExpired(IncheonFlightData? flight, string scheduleDateStr)
        {
            if (flight == null) return true;

            // 운항 날짜 파싱 (YYYYMMDD -> DateTime)
            if (!DateTime.TryParseExact(scheduleDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var scheduleDate))
            {
                return true; // 파싱 실패 시 갱신
            }

            var now = DateTime.UtcNow;
            var scheduleDateTime = DateTime.SpecifyKind(scheduleDate, DateTimeKind.Utc);
            var lastUpdated = flight.UpdatedAt ?? flight.CreatedAt;

            // 과거 항공편: 영구 캐시
            if (scheduleDateTime.Date < now.Date)
            {
                return false;
            }

            // 당일 항공편: 1시간 TTL
            if (scheduleDateTime.Date == now.Date)
            {
                return (now - lastUpdated).TotalHours > 1;
            }

            // 미래 항공편
            var daysUntilFlight = (scheduleDateTime.Date - now.Date).Days;

            // 1-3일 후: 6시간 TTL
            if (daysUntilFlight <= 3)
            {
                return (now - lastUpdated).TotalHours > 6;
            }

            // 4일 이상: 24시간 TTL
            return (now - lastUpdated).TotalHours > 24;
        }
    }
}