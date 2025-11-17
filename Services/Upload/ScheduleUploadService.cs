using LocalRAG.DTOs.UploadModels;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 일정 업로드 서비스
/// Excel 형식 (2행부터 데이터 시작, 1행은 헤더):
/// - A열: 날짜 (2025-11-17)
/// - B열: 시작시간 (09:00)
/// - C열: 종료시간 (11:30)
/// - D열: 장소 (호텔로비)
/// - E열: 일정명 (개인정비)
/// - F열: 메모 (상세 설명, 여러 줄 가능)
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
                    // A~F열 읽기
                    var dateText = sheet.Cells[row, 1].Text?.Trim(); // A: 날짜 (2025-11-17)
                    var startTimeText = sheet.Cells[row, 2].Text?.Trim(); // B: 시작시간 (09:00)
                    var endTimeText = sheet.Cells[row, 3].Text?.Trim(); // C: 종료시간 (11:30)
                    var locationText = sheet.Cells[row, 4].Text?.Trim(); // D: 장소 (호텔로비)
                    var titleText = sheet.Cells[row, 5].Text?.Trim(); // E: 일정명 (개인정비)
                    var memoText = sheet.Cells[row, 6].Value?.ToString(); // F: 메모 (줄바꿈 보존)

                    // 필수 필드 확인 (날짜, 시작시간, 일정명)
                    if (string.IsNullOrEmpty(dateText) || string.IsNullOrEmpty(startTimeText) || string.IsNullOrEmpty(titleText))
                    {
                        // 모든 필드가 비어있으면 빈 행으로 간주하여 건너뛰기
                        if (string.IsNullOrEmpty(dateText) && string.IsNullOrEmpty(startTimeText) && string.IsNullOrEmpty(titleText))
                        {
                            continue;
                        }

                        result.Warnings.Add($"Row {row}: 필수 항목이 누락되었습니다. (날짜, 시작시간, 일정명은 필수입니다.)");
                        continue;
                    }

                    // 날짜 파싱
                    DateTime scheduleDate;
                    try
                    {
                        scheduleDate = DateTime.Parse(dateText);
                    }
                    catch (FormatException)
                    {
                        result.Warnings.Add($"Row {row}: 잘못된 날짜 형식입니다. ({dateText}) - 올바른 형식: 2025-11-17");
                        continue;
                    }

                    // 시작시간 검증 (HH:mm 형식)
                    if (!TimeSpan.TryParse(startTimeText, out _))
                    {
                        result.Warnings.Add($"Row {row}: 잘못된 시작시간 형식입니다. ({startTimeText}) - 올바른 형식: 09:00");
                        continue;
                    }

                    // 종료시간 검증 (선택사항)
                    if (!string.IsNullOrEmpty(endTimeText) && !TimeSpan.TryParse(endTimeText, out _))
                    {
                        result.Warnings.Add($"Row {row}: 잘못된 종료시간 형식입니다. ({endTimeText}) - 올바른 형식: 11:30");
                        continue;
                    }

                    // ScheduleItem 생성 (개별 일정)
                    var scheduleItem = new ScheduleItem
                    {
                        ScheduleTemplate = scheduleTemplate, // Navigation property 설정
                        ScheduleDate = scheduleDate,
                        StartTime = startTimeText,
                        EndTime = string.IsNullOrEmpty(endTimeText) ? null : endTimeText,
                        Title = titleText,
                        Content = memoText,
                        Location = string.IsNullOrEmpty(locationText) ? null : locationText,
                        OrderNum = itemOrderNum,
                        CreatedAt = DateTime.UtcNow
                    };

                    scheduleItems.Add(scheduleItem);
                    itemOrderNum++;

                    _logger.LogDebug("Created schedule item: {Title} at {Date} {Time}", titleText, scheduleDate, startTimeText);
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
}
