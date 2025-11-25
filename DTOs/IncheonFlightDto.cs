using System.Text.Json.Serialization;

namespace LocalRAG.DTOs
{
    public class IncheonFlightDto
    {
        public string FlightNum { get; set; }      // 편명 (예: KE081)
        public string Airline { get; set; }        // 항공사
        public string Airport { get; set; }        // 상대 공항 (출발지/도착지)
        public string ScheduleTime { get; set; }   // 예정 시간 (HH:mm)
        public string EstimatedTime { get; set; }  // 변경/예상 시간 (HH:mm)
        public string Terminal { get; set; }       // 터미널 (T1, T2)
        public string Gate { get; set; }           // 게이트
        public string CheckInCounter { get; set; } // 체크인 카운터
        public string Status { get; set; }         // 운항 상태 (출발, 지연 등)
        public string Type { get; set; }           // DEPARTURE / ARRIVAL
        public string ScheduleDate { get; set; }   // 일정 날짜 (YYYY-MM-DD)
        public string MasterFlightId { get; set; } // 공동운항 시 주 편명
    }

    // OpenAPI 응답 매핑용 내부 클래스
    public class OpenApiResponse
    {
        [JsonPropertyName("response")]
        public ResponseBody Response { get; set; }
    }

    public class ResponseBody
    {
        [JsonPropertyName("body")]
        public BodyData Body { get; set; }
    }

    public class BodyData
    {
        [JsonPropertyName("items")]
        public List<FlightItem> Items { get; set; }
    }

    public class FlightItem
    {
        public string flightId { get; set; }
        public string airline { get; set; }
        public string airport { get; set; }
        public string scheduleDateTime { get; set; }
        public string estimatedDateTime { get; set; }
        public string terminalid { get; set; }
        public string gatenumber { get; set; }
        public string remark { get; set; }
        public string chkinrange { get; set; } // 체크인카운터
        public string codeshare { get; set; }
        public string masterflightid { get; set; }
    }
}
