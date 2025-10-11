using LocalRAG.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")]  // 임시 주석 처리 - 테스트용
public class ScheduleUploadController : ControllerBase
{
    private readonly IScheduleUploadService _uploadService;
    private readonly ILogger<ScheduleUploadController> _logger;

    public ScheduleUploadController(
        IScheduleUploadService uploadService,
        ILogger<ScheduleUploadController> logger)
    {
        _uploadService = uploadService;
        _logger = logger;
    }

    [HttpPost("conventions/{conventionId}/upload")]
    public async Task<IActionResult> UploadGuestsAndSchedules(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "파일이 없습니다." });

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "엑셀 파일(.xlsx)만 업로드 가능합니다." });

        try
        {
            _logger.LogInformation("Starting upload for convention {ConventionId}, file: {FileName}", 
                conventionId, file.FileName);

            using var stream = file.OpenReadStream();
            var result = await _uploadService.UploadGuestsAndSchedulesAsync(conventionId, stream);

            if (!result.Success)
            {
                _logger.LogWarning("Upload failed with errors: {Errors}", string.Join(", ", result.Errors));
                return BadRequest(new
                {
                    message = "업로드 실패",
                    errors = result.Errors
                });
            }

            _logger.LogInformation("Upload succeeded: {Guests} guests, {Schedules} schedules, {Assignments} assignments",
                result.GuestsCreated, result.TotalSchedules, result.ScheduleAssignments);

            return Ok(new
            {
                message = "업로드 완료",
                totalSchedules = result.TotalSchedules,
                guestsCreated = result.GuestsCreated,
                scheduleAssignments = result.ScheduleAssignments,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload error for convention {ConventionId}", conventionId);
            return StatusCode(500, new { 
                message = "업로드 중 오류가 발생했습니다.",
                error = ex.Message,
                details = ex.InnerException?.Message
            });
        }
    }
}
