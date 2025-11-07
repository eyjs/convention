using LocalRAG.DTOs.UploadModels;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 일정 업로드 서비스
/// Excel 형식:
/// - A열: "월/일(요일)_일정명_시:분" (예: "11/03(일)_조식_07:30")
/// - B열: 상세 내용 (엑셀 내부 줄바꿈 또는 <br/> 태그)
///
/// ScheduleTemplate + ScheduleItem 구조로 저장
/// 한 번의 업로드 = 하나의 ScheduleTemplate + 여러 ScheduleItems
/// </summary>
public class ScheduleUploadService : IScheduleTemplateUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ConventionDbContext _context;
    private readonly ILogger<ScheduleUploadService> _logger;

    public ScheduleUploadService(
        IUnitOfWork unitOfWork,
        ConventionDbContext context,
        ILogger<ScheduleUploadService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
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

            _logger.LogInformation("Processing {RowCount} schedule items from Excel", rowCount);

            try
            {
                // 현재 Convention의 최대 OrderNum 조회
                var maxOrderNum = await _context.ScheduleTemplates
                    .Where(st => st.ConventionId == conventionId)
                    .MaxAsync(st => (int?)st.OrderNum) ?? 0;

                // 하나의 ScheduleTemplate 생성 (코스/템플릿)
                var scheduleTemplate = new ScheduleTemplate
                {
                    ConventionId = conventionId,
                    CourseName = $"업로드_{DateTime.Now:yyyy-MM-dd_HHmmss}",
                    Description = "Excel 일정 업로드",
                    OrderNum = maxOrderNum + 1,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ScheduleTemplates.Add(scheduleTemplate);

                // ScheduleItems 목록
                var scheduleItems = new List<ScheduleItem>();
                int itemOrderNum = 0;

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

                    // B열 처리: 엑셀 내부 줄바꿈(\n)은 그대로 유지
                    string? processedContent = null;
                    if (!string.IsNullOrEmpty(detailContent))
                    {
                        processedContent = detailContent;
                    }

                    // DateTime 생성 (현재 연도 사용)
                    var currentYear = DateTime.Now.Year;
                    DateTime scheduleDate;

                    try
                    {
                        scheduleDate = new DateTime(
                            currentYear,
                            parsed.Month,
                            parsed.Day);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        result.Warnings.Add($"Row {row}: 잘못된 날짜/시간입니다. ({parsed.Month}/{parsed.Day} {parsed.Hour}:{parsed.Minute})");
                        continue;
                    }

                    // ScheduleItem 생성 (개별 일정)
                    var scheduleItem = new ScheduleItem
                    {
                        ScheduleTemplate = scheduleTemplate, // Navigation property 설정
                        ScheduleDate = scheduleDate,
                        StartTime = $"{parsed.Hour:D2}:{parsed.Minute:D2}",
                        EndTime = null, // 종료시간은 별도 지정 가능
                        Title = parsed.Title,
                        Content = processedContent,
                        Location = null,
                        OrderNum = itemOrderNum,
                        CreatedAt = DateTime.UtcNow
                    };

                    scheduleItems.Add(scheduleItem);
                    itemOrderNum++;

                    _logger.LogDebug("Created schedule item: {Title} at {Date} {Time}", parsed.Title, scheduleDate, scheduleItem.StartTime);
                }

                // ScheduleTemplate의 ScheduleItems 컬렉션에 추가
                scheduleTemplate.ScheduleItems = scheduleItems;

                // 한 번의 SaveChanges로 모든 변경사항 커밋 (암시적 트랜잭션)
                await _context.SaveChangesAsync();

                result.Success = true;
                result.TemplatesCreated = 1; // 템플릿 1개 생성
                result.ItemsCreated = scheduleItems.Count; // 실제 일정 항목 수

                // 결과 정보 추가 (화면 표시용)
                foreach (var item in scheduleItems)
                {
                    result.CreatedActions.Add(new ConventionActionInfo
                    {
                        Id = item.Id,
                        Title = item.Title,
                        ScheduleDateTime = item.ScheduleDate.Add(TimeSpan.Parse(item.StartTime))
                    });
                }

                _logger.LogInformation("Schedule upload completed: 1 template with {Count} items created", scheduleItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Schedule upload failed during processing");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schedule upload failed");
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
