using System.Text.RegularExpressions;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Services.Shared.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LocalRAG.Services.Shared;

/// <summary>
/// SMS 그룹 발송 서비스
/// 그룹 조회, 엑셀 템플릿 생성/파싱, 변수 치환 발송
/// </summary>
public class SmsGroupService : ISmsGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly ITemplateVariableService _templateVariableService;
    private readonly SmsTemplateContextFactory _contextFactory;
    private readonly ILogger<SmsGroupService> _logger;

    private const int MaxRecipientsPerRequest = 5000;
    private const int MaxMessageLength = 2000;

    public SmsGroupService(
        IUnitOfWork unitOfWork,
        ISmsService smsService,
        ITemplateVariableService templateVariableService,
        SmsTemplateContextFactory contextFactory,
        ILogger<SmsGroupService> logger)
    {
        _unitOfWork = unitOfWork;
        _smsService = smsService;
        _templateVariableService = templateVariableService;
        _contextFactory = contextFactory;
        _logger = logger;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<List<SmsGroupInfoDto>> GetGroupsAsync(int conventionId)
    {
        // UserConvention에서 해당 행사의 모든 참석자를 그룹별로 집계
        var userConventions = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
            .ToListAsync();

        var result = new List<SmsGroupInfoDto>();

        // "전체" 항목
        result.Add(new SmsGroupInfoDto
        {
            GroupName = "전체",
            Count = userConventions.Count,
            NoPhoneCount = userConventions.Count(uc => string.IsNullOrWhiteSpace(uc.User?.Phone))
        });

        // 그룹별 집계
        var groups = userConventions
            .Where(uc => !string.IsNullOrWhiteSpace(uc.GroupName))
            .GroupBy(uc => uc.GroupName!)
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            result.Add(new SmsGroupInfoDto
            {
                GroupName = group.Key,
                Count = group.Count(),
                NoPhoneCount = group.Count(uc => string.IsNullOrWhiteSpace(uc.User?.Phone))
            });
        }

        return result;
    }

    public async Task<byte[]> GenerateExcelTemplateAsync(int conventionId, string? groupName)
    {
        // 수신자 목록 조회
        var baseQuery = _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId);

        if (!string.IsNullOrEmpty(groupName) && groupName != "전체")
        {
            baseQuery = baseQuery.Where(uc => uc.GroupName == groupName);
        }

        var userConventions = await baseQuery
            .Include(uc => uc.User)
            .OrderBy(uc => uc.User!.Name)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("SMS 발송");

        // 헤더 설정
        worksheet.Cells[1, 1].Value = "이름";
        worksheet.Cells[1, 2].Value = "전화번호";
        // 예시 변수 컬럼 (관리자가 헤더명 수정 및 추가 가능)
        worksheet.Cells[1, 3].Value = "변수1";
        worksheet.Cells[1, 4].Value = "변수2";
        worksheet.Cells[1, 5].Value = "변수3";

        // 헤더 스타일
        using (var headerRange = worksheet.Cells[1, 1, 1, 5])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(220, 230, 241));
        }

        // 고정 컬럼 배경색 (수정 불가 영역 표시)
        using (var fixedRange = worksheet.Cells[1, 1, 1, 2])
        {
            fixedRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(198, 224, 180));
        }

        // 수신자 데이터 채우기
        var row = 2;
        foreach (var uc in userConventions)
        {
            if (uc.User == null) continue;
            worksheet.Cells[row, 1].Value = uc.User.Name;
            worksheet.Cells[row, 2].Value = uc.User.Phone ?? string.Empty;
            row++;
        }

        // 컬럼 너비 자동 조정
        worksheet.Cells.AutoFitColumns();
        worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 12);
        worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 15);

        return package.GetAsByteArray();
    }

    public async Task<SmsExcelParseResultDto> ParseExcelAsync(int conventionId, Stream excelStream)
    {
        var result = new SmsExcelParseResultDto();

        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            result.ErrorRows.Add(new SmsExcelErrorRowDto { Row = 0, Reason = "엑셀 시트를 찾을 수 없습니다." });
            return result;
        }

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        var colCount = worksheet.Dimension?.Columns ?? 0;

        if (rowCount < 2 || colCount < 2)
        {
            result.ErrorRows.Add(new SmsExcelErrorRowDto { Row = 0, Reason = "데이터가 없거나 형식이 올바르지 않습니다." });
            return result;
        }

        // 변수 컬럼 헤더 추출 (3열부터)
        for (var col = 3; col <= colCount; col++)
        {
            var header = worksheet.Cells[1, col].Text?.Trim();
            if (!string.IsNullOrWhiteSpace(header))
            {
                result.Columns.Add(header);
            }
        }

        // 데이터 행 파싱
        for (var row = 2; row <= rowCount; row++)
        {
            var name = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
            var phone = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty;

            // 빈 행 건너뛰기
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phone))
                continue;

            // 전화번호 정규화
            var normalizedPhone = NormalizePhone(phone);

            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                result.ErrorRows.Add(new SmsExcelErrorRowDto { Row = row, Reason = "전화번호 없음" });
                continue;
            }

            if (!IsValidKoreanMobile(normalizedPhone))
            {
                result.ErrorRows.Add(new SmsExcelErrorRowDto { Row = row, Reason = $"잘못된 전화번호 형식: {phone}" });
                continue;
            }

            var recipient = new SmsExcelRecipientDto
            {
                Name = name,
                Phone = normalizedPhone,
            };

            // 변수 값 추출
            for (var col = 3; col <= colCount; col++)
            {
                var headerIdx = col - 3;
                if (headerIdx < result.Columns.Count)
                {
                    var value = worksheet.Cells[row, col].Text?.Trim() ?? string.Empty;
                    recipient.Variables[result.Columns[headerIdx]] = value;
                }
            }

            result.Recipients.Add(recipient);
        }

        return result;
    }

    public async Task<SendSmsDirectResult> SendWithExcelVariablesAsync(int conventionId, SmsExcelSendRequestDto request)
    {
        if (request.Recipients.Count > MaxRecipientsPerRequest)
        {
            throw new ArgumentException($"수신자는 한 번에 최대 {MaxRecipientsPerRequest}명까지 가능합니다.");
        }

        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId)
            ?? throw new KeyNotFoundException("행사 정보를 찾을 수 없습니다.");

        var result = new SendSmsDirectResult
        {
            TotalCount = request.Recipients.Count
        };

        foreach (var recipient in request.Recipients)
        {
            var normalizedPhone = NormalizePhone(recipient.Phone);

            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                AddFail(result, recipient, "전화번호 없음");
                continue;
            }

            if (!IsValidKoreanMobile(normalizedPhone))
            {
                AddFail(result, recipient, $"잘못된 전화번호 형식: {recipient.Phone}");
                continue;
            }

            // SmsTemplateContext 생성 - 엑셀 변수를 GuestAttributes에 병합
            var context = new SmsTemplateContext
            {
                GuestName = recipient.Name,
                GuestPhone = normalizedPhone,
                ConventionTitle = convention.Title ?? string.Empty,
                StartDate = convention.StartDate?.ToString("yyyy.MM.dd") ?? string.Empty,
                EndDate = convention.EndDate?.ToString("yyyy.MM.dd") ?? string.Empty,
                PreEndDate = convention.PreEndDate?.ToString("yyyy.MM.dd") ?? string.Empty,
                Url = $"https://event.ifa.co.kr/invite/{convention.Id}",
            };

            // 엑셀 변수를 GuestAttributes에 병합
            foreach (var (key, value) in recipient.Variables)
            {
                context.GuestAttributes[key] = value;
            }

            // 변수 치환
            var message = _templateVariableService.ReplaceVariables(request.Template, context);

            if (string.IsNullOrWhiteSpace(message))
            {
                AddFail(result, recipient, "메시지 없음");
                continue;
            }

            if (message.Length > MaxMessageLength)
            {
                AddFail(result, recipient, $"메시지가 너무 깁니다 ({message.Length}/{MaxMessageLength}자)");
                continue;
            }

            try
            {
                var success = await _smsService.SendSmsAsync(
                    conventionId,
                    recipient.Name,
                    normalizedPhone,
                    message);

                if (success)
                {
                    result.SuccessCount++;
                }
                else
                {
                    AddFail(result, recipient, "발송 실패");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS 엑셀 변수 발송 오류 - To: {Phone}", normalizedPhone);
                AddFail(result, recipient, ex.Message);
            }
        }

        _logger.LogInformation(
            "SMS 엑셀 변수 발송 완료 - Convention: {ConvId}, Total: {Total}, Success: {Success}, Fail: {Fail}",
            conventionId, result.TotalCount, result.SuccessCount, result.FailCount);

        return result;
    }

    private static void AddFail(SendSmsDirectResult result, SmsExcelRecipientDto recipient, string reason)
    {
        result.FailCount++;
        result.FailedItems.Add(new SmsDirectFailItem
        {
            Name = recipient.Name,
            Phone = recipient.Phone,
            Reason = reason
        });
    }

    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = Regex.Replace(phone, @"\D", "");

        if (digits.StartsWith("82") && digits.Length >= 11)
        {
            digits = "0" + digits.Substring(2);
        }

        return digits;
    }

    private static bool IsValidKoreanMobile(string phone)
    {
        return Regex.IsMatch(phone, @"^01[016789]\d{7,8}$");
    }
}
