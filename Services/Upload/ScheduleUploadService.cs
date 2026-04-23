using LocalRAG.DTOs.UploadModels;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LocalRAG.Services.Upload;

/// <summary>기존 일정 충돌 감지용 내부 전달 타입</summary>
internal sealed record ExistingScheduleEntry
{
    public DateTime ScheduleDate { get; init; }
    public string? StartTime { get; init; }
    public string Title { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
}

/// <summary>
/// 일정 업로드 서비스
/// Excel 형식 (2행부터 데이터 시작, 1행은 헤더):
/// - A열: 날짜 (2025-11-17)
/// - B열: 시작시간 (09:00)
/// - C열: 종료시간 (11:30)
/// - D열: 장소 (호텔로비)
/// - E열: 지도링크 (선택, URL)
/// - F열: 일정명 (필수)
/// - G열: 내용 (선택, 여러 줄 가능)
/// - H열: 노출_개인정보 (선택, 쉼표 구분 속성 키 예: "룸번호,룸메이트")
///
/// 멀티시트 지원: 시트 1개 = ScheduleTemplate 1개 (시트명 = 코스명)
/// 마지막 행이 ※로 시작하면 안내 행으로 스킵
/// </summary>
public class ScheduleUploadService : IScheduleTemplateUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleUploadService> _logger;

