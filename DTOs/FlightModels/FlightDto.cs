using System.Text.Json.Serialization;

namespace LocalRAG.DTOs.FlightModels;

// Main DTO to be returned to the client
public class FlightDto
{
    public string? Type { get; set; }
    public string? FlightId { get; set; }
    public string? Airline { get; set; }
    public string? Airport { get; set; }
    public string? AirportCode { get; set; }
    public string? ScheduleDate { get; set; }
    public string? ScheduleTime { get; set; }
    public string? EstimatedTime { get; set; }
    public string? Terminal { get; set; }
    public string? Gate { get; set; }
    public string? CheckInCounter { get; set; }
    public string? Status { get; set; }
    public string? MasterFlightId { get; set; }
}

// DTOs for deserializing the OpenAPI response
public class IncheonApiResponse
{
    [JsonPropertyName("response")]
    public IncheonResponseWrapper? Response { get; set; }
}

public class IncheonResponseWrapper
{
    [JsonPropertyName("header")]
    public IncheonResponseHeader? Header { get; set; }
    
    [JsonPropertyName("body")]
    public IncheonResponseBody? Body { get; set; }
}

public class IncheonResponseHeader
{
    [JsonPropertyName("resultCode")]
    public string? ResultCode { get; set; }

    [JsonPropertyName("resultMsg")]
    public string? ResultMsg { get; set; }
}

public class IncheonResponseBody
{
    [JsonPropertyName("items")]
    public List<IncheonFlightItem>? Items { get; set; }
}

public class IncheonFlightItem
{
    public string? Masterflightid { get; set; }
    public string? Remark { get; set; }
    public string? ScheduleDateTime { get; set; }
    public string? Terminalid { get; set; }
    public string? TypeOfFlight { get; set; }
    public string? Airline { get; set; }
    public string? Airport { get; set; }
    public string? AirportCode { get; set; }
    public string? Chkinrange { get; set; }
    public string? Codeshare { get; set; }
    public string? EstimatedDateTime { get; set; }
    public string? Fid { get; set; }
    public string? FlightId { get; set; }
    public string? Gatenumber { get; set; }
    public string? Fstandposition { get; set; }
}
