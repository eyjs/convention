using LocalRAG.DTOs.UploadModels;
using LocalRAG.Entities.Action;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 일정 템플릿 업로드 서비스
/// Excel 형식:
/// - A열: "월/일(요일)_일정명_시:분" (예: "11/03(일)_조식_07:30")
/// - B열: 상세 내용 (엑셀 내부 줄바꿈 또는 <br/> 태그)
///
/// ConventionAction으로 생성되며, ActionType은 "SCHEDULE_{자동증가번호}" 형식
/// </summary>
public class ScheduleTemplateUploadService : IScheduleTemplateUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleTemplateUploadService> _logger;

    public ScheduleTemplateUploadService(
        IUnitOfWork unitOfWork,
        ILogger<ScheduleTemplateUploadService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<ScheduleTemplateUploadResult> UploadScheduleTemplatesAsync(int conventionId, Stream excelStream)
    {
        var result = new ScheduleTemplateUploadResult();

        try
        {
            // Convention 존재 확인
            var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
            if (convention == null)
            {
                result.Errors.Add($"Convention {conventionId}를 찾을 수 없습니다.");
                return result;
            }

            using var package = new ExcelPackage(excelStream);

            if (package.Workbook.Worksheets.Count < 1)
            {
                result.Errors.Add("Excel 파일에 시트가 없습니다.");
                return result;
            }

            var sheet = package.Workbook.Worksheets[0];

            if (sheet.Dimension == null)
            {
                result.Errors.Add("시트가 비어있습니다.");
                return result;
            }

            var rowCount = sheet.Dimension.Rows;

            _logger.LogInformation("Processing {RowCount} schedule templates from Excel", rowCount);

            // 트랜잭션으로 처리
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 기존 SCHEDULE_ ActionType의 최대 번호 찾기
                var existingActions = await _unitOfWork.ConventionActions
                    .FindAsync(a => a.ConventionId == conventionId && a.ActionType.StartsWith("SCHEDULE_"));

                int maxScheduleNumber = 0;
                foreach (var action in existingActions)
                {
                    var match = Regex.Match(action.ActionType, @"SCHEDULE_(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
                    {
                        if (num > maxScheduleNumber)
                            maxScheduleNumber = num;
                    }
                }

                int scheduleCounter = maxScheduleNumber + 1;
                int orderNum = 0;

                for (int row = 2; row <= rowCount; row++) // 1행은 헤더
                {
                    var scheduleHeader = sheet.Cells[row, 1].Text?.Trim();
                    var detailContent = sheet.Cells[row, 2].Value?.ToString(); // Value로 읽어서 줄바꿈 보존

                    if (string.IsNullOrEmpty(scheduleHeader))
                    {
                        continue; // 빈 행 건너뛰기
                    }

                    // A열 파싱: "11/03(일)_조식_07:30"
                    var parsed = ParseScheduleHeader(scheduleHeader);
                    if (parsed == null)
                    {
                        result.Warnings.Add($"Row {row}: 일정 헤더 형식이 올바르지 않습니다. ({scheduleHeader})");
                        continue;
                    }

                    // B열 처리: 엑셀 내부 줄바꿈(\n) → HTML 줄바꿈(<br/>)으로 변환
                    string? processedContent = null;
                    if (!string.IsNullOrEmpty(detailContent))
                    {
                        processedContent = detailContent
                            .Replace("\r\n", "<br/>")
                            .Replace("\n", "<br/>")
                            .Replace("\r", "<br/>");
                    }

                    // DateTime 생성 (현재 연도 사용)
                    var currentYear = DateTime.Now.Year;
                    DateTime scheduleDateTime;
                    try
                    {
                        scheduleDateTime = new DateTime(
                            currentYear,
                            parsed.Month,
                            parsed.Day,
                            parsed.Hour,
                            parsed.Minute,
                            0);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        result.Warnings.Add($"Row {row}: 잘못된 날짜/시간입니다. ({parsed.Month}/{parsed.Day} {parsed.Hour}:{parsed.Minute})");
                        continue;
                    }

                    // ConventionAction 생성
                    var actionType = $"SCHEDULE_{scheduleCounter:D4}";
                    var title = parsed.Title;

                    var conventionAction = new ConventionAction
                    {
                        ConventionId = conventionId,
                        ActionType = actionType,
                        Title = title,
                        Description = processedContent,
                        MapsTo = "SCHEDULE", // 일정 타입
                        Deadline = scheduleDateTime, // 일정 시간을 Deadline으로 설정
                        ConfigJson = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            scheduleDateTime = scheduleDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            originalHeader = scheduleHeader
                        }),
                        IsActive = true,
                        OrderNum = orderNum,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.ConventionActions.AddAsync(conventionAction);
                    result.TemplatesCreated++;
                    scheduleCounter++;
                    orderNum++;

                    result.CreatedActions.Add(new ConventionActionInfo
                    {
                        Id = 0, // ID는 SaveChanges 후 설정됨
                        ActionType = actionType,
                        Title = title,
                        ScheduleDateTime = scheduleDateTime
                    });

                    _logger.LogDebug("Created schedule template: {Title} at {DateTime}", title, scheduleDateTime);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.Success = true;
                result.ItemsCreated = result.TemplatesCreated;

                _logger.LogInformation("Schedule template upload completed: {Count} templates created", result.TemplatesCreated);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Transaction failed during schedule template upload");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schedule template upload failed");
            result.Errors.Add($"업로드 실패: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.Errors.Add($"상세: {ex.InnerException.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// 일정 헤더 파싱
    /// 형식: "월/일(요일)_일정명_시:분" (예: "11/03(일)_조식_07:30")
    /// </summary>
    private ScheduleHeaderParsed? ParseScheduleHeader(string header)
    {
        // 정규식: "MM/DD(요일)_제목_HH:MM"
        var pattern = @"(\d{1,2})/(\d{1,2})\((.+?)\)_(.+?)_(\d{1,2}):(\d{2})";
        var match = Regex.Match(header, pattern);

        if (!match.Success)
        {
            return null;
        }

        return new ScheduleHeaderParsed
        {
            Month = int.Parse(match.Groups[1].Value),
            Day = int.Parse(match.Groups[2].Value),
            DayOfWeek = match.Groups[3].Value,
            Title = match.Groups[4].Value.Trim(),
            Hour = int.Parse(match.Groups[5].Value),
            Minute = int.Parse(match.Groups[6].Value)
        };
    }

    private class ScheduleHeaderParsed
    {
        public int Month { get; set; }
        public int Day { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Hour { get; set; }
        public int Minute { get; set; }
    }
}