    public ScheduleUploadService(
        IUnitOfWork unitOfWork,
        ILogger<ScheduleUploadService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    // ============================================================
    // Preview
    // ============================================================

    public async Task<ScheduleTemplatePreviewResult> PreviewScheduleTemplatesAsync(int conventionId, Stream excelStream)
    {
        var result = new ScheduleTemplatePreviewResult();

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

        int currentYear = convention.StartDate?.Year ?? DateTime.Now.Year;

        // 기존 일정 충돌 감지용 인덱스 (모든 시트 공통)
        var existingList = await _unitOfWork.ScheduleItems.Query
            .Where(si => si.ScheduleTemplate.ConventionId == conventionId)
            .Select(si => new ExistingScheduleEntry
            {
                ScheduleDate = si.ScheduleDate,
                StartTime = si.StartTime,
                Title = si.Title,
                CourseName = si.ScheduleTemplate.CourseName
            })
            .ToListAsync();

        var existingIndex = existingList
            .GroupBy(e => (e.ScheduleDate.Date, e.StartTime ?? ""))
            .ToDictionary(
                g => g.Key,
                g => (g.First().ScheduleDate, g.First().StartTime, g.First().Title, g.First().CourseName));

        // 모든 시트 순회
        foreach (var sheet in package.Workbook.Worksheets)
        {
            if (sheet.Dimension == null) continue;

            var sheetPreview = ParseSheetPreview(sheet, existingIndex, currentYear);
            result.Sheets.Add(sheetPreview);

            // 집계
            result.TotalRows += sheetPreview.TotalRows;
            result.ValidRows += sheetPreview.ValidRows;
            result.WarningRows += sheetPreview.WarningRows;
            result.SkippedRows += sheetPreview.SkippedRows;
            result.ConflictRows += sheetPreview.ConflictRows;
            result.Items.AddRange(sheetPreview.Items);
        }

        return result;
    }

    private SheetPreview ParseSheetPreview(
        ExcelWorksheet sheet,
        Dictionary<(DateTime, string), (DateTime ScheduleDate, string? StartTime, string Title, string CourseName)> existingIndex,
        int currentYear)
    {
        var preview = new SheetPreview
        {
            SheetName = sheet.Name,
        };

        int rowCount = sheet.Dimension.Rows;
        preview.TotalRows = rowCount - 1; // 헤더 제외

        DateTime? lastDate = null;

        for (int row = 2; row <= rowCount; row++)
        {
            var dateCell = sheet.Cells[row, 1];
            var startTimeCell = sheet.Cells[row, 2];
            var endTimeCell = sheet.Cells[row, 3];
            var locationText = sheet.Cells[row, 4].Text?.Trim();
            var mapUrl = sheet.Cells[row, 5].Text?.Trim();
            var titleText = sheet.Cells[row, 6].Text?.Trim();
            var contentText = sheet.Cells[row, 7].Value?.ToString();
            var visibleAttrs = sheet.Cells[row, 8].Text?.Trim();

            var dateText = dateCell.Text?.Trim();
            var startTimeText = startTimeCell.Text?.Trim();
            var endTimeText = endTimeCell.Text?.Trim();

            // 완전히 빈 행 스킵
            if (string.IsNullOrEmpty(dateText) && string.IsNullOrEmpty(startTimeText)
                && string.IsNullOrEmpty(titleText) && string.IsNullOrEmpty(locationText))
            {
                preview.TotalRows--;
                continue;
            }

            // ※로 시작하는 안내 행 스킵
            if (!string.IsNullOrEmpty(dateText) && dateText.StartsWith('※'))
            {
                preview.TotalRows--;
                continue;
            }

            var item = new SchedulePreviewItem
            {
                Row = row,
                Location = locationText,
                MapUrl = string.IsNullOrEmpty(mapUrl) ? null : mapUrl,
                Title = titleText,
                Content = contentText,
                VisibleAttributes = string.IsNullOrEmpty(visibleAttrs) ? null : visibleAttrs,
            };

            // 날짜 파싱 (없으면 이전 행 날짜 상속)
            DateTime? scheduleDate = ParseDate(dateCell, dateText, currentYear);
            var warnings = new List<string>();

            if (scheduleDate == null)
            {
                if (lastDate.HasValue)
                {
                    scheduleDate = lastDate;
                    warnings.Add("날짜 누락 → 이전 행 날짜 상속");
                }
                else
                {
                    item.Status = "skipped";
                    item.Message = $"날짜 형식을 인식할 수 없습니다. ({dateText})";
                    item.Date = dateText;
                    preview.SkippedRows++;
                    preview.Items.Add(item);
                    continue;
                }
            }
            lastDate = scheduleDate;
            item.Date = scheduleDate.Value.ToString("yyyy-MM-dd");

            // 필수 제목 — 없으면 장소를 제목으로 대체
            if (string.IsNullOrEmpty(titleText))
            {
                if (!string.IsNullOrEmpty(locationText))
                {
                    item.Title = locationText;
                    warnings.Add("일정명 누락 → 장소를 제목으로 대체");
                }
                else
                {
                    item.Status = "skipped";
                    item.Message = "일정명과 장소가 모두 비어있습니다.";
                    preview.SkippedRows++;
                    preview.Items.Add(item);
                    continue;
                }
            }

            // 시작시간 파싱
            var startTimeStr = ParseTime(startTimeCell, startTimeText);
            if (string.IsNullOrEmpty(startTimeStr))
            {
                warnings.Add("시작시간 미정");
            }
            item.StartTime = startTimeStr;

            // 종료시간 파싱 (선택)
            if (!string.IsNullOrEmpty(endTimeText))
            {
                var endTimeStr = ParseTime(endTimeCell, endTimeText);
                if (endTimeStr == null)
                    warnings.Add($"종료시간 인식 실패 ({endTimeText})");
                item.EndTime = endTimeStr;
            }

            // 충돌 감지
            if (!string.IsNullOrEmpty(startTimeStr))
            {
                var key = (scheduleDate.Value.Date, startTimeStr);
                if (existingIndex.TryGetValue(key, out var conflict))
                {
                    item.HasConflict = true;
                    item.ConflictDetail = $"기존 [{conflict.CourseName}] {conflict.Title}";
                    preview.ConflictRows++;
                }
            }

            if (warnings.Count > 0)
            {
                item.Status = "warning";
                item.Message = string.Join(", ", warnings);
                preview.WarningRows++;
            }
            else
            {
                preview.ValidRows++;
            }
            preview.Items.Add(item);
        }

        return preview;
    }

    // ============================================================
    // Confirm — 단일 시트 (기존 호환)
    // ============================================================

    public async Task<ScheduleTemplateUploadResult> ConfirmScheduleTemplatesAsync(int conventionId, ScheduleTemplateConfirmRequest request)
    {
        var result = new ScheduleTemplateUploadResult();

        try
        {
            var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
            if (convention == null)
            {
                result.Errors.Add($"Convention {conventionId}를 찾을 수 없습니다.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.CourseName))
            {
                result.Errors.Add("코스명을 입력해주세요.");
                return result;
            }

            var itemsToSave = request.Items.Where(i => i.Status != "skipped").ToList();
            if (itemsToSave.Count == 0)
            {
                result.Errors.Add("저장할 일정이 없습니다.");
                return result;
            }

            var maxOrderNum = await _unitOfWork.ScheduleTemplates.Query
                .Where(st => st.ConventionId == conventionId)
                .MaxAsync(st => (int?)st.OrderNum) ?? 0;

            var scheduleTemplate = new ScheduleTemplate
            {
                ConventionId = conventionId,
                CourseName = request.CourseName.Trim(),
                Description = request.Description ?? "Excel 일정 업로드",
                OrderNum = maxOrderNum + 1,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ScheduleTemplates.AddAsync(scheduleTemplate);

            var scheduleItems = BuildScheduleItems(scheduleTemplate, itemsToSave, convention);

            if (scheduleItems.Count == 0)
            {
                result.Errors.Add("저장 가능한 일정이 없습니다. (행사 기간 외 또는 데이터 오류)");
                return result;
            }

            scheduleTemplate.ScheduleItems = scheduleItems;
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.TemplatesCreated = 1;
            result.ItemsCreated = scheduleItems.Count;
            AppendCreatedActions(result, scheduleItems);

            _logger.LogInformation("Schedule confirmed: course={Course}, items={Count}", request.CourseName, scheduleItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schedule confirm failed");
            result.Errors.Add($"저장 실패: {ex.Message}");
        }

        return result;
    }

    // ============================================================
    // Confirm — 멀티시트 일괄 저장
    // ============================================================

    public async Task<ScheduleTemplateUploadResult> ConfirmMultiSheetAsync(int conventionId, MultiSheetConfirmRequest request)
    {
        var result = new ScheduleTemplateUploadResult();

        try
        {
            var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
            if (convention == null)
            {
                result.Errors.Add($"Convention {conventionId}를 찾을 수 없습니다.");
                return result;
            }

            if (request.Sheets == null || request.Sheets.Count == 0)
            {
                result.Errors.Add("저장할 시트가 없습니다.");
                return result;
            }

            var maxOrderNum = await _unitOfWork.ScheduleTemplates.Query
                .Where(st => st.ConventionId == conventionId)
                .MaxAsync(st => (int?)st.OrderNum) ?? 0;

            foreach (var sheetReq in request.Sheets)
            {
                var courseName = sheetReq.CourseName?.Trim();
                if (string.IsNullOrEmpty(courseName))
                {
                    result.Warnings.Add($"코스명이 비어있어 스킵됩니다.");
                    continue;
                }

                var itemsToSave = sheetReq.Items.Where(i => i.Status != "skipped").ToList();
                if (itemsToSave.Count == 0)
                {
                    result.Warnings.Add($"[{courseName}] 저장할 일정이 없어 스킵됩니다.");
                    continue;
                }

                var scheduleTemplate = new ScheduleTemplate
                {
                    ConventionId = conventionId,
                    CourseName = courseName,
                    Description = sheetReq.Description ?? "Excel 일정 업로드",
                    OrderNum = ++maxOrderNum,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ScheduleTemplates.AddAsync(scheduleTemplate);

                var scheduleItems = BuildScheduleItems(scheduleTemplate, itemsToSave, convention);
                if (scheduleItems.Count == 0)
                {
                    result.Warnings.Add($"[{courseName}] 저장 가능한 일정이 없습니다. (행사 기간 외 또는 데이터 오류)");
                    continue;
                }

                scheduleTemplate.ScheduleItems = scheduleItems;
                result.TemplatesCreated++;
                result.ItemsCreated += scheduleItems.Count;
                AppendCreatedActions(result, scheduleItems);

                _logger.LogInformation("MultiSheet: course={Course}, items={Count}", courseName, scheduleItems.Count);
            }

            if (result.TemplatesCreated > 0)
            {
                await _unitOfWork.SaveChangesAsync();
                result.Success = true;
            }
            else
            {
                result.Errors.Add("저장된 일정 코스가 없습니다.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Multi-sheet schedule confirm failed");
            result.Errors.Add($"저장 실패: {ex.Message}");
        }

        return result;
    }

    // ============================================================
    // Legacy 단순 업로드 (첫 번째 시트만)
    // ============================================================

    public async Task<ScheduleTemplateUploadResult> UploadScheduleTemplatesAsync(int conventionId, Stream excelStream)
    {
        var result = new ScheduleTemplateUploadResult();

        try
        {
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

            int currentYear = convention.StartDate?.Year ?? DateTime.Now.Year;

            var maxOrderNum = await _unitOfWork.ScheduleTemplates.Query
                .Where(st => st.ConventionId == conventionId)
                .MaxAsync(st => (int?)st.OrderNum) ?? 0;

            foreach (var sheet in package.Workbook.Worksheets)
            {
                if (sheet.Dimension == null) continue;

                var courseName = sheet.Name;
                var rowCount = sheet.Dimension.Rows;
                var scheduleItems = new List<ScheduleItem>();
                int itemOrderNum = 0;

                var scheduleTemplate = new ScheduleTemplate
                {
                    ConventionId = conventionId,
                    CourseName = courseName,
                    Description = "Excel 일정 업로드",
                    OrderNum = ++maxOrderNum,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ScheduleTemplates.AddAsync(scheduleTemplate);

                for (int row = 2; row <= rowCount; row++)
                {
                    var dateCell = sheet.Cells[row, 1];
                    var startTimeCell = sheet.Cells[row, 2];
                    var endTimeCell = sheet.Cells[row, 3];
                    var locationText = sheet.Cells[row, 4].Text?.Trim();
                    var mapUrl = sheet.Cells[row, 5].Text?.Trim();
                    var titleText = sheet.Cells[row, 6].Text?.Trim();
                    var contentText = sheet.Cells[row, 7].Value?.ToString();
                    var visibleAttrs = sheet.Cells[row, 8].Text?.Trim();

                    var dateText = dateCell.Text?.Trim();
                    var startTimeText = startTimeCell.Text?.Trim();
                    var endTimeText = endTimeCell.Text?.Trim();

                    // 빈 행 스킵
                    if (string.IsNullOrEmpty(dateText) && string.IsNullOrEmpty(startTimeText) && string.IsNullOrEmpty(titleText))
                        continue;

                    // ※ 안내 행 스킵
                    if (!string.IsNullOrEmpty(dateText) && dateText.StartsWith('※'))
                        continue;

                    // 필수 필드 확인
                    if (string.IsNullOrEmpty(dateText) || string.IsNullOrEmpty(titleText))
                    {
                        result.Warnings.Add($"[{courseName}] Row {row}: 필수 항목 누락 (날짜·일정명 필수)");
                        continue;
                    }

                    DateTime? scheduleDate = ParseDate(dateCell, dateText, currentYear);
                    if (scheduleDate == null)
                    {
                        result.Warnings.Add($"[{courseName}] Row {row}: 날짜 형식 인식 불가 ({dateText})");
                        continue;
                    }

                    string? startTimeStr = ParseTime(startTimeCell, startTimeText);
                    if (string.IsNullOrEmpty(startTimeStr))
                    {
                        result.Warnings.Add($"[{courseName}] Row {row}: 시작시간 형식 인식 불가 ({startTimeText})");
                        continue;
                    }

                    string? endTimeStr = null;
                    if (!string.IsNullOrEmpty(endTimeText))
                    {
                        endTimeStr = ParseTime(endTimeCell, endTimeText);
                        if (endTimeStr == null)
                            result.Warnings.Add($"[{courseName}] Row {row}: 종료시간 형식 인식 불가 ({endTimeText})");
                    }

                    scheduleItems.Add(new ScheduleItem
                    {
                        ScheduleTemplate = scheduleTemplate,
                        ScheduleDate = scheduleDate.Value,
                        StartTime = startTimeStr,
                        EndTime = endTimeStr,
                        Location = string.IsNullOrEmpty(locationText) ? null : locationText,
                        MapUrl = string.IsNullOrEmpty(mapUrl) ? null : mapUrl,
                        Title = titleText,
                        Content = contentText,
                        VisibleAttributes = string.IsNullOrEmpty(visibleAttrs) ? null : visibleAttrs,
                        OrderNum = itemOrderNum++,
                        CreatedAt = DateTime.UtcNow
                    });

                    _logger.LogDebug("Created schedule item: {Title} at {Date} {Time}", titleText, scheduleDate, startTimeStr);
                }

                scheduleTemplate.ScheduleItems = scheduleItems;
                result.TemplatesCreated++;
                result.ItemsCreated += scheduleItems.Count;
                AppendCreatedActions(result, scheduleItems);
            }

            if (result.TemplatesCreated > 0)
            {
                await _unitOfWork.SaveChangesAsync();
                result.Success = true;
                _logger.LogInformation("Schedule upload completed: {Templates} templates, {Items} items", result.TemplatesCreated, result.ItemsCreated);
            }
            else
            {
                result.Errors.Add("저장된 일정 코스가 없습니다.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schedule upload failed");
            result.Errors.Add($"업로드 실패: {ex.Message}");
            if (ex.InnerException != null)
                result.Errors.Add($"상세: {ex.InnerException.Message}");
        }

        return result;
    }

    // ============================================================
    // 공통 헬퍼
    // ============================================================

    /// <summary>
    /// SchedulePreviewItem 목록에서 ScheduleItem 엔티티 생성 (서버 재검증 포함)
    /// </summary>
    private List<ScheduleItem> BuildScheduleItems(
        ScheduleTemplate scheduleTemplate,
        List<SchedulePreviewItem> previewItems,
        LocalRAG.Entities.Convention convention)
    {
        var items = new List<ScheduleItem>();
        int orderNum = 0;

        foreach (var item in previewItems)
        {
            if (!DateTime.TryParse(item.Date, out var date)) continue;

            var title = item.Title?.Trim();
            if (string.IsNullOrEmpty(title)) continue;

            // 날짜 범위 검증 (±7일 여유)
            if (convention.StartDate.HasValue && convention.EndDate.HasValue)
            {
                var min = convention.StartDate.Value.AddDays(-7);
                var max = convention.EndDate.Value.AddDays(7);
                if (date < min || date > max)
                {
                    _logger.LogWarning("Schedule item date out of range: {Date}, Title: {Title}", date, title);
                    continue;
                }
            }

            string startTime = string.Empty;
            if (!string.IsNullOrEmpty(item.StartTime) && TimeSpan.TryParse(item.StartTime, out _))
                startTime = item.StartTime;

            string? endTime = null;
            if (!string.IsNullOrEmpty(item.EndTime) && TimeSpan.TryParse(item.EndTime, out _))
                endTime = item.EndTime;

            items.Add(new ScheduleItem
            {
                ScheduleTemplate = scheduleTemplate,
                ScheduleDate = date,
                StartTime = startTime,
                EndTime = endTime,
                Title = title,
                Content = item.Content?.Length > 4000 ? item.Content[..4000] : item.Content,
                Location = item.Location?.Length > 500 ? item.Location[..500] : item.Location,
                MapUrl = string.IsNullOrEmpty(item.MapUrl) ? null : item.MapUrl,
                VisibleAttributes = string.IsNullOrEmpty(item.VisibleAttributes) ? null : item.VisibleAttributes,
                OrderNum = orderNum++,
                CreatedAt = DateTime.UtcNow
            });
        }

        return items;
    }

    private static void AppendCreatedActions(ScheduleTemplateUploadResult result, List<ScheduleItem> items)
    {
        foreach (var item in items)
        {
            DateTime? dt = item.ScheduleDate;
            if (!string.IsNullOrEmpty(item.StartTime) && TimeSpan.TryParse(item.StartTime, out var ts))
                dt = item.ScheduleDate.Add(ts);

            result.CreatedActions.Add(new ConventionActionInfo
            {
                Id = item.Id,
                Title = item.Title,
                ScheduleDateTime = dt
            });
        }
    }

    /// <summary>
    /// 날짜 셀 파싱 — 4가지 형식 지원
    /// 1) Excel DateTime 값
    /// 2) Excel 시리얼 숫자 (45978 → 2025-11-17)
    /// 3) 텍스트: "2025-11-17"
    /// 4) 한글: "4월 9일"
    /// </summary>
    private DateTime? ParseDate(OfficeOpenXml.ExcelRange cell, string? text, int fallbackYear)
    {
        var value = cell.Value;
        if (value is DateTime dt)
            return dt.Date;

        double? serialDouble = value switch
        {
            double dv => dv,
            decimal dec => (double)dec,
            float f => f,
            int i => i,
            long l => l,
            _ => null
        };

        if (serialDouble.HasValue && serialDouble.Value > 1000)
        {
            try { return DateTime.FromOADate(serialDouble.Value).Date; } catch { }
        }

        if (string.IsNullOrWhiteSpace(text)) return null;

        if (DateTime.TryParse(text, out var parsed))
            return parsed.Date;

        // 한글 날짜 "4월 9일"
        var match = System.Text.RegularExpressions.Regex.Match(text, @"(\d{1,2})\s*월\s*(\d{1,2})\s*일");
        if (match.Success)
        {
            int month = int.Parse(match.Groups[1].Value);
            int day = int.Parse(match.Groups[2].Value);
            try { return new DateTime(fallbackYear, month, day); } catch { }
        }

        // Excel 시리얼 텍스트 ("45978")
        if (double.TryParse(text, out var serial) && serial > 1000)
        {
            try { return DateTime.FromOADate(serial).Date; } catch { }
        }

        return null;
    }

    /// <summary>
    /// 시간 셀 파싱 — 3가지 형식 지원
    /// 1) Excel 시간 시리얼 (0.375 → 09:00)
    /// 2) 텍스트 "09:00", "9:00"
    /// 3) 시리얼 텍스트 ("0.375")
    /// </summary>
    private string? ParseTime(OfficeOpenXml.ExcelRange cell, string? text)
    {
        var value = cell.Value;
        double serial = 0;
        if (value is double d) serial = d;
        else if (value is decimal dec) serial = (double)dec;

        if (serial > 0 && serial < 1)
        {
            var totalMinutes = (int)Math.Round(serial * 24 * 60);
            return $"{totalMinutes / 60:D2}:{totalMinutes % 60:D2}";
        }

        if (string.IsNullOrWhiteSpace(text)) return null;

        if (TimeSpan.TryParse(text, out var ts))
            return $"{ts.Hours:D2}:{ts.Minutes:D2}";

        if (double.TryParse(text, out var serialText) && serialText > 0 && serialText < 1)
        {
            var totalMinutes = (int)Math.Round(serialText * 24 * 60);
            return $"{totalMinutes / 60:D2}:{totalMinutes % 60:D2}";
        }

        return null;
    }
}
