using LocalRAG.DTOs.UploadModels;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
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
    private readonly ILogger<ScheduleUploadService> _logger;

    public ScheduleUploadService(
        IUnitOfWork unitOfWork,
        ILogger<ScheduleUploadService> logger)
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

            _logger.LogInformation("Processing {RowCount} schedule items from Excel", rowCount);

            try
            {
                // 현재 Convention의 최대 OrderNum 조회
                var maxOrderNum = await _unitOfWork.ScheduleTemplates.Query
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

                await _unitOfWork.ScheduleTemplates.AddAsync(scheduleTemplate);

                // ScheduleItems 목록
                var scheduleItems = new List<ScheduleItem>();
                int itemOrderNum = 0;

                // 현재 연도 (한글 날짜 파싱용: "4월 9일" → 2026-04-09)
                int currentYear = convention.StartDate?.Year ?? DateTime.Now.Year;

                for (int row = 2; row <= rowCount; row++) // 1행은 헤더
                {
                    // A~F열 읽기 — Value(원본)와 Text(서식 적용) 모두 활용
                    var dateCell = sheet.Cells[row, 1];
                    var startTimeCell = sheet.Cells[row, 2];
                    var endTimeCell = sheet.Cells[row, 3];
                    var locationText = sheet.Cells[row, 4].Text?.Trim();
                    var titleText = sheet.Cells[row, 5].Text?.Trim();
                    var memoText = sheet.Cells[row, 6].Value?.ToString();

                    var dateText = dateCell.Text?.Trim();
                    var startTimeText = startTimeCell.Text?.Trim();
                    var endTimeText = endTimeCell.Text?.Trim();

                    // 빈 행은 건너뛰기 (모든 필수 필드가 비어있을 때)
                    if (string.IsNullOrEmpty(dateText) && string.IsNullOrEmpty(startTimeText) && string.IsNullOrEmpty(titleText))
                    {
                        continue;
                    }

                    // 필수 필드 확인
                    if (string.IsNullOrEmpty(dateText) || string.IsNullOrEmpty(titleText))
                    {
                        result.Warnings.Add($"Row {row}: 필수 항목이 누락되었습니다. (날짜, 일정명은 필수입니다.)");
                        continue;
                    }

                    // === 날짜 파싱 (3가지 형식 지원) ===
                    DateTime? scheduleDate = ParseDate(dateCell, dateText, currentYear);
                    if (scheduleDate == null)
                    {
                        result.Warnings.Add($"Row {row}: 날짜 형식을 인식할 수 없습니다. ({dateText})");
                        continue;
                    }

                    // === 시작시간 파싱 ===
                    string? startTimeStr = ParseTime(startTimeCell, startTimeText);
                    if (string.IsNullOrEmpty(startTimeStr))
                    {
                        result.Warnings.Add($"Row {row}: 시작시간 형식을 인식할 수 없습니다. ({startTimeText})");
                        continue;
                    }

                    // === 종료시간 파싱 (선택) ===
                    string? endTimeStr = null;
                    if (!string.IsNullOrEmpty(endTimeText))
                    {
                        endTimeStr = ParseTime(endTimeCell, endTimeText);
                        if (endTimeStr == null)
                        {
                            result.Warnings.Add($"Row {row}: 종료시간 형식을 인식할 수 없습니다. ({endTimeText})");
                            // 종료시간은 선택이므로 경고만 남기고 진행
                        }
                    }

                    // ScheduleItem 생성
                    var scheduleItem = new ScheduleItem
                    {
                        ScheduleTemplate = scheduleTemplate,
                        ScheduleDate = scheduleDate.Value,
                        StartTime = startTimeStr,
                        EndTime = endTimeStr,
                        Title = titleText,
                        Content = memoText,
                        Location = string.IsNullOrEmpty(locationText) ? null : locationText,
                        OrderNum = itemOrderNum,
                        CreatedAt = DateTime.UtcNow
                    };

                    scheduleItems.Add(scheduleItem);
                    itemOrderNum++;

                    _logger.LogDebug("Created schedule item: {Title} at {Date} {Time}", titleText, scheduleDate, startTimeStr);
                }

                // ScheduleTemplate의 ScheduleItems 컬렉션에 추가
                scheduleTemplate.ScheduleItems = scheduleItems;

                // 한 번의 SaveChanges로 모든 변경사항 커밋 (암시적 트랜잭션)
                await _unitOfWork.SaveChangesAsync();

                result.Success = true;
                result.TemplatesCreated = 1; // 템플릿 1개 생성
                result.ItemsCreated = scheduleItems.Count; // 실제 일정 항목 수

                // 결과 정보 추가 (화면 표시용)
                foreach (var item in scheduleItems)
                {
                    DateTime? scheduleDateTime = item.ScheduleDate;
                    if (!string.IsNullOrEmpty(item.StartTime) && TimeSpan.TryParse(item.StartTime, out var ts))
                    {
                        scheduleDateTime = item.ScheduleDate.Add(ts);
                    }
                    result.CreatedActions.Add(new ConventionActionInfo
                    {
                        Id = item.Id,
                        Title = item.Title,
                        ScheduleDateTime = scheduleDateTime
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

    // ============================================================
    // Preview / Confirm 방식 (신규)
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

        var sheet = package.Workbook.Worksheets[0];
        if (sheet.Dimension == null)
        {
            result.Errors.Add("시트가 비어있습니다.");
            return result;
        }

        int currentYear = convention.StartDate?.Year ?? DateTime.Now.Year;
        var rowCount = sheet.Dimension.Rows;
        result.TotalRows = rowCount - 1; // 헤더 제외

        // 기존 일정 조회 — 충돌 감지용 Dictionary 인덱싱 (O(1) 조회)
        var existingList = await _unitOfWork.ScheduleItems.Query
            .Where(si => si.ScheduleTemplate.ConventionId == conventionId)
            .Select(si => new { si.ScheduleDate, si.StartTime, si.Title, si.ScheduleTemplate.CourseName })
            .ToListAsync();

        var existingIndex = existingList
            .GroupBy(e => (e.ScheduleDate.Date, e.StartTime ?? ""))
            .ToDictionary(g => g.Key, g => g.First());

        // 이전 행에서 파싱된 날짜 — 다음 행의 날짜 누락 시 상속
        DateTime? lastDate = null;

        for (int row = 2; row <= rowCount; row++)
        {
            var dateCell = sheet.Cells[row, 1];
            var startTimeCell = sheet.Cells[row, 2];
            var endTimeCell = sheet.Cells[row, 3];
            var locationText = sheet.Cells[row, 4].Text?.Trim();
            var titleText = sheet.Cells[row, 5].Text?.Trim();
            var memoText = sheet.Cells[row, 6].Value?.ToString();

            var dateText = dateCell.Text?.Trim();
            var startTimeText = startTimeCell.Text?.Trim();
            var endTimeText = endTimeCell.Text?.Trim();

            // 완전히 빈 행은 스킵 (집계에 포함 X)
            if (string.IsNullOrEmpty(dateText) && string.IsNullOrEmpty(startTimeText) && string.IsNullOrEmpty(titleText) && string.IsNullOrEmpty(locationText))
            {
                result.TotalRows--;
                continue;
            }

            var item = new SchedulePreviewItem
            {
                Row = row,
                Location = locationText,
                Title = titleText,
                Content = memoText,
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
                    result.SkippedRows++;
                    result.Items.Add(item);
                    continue;
                }
            }
            lastDate = scheduleDate;
            item.Date = scheduleDate.Value.ToString("yyyy-MM-dd");

            // 필수 제목 — 없으면 장소를 제목으로 대체하여 살림
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
                    result.SkippedRows++;
                    result.Items.Add(item);
                    continue;
                }
            }

            // 시작시간 파싱 — 없으면 이전 행 종료시간 상속 또는 null 허용
            var startTimeStr = ParseTime(startTimeCell, startTimeText);
            if (string.IsNullOrEmpty(startTimeStr))
            {
                // 시작시간 누락 — 일정은 저장하되 "시간 미정"으로 warning
                warnings.Add("시작시간 미정");
            }
            item.StartTime = startTimeStr;

            // 종료시간 파싱 (선택)
            if (!string.IsNullOrEmpty(endTimeText))
            {
                var endTimeStr = ParseTime(endTimeCell, endTimeText);
                if (endTimeStr == null)
                {
                    warnings.Add($"종료시간 인식 실패 ({endTimeText})");
                }
                item.EndTime = endTimeStr;
            }

            // 충돌 감지: Dictionary 기반 O(1) 조회
            if (!string.IsNullOrEmpty(startTimeStr))
            {
                var key = (scheduleDate.Value.Date, startTimeStr);
                if (existingIndex.TryGetValue(key, out var conflict))
                {
                    item.HasConflict = true;
                    item.ConflictDetail = $"기존 [{conflict.CourseName}] {conflict.Title}";
                    result.ConflictRows++;
                }
            }

            // warning 집계
            if (warnings.Count > 0)
            {
                item.Status = "warning";
                item.Message = string.Join(", ", warnings);
                result.WarningRows++;
            }
            else
            {
                result.ValidRows++;
            }
            result.Items.Add(item);
        }

        return result;
    }

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

            // skipped 제외한 valid + warning 만 저장
            var itemsToSave = request.Items
                .Where(i => i.Status != "skipped")
                .ToList();

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

            var scheduleItems = new List<ScheduleItem>();
            int itemOrderNum = 0;
            foreach (var item in itemsToSave)
            {
                // 서버 재검증 — 클라이언트 변조 방지
                if (!DateTime.TryParse(item.Date, out var date)) continue;

                var title = item.Title?.Trim();
                if (string.IsNullOrEmpty(title)) continue;

                // 날짜가 행사 기간 밖이면 거부 (±7일 여유)
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

                // 시간 재검증
                string startTime = string.Empty;
                if (!string.IsNullOrEmpty(item.StartTime) && TimeSpan.TryParse(item.StartTime, out _))
                {
                    startTime = item.StartTime;
                }

                string? endTime = null;
                if (!string.IsNullOrEmpty(item.EndTime) && TimeSpan.TryParse(item.EndTime, out _))
                {
                    endTime = item.EndTime;
                }

                scheduleItems.Add(new ScheduleItem
                {
                    ScheduleTemplate = scheduleTemplate,
                    ScheduleDate = date,
                    StartTime = startTime,
                    EndTime = endTime,
                    Title = title,
                    Content = item.Content?.Length > 4000 ? item.Content.Substring(0, 4000) : item.Content,
                    Location = item.Location?.Length > 500 ? item.Location.Substring(0, 500) : item.Location,
                    OrderNum = itemOrderNum++,
                    CreatedAt = DateTime.UtcNow
                });
            }

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

            foreach (var item in scheduleItems)
            {
                DateTime? dt = item.ScheduleDate;
                if (!string.IsNullOrEmpty(item.StartTime) && TimeSpan.TryParse(item.StartTime, out var ts))
                {
                    dt = item.ScheduleDate.Add(ts);
                }
                result.CreatedActions.Add(new ConventionActionInfo
                {
                    Id = item.Id,
                    Title = item.Title,
                    ScheduleDateTime = dt
                });
            }

            _logger.LogInformation("Schedule confirmed: course={Course}, items={Count}", request.CourseName, scheduleItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schedule confirm failed");
            result.Errors.Add($"저장 실패: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 날짜 셀 파싱 — 3가지 형식 지원
    /// 1) Excel 시리얼 숫자 (45978 → 2025-11-17)
    /// 2) DateTime 값 (서식으로 저장된 경우)
    /// 3) 텍스트: "2025-11-17", "4월 9일", "11/17" 등
    /// </summary>
    private DateTime? ParseDate(OfficeOpenXml.ExcelRange cell, string? text, int fallbackYear)
    {
        // 1) 셀 값이 DateTime이거나 숫자(시리얼)인 경우
        var value = cell.Value;
        if (value is DateTime dt)
        {
            return dt.Date;
        }

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
            try
            {
                return DateTime.FromOADate(serialDouble.Value).Date;
            }
            catch { }
        }

        if (string.IsNullOrWhiteSpace(text)) return null;

        // 2) 표준 날짜 형식 시도
        if (DateTime.TryParse(text, out var parsed))
        {
            return parsed.Date;
        }

        // 3) 한글 날짜 "4월 9일" 형식
        var match = System.Text.RegularExpressions.Regex.Match(text, @"(\d{1,2})\s*월\s*(\d{1,2})\s*일");
        if (match.Success)
        {
            int month = int.Parse(match.Groups[1].Value);
            int day = int.Parse(match.Groups[2].Value);
            try
            {
                return new DateTime(fallbackYear, month, day);
            }
            catch { }
        }

        // 4) Excel 시리얼이 텍스트로 온 경우 ("45978")
        if (double.TryParse(text, out var serial) && serial > 1000)
        {
            try
            {
                return DateTime.FromOADate(serial).Date;
            }
            catch { }
        }

        return null;
    }

    /// <summary>
    /// 시간 셀 파싱 — 2가지 형식 지원
    /// 1) Excel 시간 시리얼 (0.375 → 09:00)
    /// 2) 텍스트 "09:00", "9:00", "21:00"
    /// </summary>
    private string? ParseTime(OfficeOpenXml.ExcelRange cell, string? text)
    {
        // 1) 셀 값이 숫자(시간 시리얼)인 경우
        var value = cell.Value;
        double serial = 0;
        if (value is double d) serial = d;
        else if (value is decimal dec) serial = (double)dec;

        if (serial > 0 && serial < 1)
        {
            var totalMinutes = (int)Math.Round(serial * 24 * 60);
            var h = totalMinutes / 60;
            var m = totalMinutes % 60;
            return $"{h:D2}:{m:D2}";
        }

        if (string.IsNullOrWhiteSpace(text)) return null;

        // 2) 텍스트 파싱 (HH:mm, H:mm)
        if (TimeSpan.TryParse(text, out var ts))
        {
            return $"{ts.Hours:D2}:{ts.Minutes:D2}";
        }

        // 3) 텍스트가 시리얼 숫자인 경우 ("0.375")
        if (double.TryParse(text, out var serialText) && serialText > 0 && serialText < 1)
        {
            var totalMinutes = (int)Math.Round(serialText * 24 * 60);
            var h = totalMinutes / 60;
            var m = totalMinutes % 60;
            return $"{h:D2}:{m:D2}";
        }

        return null;
    }
}
