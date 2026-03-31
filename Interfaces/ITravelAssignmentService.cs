namespace LocalRAG.Interfaces;

public interface ITravelAssignmentService
{
    /// <summary>
    /// 사용자 본인의 여행 배정 정보 조회 (룸메이트 포함)
    /// </summary>
    Task<TravelInfoResponse?> GetMyTravelInfoAsync(int conventionId, int userId);

    /// <summary>
    /// 관리자: 전체 참석자 여행 배정 목록 조회
    /// </summary>
    Task<List<UserTravelAssignment>> GetAllAssignmentsAsync(int conventionId);

    /// <summary>
    /// 관리자: 개별 유저의 특정 일자 정보 수정
    /// </summary>
    Task UpdateTravelDayAsync(int conventionId, int userId, TravelDayUpdateRequest request);

    /// <summary>
    /// 관리자: 일괄 업데이트
    /// </summary>
    Task<BulkTravelUpdateResult> BulkUpdateAsync(int conventionId, List<TravelDayUpdateRequest> assignments);
}

// --- DTOs ---

public class TravelInfoResponse
{
    public List<TravelDayInfo> Days { get; set; } = new();
}

public class TravelDayInfo
{
    public string Date { get; set; } = string.Empty;
    public string? Bus { get; set; }
    public string? Hotel { get; set; }
    public string? Room { get; set; }
    public string? Memo { get; set; }
    public List<string> Roommates { get; set; } = new();
}

public class UserTravelAssignment
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public List<TravelDayData> Days { get; set; } = new();
}

public class TravelDayData
{
    public string Date { get; set; } = string.Empty;
    public string? Bus { get; set; }
    public string? Hotel { get; set; }
    public string? Room { get; set; }
    public string? Memo { get; set; }
}

public class TravelDayUpdateRequest
{
    public int UserId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? Bus { get; set; }
    public string? Hotel { get; set; }
    public string? Room { get; set; }
    public string? Memo { get; set; }
}

public class BulkTravelUpdateResult
{
    public int Updated { get; set; }
    public List<string> Warnings { get; set; } = new();
}
