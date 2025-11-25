using LocalRAG.DTOs.FlightModels;
using LocalRAG.Entities.Flight;

namespace LocalRAG.Services.Flight
{
    public interface IFlightService
    {
        // 인천공항 API에서 데이터를 가져와 DB에 저장
        Task SyncFlightDataFromApi(string searchDate, string flightType);

        // DB에서 항공편 검색 (항공편 번호 + 날짜), forceRefresh=true 시 강제 갱신
        Task<List<IncheonFlightData>> SearchFlightsFromDb(string flightId, string searchDate, bool forceRefresh = false);

        // DB에서 날짜 기준 항공편 목록 조회
        Task<List<IncheonFlightData>> GetFlightsByDateFromDb(string searchDate, string flightType, bool forceRefresh = false);

        // 레거시 메서드 (기존 호환성 유지)
        Task<List<FlightDto>> GetFlights(string flightType, string searchDate, string? fromTime = null, string? toTime = null);
        Task<FlightDto?> GetFlight(string flightId, string searchDate, string flightType);
    }
}
