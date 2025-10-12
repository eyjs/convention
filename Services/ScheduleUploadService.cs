using LocalRAG.Data;
using LocalRAG.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace LocalRAG.Services;

public interface IScheduleUploadService
{
    Task<ScheduleUploadResult> UploadGuestsAndSchedulesAsync(int conventionId, Stream excelStream);
}

public class ScheduleUploadService : IScheduleUploadService
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ScheduleUploadService> _logger;

    public ScheduleUploadService(ConventionDbContext context, ILogger<ScheduleUploadService> logger)
    {
        _context = context;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<ScheduleUploadResult> UploadGuestsAndSchedulesAsync(int conventionId, Stream excelStream)
    {
        var result = new ScheduleUploadResult();
        
        try
        {
            using var package = new ExcelPackage(excelStream);
            
            if (package.Workbook.Worksheets.Count < 1)
            {
                result.Errors.Add("엑셀 파일에 시트가 없습니다.");
                return result;
            }

            var sheet1 = package.Workbook.Worksheets[0];
            
            if (sheet1.Dimension == null)
            {
                result.Errors.Add("Sheet1이 비어있습니다.");
                return result;
            }

            var rowCount = sheet1.Dimension.Rows;
            var colCount = sheet1.Dimension.Columns;

            var strategy = _context.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 기존 스케줄 데이터 삭제
                    await DeleteExistingScheduleDataAsync(conventionId);

                    // 2. Sheet2가 있으면 일정 내용 파싱
                    Dictionary<string, string> scheduleContents = new();
                    if (package.Workbook.Worksheets.Count >= 2)
                    {
                        var sheet2 = package.Workbook.Worksheets[1];
                        scheduleContents = ParseScheduleContents(sheet2);
                    }

                    // 3. 일정 헤더 파싱 (각 헤더 = 1개 ScheduleTemplate)
                    var scheduleHeaders = ParseScheduleHeaders(sheet1, colCount);
                    
                    if (scheduleHeaders.Count == 0)
                    {
                        result.Errors.Add("일정 헤더를 찾을 수 없습니다. (F열부터 시작하는 날짜/시간 형식 확인)");
                        return;
                    }

                    // 4. 각 헤더별로 ScheduleTemplate 생성
                    var templateMap = await CreateScheduleTemplatesAsync(conventionId, scheduleHeaders, scheduleContents);
                    
                    // 5. Guest 생성 또는 업데이트 및 일정 배정
                    var newAssignments = new List<GuestScheduleTemplate>();
                    var newAttributes = new List<GuestAttribute>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var name = sheet1.Cells[row, 3].Text?.Trim();
                        
                        if (string.IsNullOrEmpty(name)) continue;

                        var hrId = sheet1.Cells[row, 4].Text?.Trim();
                        var phone = sheet1.Cells[row, 5].Text?.Trim();
                        var buseo = sheet1.Cells[row, 2].Text?.Trim();
                        var affiliation = sheet1.Cells[row, 1].Text?.Trim();

                        // Guest 찾기 또는 생성
                        var guest = await _context.Guests
                            .FirstOrDefaultAsync(g => g.ConventionId == conventionId && g.GuestName == name && g.Telephone == phone);
                        
                        if (guest == null)
                        {
                            guest = new Guest
                            {
                                ConventionId = conventionId,
                                GuestName = name,
                                Telephone = phone ?? "",
                                ResidentNumber = hrId,
                                CorpPart = buseo,
                                Affiliation = affiliation,
                                IsRegisteredUser = false,
                                AccessToken = Guid.NewGuid().ToString("N")
                            };
                            
                            _context.Guests.Add(guest);
                            await _context.SaveChangesAsync();
                            result.GuestsCreated++;
                        }
                        else
                        {
                            // 기존 Guest 정보 업데이트
                            guest.ResidentNumber = hrId;
                            guest.CorpPart = buseo;
                            guest.Affiliation = affiliation;
                        }

                        // 일정 배정
                        foreach (var header in scheduleHeaders)
                        {
                            var cellValue = sheet1.Cells[row, header.ColumnIndex].Text?.Trim();
                            
                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                var template = templateMap[header.ColumnIndex];
                                
                                newAssignments.Add(new GuestScheduleTemplate
                                {
                                    GuestId = guest.Id,
                                    ScheduleTemplateId = template.Id
                                });
                                result.ScheduleAssignments++;
                                
                                // "O"가 아닌 다른 값이면 노트로 저장
                                if (cellValue.ToUpper() != "O")
                                {
                                    newAttributes.Add(new GuestAttribute
                                    {
                                        GuestId = guest.Id,
                                        AttributeKey = $"schedule_{template.Id}_note",
                                        AttributeValue = cellValue
                                    });
                                }
                            }
                        }
                    }

                    // 6. 배치로 추가
                    if (newAssignments.Any())
                    {
                        await _context.GuestScheduleTemplates.AddRangeAsync(newAssignments);
                    }
                    
                    if (newAttributes.Any())
                    {
                        await _context.GuestAttributes.AddRangeAsync(newAttributes);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    result.Success = true;
                    result.TotalSchedules = scheduleHeaders.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction failed");
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed");
            result.Errors.Add($"업로드 실패: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.Errors.Add($"상세: {ex.InnerException.Message}");
            }
        }

        return result;
    }

    private async Task DeleteExistingScheduleDataAsync(int conventionId)
    {
        var templateIds = await _context.ScheduleTemplates
            .Where(st => st.ConventionId == conventionId)
            .Select(st => st.Id)
            .ToListAsync();

        if (templateIds.Any())
        {
            var items = await _context.ScheduleItems
                .Where(si => templateIds.Contains(si.ScheduleTemplateId))
                .ToListAsync();
            _context.ScheduleItems.RemoveRange(items);

            var assignments = await _context.GuestScheduleTemplates
                .Where(gst => templateIds.Contains(gst.ScheduleTemplateId))
                .ToListAsync();
            _context.GuestScheduleTemplates.RemoveRange(assignments);
        }

        var templates = await _context.ScheduleTemplates
            .Where(st => st.ConventionId == conventionId)
            .ToListAsync();
        _context.ScheduleTemplates.RemoveRange(templates);

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted existing schedule data for convention {ConventionId}: {TemplateCount} templates", 
            conventionId, templates.Count);
    }

    private Dictionary<string, string> ParseScheduleContents(ExcelWorksheet sheet2)
    {
        var contents = new Dictionary<string, string>();
        
        if (sheet2.Dimension == null) return contents;
        
        var rowCount = sheet2.Dimension.Rows;
        
        for (int row = 1; row <= rowCount; row++)
        {
            var scheduleName = sheet2.Cells[row, 1].Text?.Trim();
            var description = sheet2.Cells[row, 2].Text?.Trim();
            
            if (!string.IsNullOrEmpty(scheduleName) && !string.IsNullOrEmpty(description))
            {
                contents[scheduleName] = description.Replace("<br/>", "\n");
            }
        }
        
        return contents;
    }

    private List<ScheduleHeaderInfo> ParseScheduleHeaders(ExcelWorksheet sheet, int colCount)
    {
        var headers = new List<ScheduleHeaderInfo>();
        
        for (int col = 6; col <= colCount; col++)
        {
            var headerText = sheet.Cells[1, col].Text?.Trim();
            if (string.IsNullOrEmpty(headerText)) continue;

            // 패턴: "11/03(일)_코스명_07:30"
            var match = Regex.Match(headerText, @"(\d+)/(\d+)\((.+?)\)_(.+?)_(\d{2}):(\d{2})");
            
            if (match.Success)
            {
                var month = int.Parse(match.Groups[1].Value);
                var day = int.Parse(match.Groups[2].Value);
                var courseName = match.Groups[4].Value.Trim(); // 코스명만 추출
                var hour = int.Parse(match.Groups[5].Value);
                var minute = int.Parse(match.Groups[6].Value);
                
                headers.Add(new ScheduleHeaderInfo
                {
                    ColumnIndex = col,
                    Month = month,
                    Day = day,
                    Hour = hour,
                    Minute = minute,
                    Title = courseName, // 순수 코스명
                    HeaderText = headerText // 전체 헤더 (Sheet2 매칭용)
                });
            }
        }

        return headers;
    }

    private async Task<Dictionary<int, ScheduleTemplate>> CreateScheduleTemplatesAsync(
        int conventionId, 
        List<ScheduleHeaderInfo> headers,
        Dictionary<string, string> scheduleContents)
    {
        var templateMap = new Dictionary<int, ScheduleTemplate>();
        int orderNum = 0;
        
        // 각 헤더별로 ScheduleTemplate 생성
        foreach (var header in headers.OrderBy(h => h.Month).ThenBy(h => h.Day).ThenBy(h => h.Hour).ThenBy(h => h.Minute))
        {
            // CourseName: 헤더 전체 텍스트 사용
            var courseName = header.HeaderText;
            
            // Sheet2에서 내용 찾기
            string? content = null;
            if (scheduleContents.ContainsKey(header.HeaderText))
            {
                content = scheduleContents[header.HeaderText];
            }
            
            // DateTime 생성 (현재 년도 사용)
            var currentYear = DateTime.Now.Year;
            var scheduleDateTime = new DateTime(currentYear, header.Month, header.Day, header.Hour, header.Minute, 0);
            
            // ScheduleTemplate 생성
            var template = new ScheduleTemplate
            {
                ConventionId = conventionId,
                CourseName = courseName,
                Description = null,
                OrderNum = orderNum++
            };
            
            _context.ScheduleTemplates.Add(template);
            await _context.SaveChangesAsync();
            
            // ScheduleItem 생성 (1개만)
            var scheduleItem = new ScheduleItem
            {
                ScheduleTemplateId = template.Id,
                ScheduleDate = scheduleDateTime, // DateTime 형태로 저장
                StartTime = $"{header.Hour:D2}:{header.Minute:D2}", // 호환성을 위해 유지
                Title = header.Title,
                Content = content ?? header.Title,
                OrderNum = 0
            };
            
            _context.ScheduleItems.Add(scheduleItem);
            
            templateMap[header.ColumnIndex] = template;
        }

        await _context.SaveChangesAsync();
        return templateMap;
    }
}

public class ScheduleHeaderInfo
{
    public int ColumnIndex { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HeaderText { get; set; } = string.Empty;
}

public class ScheduleUploadResult
{
    public bool Success { get; set; }
    public int TotalSchedules { get; set; }
    public int GuestsCreated { get; set; }
    public int ScheduleAssignments { get; set; }
    public List<string> Errors { get; set; } = new();
}
