using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Data.Entities;
using LocalRAG.Interfaces;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TravelController : ControllerBase
{
    private readonly TravelDbContext _context;
    private readonly IRagService _ragService;
    private readonly ILogger<TravelController> _logger;

    public TravelController(TravelDbContext context, IRagService ragService, ILogger<TravelController> logger)
    {
        _context = context;
        _ragService = ragService;
        _logger = logger;
    }

    // GET: api/travel/trips
    [HttpGet("trips")]
    public async Task<ActionResult<IEnumerable<TravelTrip>>> GetTrips()
    {
        var trips = await _context.TravelTrips
            .Include(t => t.TripGuests)
            .Include(t => t.ScheduleItems.Where(s => s.DeleteYn == 0))
            .ToListAsync();

        return Ok(trips);
    }

    // GET: api/travel/trips/{id}
    [HttpGet("trips/{id}")]
    public async Task<ActionResult<TravelTrip>> GetTrip(int id)
    {
        var trip = await _context.TravelTrips
            .Include(t => t.TripGuests)
            .Include(t => t.ScheduleItems.Where(s => s.DeleteYn == 0))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null)
        {
            return NotFound();
        }

        return Ok(trip);
    }

    // GET: api/travel/trips/{tripId}/dashboard/{guestId}
    [HttpGet("trips/{tripId}/dashboard/{guestId}")]
    public async Task<ActionResult<object>> GetTripDashboard(int tripId, int guestId)
    {
        var trip = await _context.TravelTrips
            .Include(t => t.TripGuests)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
        {
            return NotFound("Trip not found");
        }

        var schedules = await _context.ScheduleItems
            .Where(s => s.ConvId == tripId && s.GuestId == guestId && s.DeleteYn == 0)
            .OrderBy(s => s.GroupId)
            .ToListAsync();

        var guest = await _context.TripGuests
            .FirstOrDefaultAsync(g => g.Id == guestId && g.TripId == tripId);

        var dashboard = new
        {
            Trip = trip,
            Guest = guest,
            Schedules = schedules,
            TotalSchedules = schedules.Count,
            Groups = schedules.GroupBy(s => new { s.GroupId, s.GroupName, s.GroupIcon })
                             .Select(g => new
                             {
                                 g.Key.GroupId,
                                 g.Key.GroupName,
                                 g.Key.GroupIcon,
                                 ItemCount = g.Count()
                             })
                             .ToList()
        };

        return Ok(dashboard);
    }

    // GET: api/travel/trips/{tripId}/schedules/{guestId}
    [HttpGet("trips/{tripId}/schedules/{guestId}")]
    public async Task<ActionResult<IEnumerable<ScheduleItem>>> GetSchedules(int tripId, int guestId)
    {
        var schedules = await _context.ScheduleItems
            .Where(s => s.ConvId == tripId && s.GuestId == guestId && s.DeleteYn == 0)
            .OrderBy(s => s.GroupId)
            .ThenBy(s => s.Id)
            .ToListAsync();

        return Ok(schedules);
    }

    // POST: api/travel/trips
    [HttpPost("trips")]
    public async Task<ActionResult<TravelTrip>> CreateTrip(TravelTrip trip)
    {
        _context.TravelTrips.Add(trip);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, trip);
    }

    // POST: api/travel/trips/{tripId}/schedules
    [HttpPost("trips/{tripId}/schedules")]
    public async Task<ActionResult<ScheduleItem>> CreateSchedule(int tripId, ScheduleItem schedule)
    {
        schedule.ConvId = tripId;
        schedule.CreatedAt = DateTime.UtcNow;

        _context.ScheduleItems.Add(schedule);
        await _context.SaveChangesAsync();

        // RAG 벡터 데이터베이스에 일정 정보 추가
        try
        {
            var content = $"Trip: {tripId}, Group: {schedule.GroupName}, Activity: {schedule.SubGroupName}";
            var metadata = new Dictionary<string, object>
            {
                {"tripId", tripId},
                {"scheduleId", schedule.Id},
                {"groupName", schedule.GroupName},
                {"type", "schedule"}
            };

            await _ragService.AddDocumentAsync(content, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add schedule to RAG database");
        }

        return CreatedAtAction(nameof(GetSchedules), new { tripId = tripId, guestId = schedule.GuestId }, schedule);
    }

    // PUT: api/travel/schedules/{id}
    [HttpPut("schedules/{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, ScheduleItem schedule)
    {
        if (id != schedule.Id)
        {
            return BadRequest();
        }

        _context.Entry(schedule).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ScheduleExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/travel/schedules/{id}
    [HttpDelete("schedules/{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var schedule = await _context.ScheduleItems.FindAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        // 소프트 삭제
        schedule.DeleteYn = 1;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/travel/trips/{tripId}/sync-vector-data
    [HttpPost("trips/{tripId}/sync-vector-data")]
    public async Task<ActionResult<object>> SyncVectorData(int tripId)
    {
        try
        {
            var trip = await _context.TravelTrips
                .Include(t => t.ScheduleItems.Where(s => s.DeleteYn == 0))
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
            {
                return NotFound("Trip not found");
            }

            int syncedCount = 0;
            foreach (var schedule in trip.ScheduleItems)
            {
                var content = $"Trip: {trip.TripName}, Destination: {trip.Destination}, Group: {schedule.GroupName}, Activity: {schedule.SubGroupName}";
                var metadata = new Dictionary<string, object>
                {
                    {"tripId", tripId},
                    {"scheduleId", schedule.Id},
                    {"groupName", schedule.GroupName},
                    {"destination", trip.Destination ?? ""},
                    {"type", "schedule"}
                };

                await _ragService.AddDocumentAsync(content, metadata);
                syncedCount++;
            }

            // 여행 기본 정보도 추가
            var tripContent = $"Trip: {trip.TripName}, Destination: {trip.Destination}, Description: {trip.Description}, Start: {trip.StartDate:yyyy-MM-dd}, End: {trip.EndDate:yyyy-MM-dd}";
            var tripMetadata = new Dictionary<string, object>
            {
                {"tripId", tripId},
                {"destination", trip.Destination ?? ""},
                {"type", "trip_info"}
            };

            await _ragService.AddDocumentAsync(tripContent, tripMetadata);
            syncedCount++;

            return Ok(new
            {
                TripId = tripId,
                SyncedDocuments = syncedCount,
                Message = $"Successfully synced {syncedCount} documents to vector database"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync vector data for trip {TripId}", tripId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    // POST: api/travel/chat
    [HttpPost("chat")]
    public async Task<ActionResult<object>> TravelChat([FromBody] TravelChatRequest request)
    {
        try
        {
            // 여행 관련 질문에 특화된 프롬프트 생성
            var enhancedQuery = $"여행 관련 질문: {request.Question}";
            if (request.TripId.HasValue)
            {
                enhancedQuery += $" (여행 ID: {request.TripId})";
            }

            var ragResponse = await _ragService.QueryAsync(enhancedQuery, request.TopK);

            return Ok(new
            {
                Question = request.Question,
                Answer = ragResponse.Answer,
                Sources = ragResponse.Sources,
                LlmProvider = ragResponse.LlmProvider,
                TripId = request.TripId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process travel chat query: {Question}", request.Question);
            return BadRequest(new { Error = ex.Message });
        }
    }

    private bool ScheduleExists(int id)
    {
        return _context.ScheduleItems.Any(e => e.Id == id);
    }
}

// DTOs for Travel Chat
public record TravelChatRequest(
    string Question,
    int? TripId = null,
    int TopK = 5
);