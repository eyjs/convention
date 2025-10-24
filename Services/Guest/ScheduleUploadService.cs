using LocalRAG.Data;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace LocalRAG.Services.Guest;

public class ScheduleUploadService : IScheduleUploadService
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ScheduleUploadService> _logger;
    private const string DEFAULT_COURSE_NAME = "기본 일정 코스";

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
                    // 1. 기본 일정 템플릿 가져오기 또는 생성
                    var defaultTemplate = await GetOrCreateDefaultTemplateAsync(conventionId);

                    // 2. Sheet2가 있으면 일정 내용 파싱
                    Dictionary<string, string> scheduleContents = new();
                    if (package.Workbook.Worksheets.Count >= 2)
                    {
                        var sheet2 = package.Workbook.Worksheets[1];
                        scheduleContents = ParseScheduleContents(sheet2);
                    }

                    // 3. 일정 헤더 파싱
                    var scheduleHeaders = ParseScheduleHeaders(sheet1, colCount);
                    
                    if (scheduleHeaders.Count == 0)
                    {
                        result.Errors.Add("일정 헤더를 찾을 수 없습니다. (F열부터 시작하는 날짜/시간 형식 확인)");
                        return;
                    }

                    // 4. 기존 ScheduleItem 삭제 (템플릿은 유지)
                    await DeleteExistingScheduleItemsAsync(defaultTemplate.Id);

                    // 5. 새로운 ScheduleItem 생성
                    await CreateScheduleItemsAsync(defaultTemplate.Id, scheduleHeaders, scheduleContents);
                    result.TotalSchedules = scheduleHeaders.Count;

                    // 6. Guest 생성/업데이트 및 일정 배정
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
                            guest = new LocalRAG.Entities.Guest
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

                        // 기존 배정 확인
                        var existingAssignment = await _context.GuestScheduleTemplates
                            .FirstOrDefaultAsync(gst => gst.GuestId == guest.Id && gst.ScheduleTemplateId == defaultTemplate.Id);

                        if (existingAssignment == null)
                        {
                            // 기본 템플릿 배정
                            newAssignments.Add(new GuestScheduleTemplate
                            {
                                GuestId = guest.Id,
                                ScheduleTemplateId = defaultTemplate.Id
                            });
                            result.ScheduleAssignments++;
                        }

                        // 일정별 노트 처리
                        foreach (var header in scheduleHeaders)
                        {
                            var cellValue = sheet1.Cells[row, header.ColumnIndex].Text?.Trim();
                            
                            if (!string.IsNullOrEmpty(cellValue) && cellValue.ToUpper() != "O")
                            {
                                newAttributes.Add(new GuestAttribute
                                {
                                    GuestId = guest.Id,
                                    AttributeKey = $"schedule_{header.ColumnIndex}_note",
                                    AttributeValue = cellValue
                                });
                            }
                        }
                    }

                    // 7. 배치로 추가
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

    /// <summary>
    /// 행사의 기본 일정 템플릿 가져오기 또는 생성
    /// </summary>
    private async Task<ScheduleTemplate> GetOrCreateDefaultTemplateAsync(int conventionId)
    {
        var template = await _context.ScheduleTemplates
            .FirstOrDefaultAsync(st => st.ConventionId == conventionId && st.CourseName == DEFAULT_COURSE_NAME);

        if (template == null)
        {
            template = new ScheduleTemplate
            {
                ConventionId = conventionId,
                CourseName = DEFAULT_COURSE_NAME,
                Description = "엑셀 업로드로 생성된 기본 일정 코스",
                OrderNum = 0
            };

            _context.ScheduleTemplates.Add(template);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created default schedule template for convention {ConventionId}", conventionId);
        }

        return template;
    }

    /// <summary>
    /// 기존 ScheduleItem만 삭제 (템플릿은 유지)
    /// </summary>
    private async Task DeleteExistingScheduleItemsAsync(int templateId)
    {
        var items = await _context.ScheduleItems
            .Where(si => si.ScheduleTemplateId == templateId)
            .ToListAsync();

        if (items.Any())
        {
            _context.ScheduleItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted {Count} existing schedule items for template {TemplateId}", 
                items.Count, templateId);
        }
    }

    /// <summary>
    /// ScheduleItem 생성
    /// </summary>
    private async Task CreateScheduleItemsAsync(
        int templateId,
        List<ScheduleHeaderInfo> headers,
        Dictionary<string, string> scheduleContents)
    {
        int orderNum = 0;
        
        foreach (var header in headers.OrderBy(h => h.Month).ThenBy(h => h.Day).ThenBy(h => h.Hour).ThenBy(h => h.Minute))
        {
            // Sheet2에서 내용 찾기
            string? content = null;
            if (scheduleContents.ContainsKey(header.HeaderText))
            {
                content = scheduleContents[header.HeaderText];
            }
            
            // DateTime 생성
            var currentYear = DateTime.Now.Year;
            var scheduleDateTime = new DateTime(currentYear, header.Month, header.Day, header.Hour, header.Minute, 0);
            
            // ScheduleItem 생성
            var scheduleItem = new ScheduleItem
            {
                ScheduleTemplateId = templateId,
                ScheduleDate = scheduleDateTime,
                StartTime = $"{header.Hour:D2}:{header.Minute:D2}",
                Title = header.Title,
                Content = content ?? header.Title,
                OrderNum = orderNum++
            };
            
            _context.ScheduleItems.Add(scheduleItem);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Created {Count} schedule items for template {TemplateId}", orderNum, templateId);
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
                var courseName = match.Groups[4].Value.Trim();
                var hour = int.Parse(match.Groups[5].Value);
                var minute = int.Parse(match.Groups[6].Value);
                
                headers.Add(new ScheduleHeaderInfo
                {
                    ColumnIndex = col,
                    Month = month,
                    Day = day,
                    Hour = hour,
                    Minute = minute,
                    Title = courseName,
                    HeaderText = headerText
                });
            }
        }

        return headers;
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
