using LocalRAG.Constants;
using LocalRAG.DTOs.UploadModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Services.Upload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LocalRAG.Controllers;

/// <summary>
/// Excel 업로드 관련 API
/// - 참석자 업로드
/// - 일정 템플릿 업로드
/// - 속성 업로드
/// - 그룹-일정 매핑
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Constants.Roles.Admin)]
public class UploadController : ControllerBase
{
    private readonly IUserUploadService _userUploadService;
    private readonly IScheduleTemplateUploadService _scheduleTemplateUploadService;
    private readonly IAttributeUploadService _attributeUploadService;
    private readonly IGroupScheduleMappingService _groupScheduleMappingService;
    private readonly IOptionTourUploadService _optionTourUploadService;
    private readonly PassportUploadService _passportUploadService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        IUserUploadService userUploadService,
        IScheduleTemplateUploadService scheduleTemplateUploadService,
        IAttributeUploadService attributeUploadService,
        IGroupScheduleMappingService groupScheduleMappingService,
        IOptionTourUploadService optionTourUploadService,
        PassportUploadService passportUploadService,
        IUnitOfWork unitOfWork,
        ILogger<UploadController> logger)
    {
        _userUploadService = userUploadService;
        _scheduleTemplateUploadService = scheduleTemplateUploadService;
        _attributeUploadService = attributeUploadService;
        _groupScheduleMappingService = groupScheduleMappingService;
        _optionTourUploadService = optionTourUploadService;
        _passportUploadService = passportUploadService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 참석자 정보 업로드
    /// Excel 형식: [소속|부서|이름|사번(주민번호)|전화번호|그룹]
    /// </summary>
    [HttpPost("conventions/{conventionId}/guests")]
    [ProducesResponseType(typeof(UserUploadResult), 200)]
    public async Task<IActionResult> UploadGuests(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "파일이 비어있습니다." });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });
        }

        _logger.LogInformation("Uploading guests for convention {ConventionId}, file: {FileName}",
            conventionId, file.FileName);

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _userUploadService.UploadUsersAsync(conventionId, stream);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload guests");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 일정 템플릿 업로드
    /// Excel 형식:
    /// - A열: "월/일(요일)_일정명_시:분" (예: "11/03(일)_조식_07:30")
    /// - B열: 상세 내용 (엑셀 내부 줄바꿈 지원)
    /// </summary>
    [HttpPost("conventions/{conventionId}/schedule-templates")]
    [ProducesResponseType(typeof(ScheduleTemplateUploadResult), 200)]
    public async Task<IActionResult> UploadScheduleTemplates(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "파일이 비어있습니다." });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });
        }

        _logger.LogInformation("Uploading schedule templates for convention {ConventionId}, file: {FileName}",
            conventionId, file.FileName);

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _scheduleTemplateUploadService.UploadScheduleTemplatesAsync(conventionId, stream);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload schedule templates");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 일정 업로드 미리보기 — 파싱만 수행하고 결과 반환 (저장 X)
    /// </summary>
    [HttpPost("conventions/{conventionId}/schedule-templates/preview")]
    [ProducesResponseType(typeof(DTOs.UploadModels.ScheduleTemplatePreviewResult), 200)]
    public async Task<IActionResult> PreviewScheduleTemplates(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "파일이 비어있습니다." });

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _scheduleTemplateUploadService.PreviewScheduleTemplatesAsync(conventionId, stream);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preview schedule templates");
            return StatusCode(500, new { error = "미리보기 처리 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 미리보기 확인 후 단일 시트 최종 저장 (기존 호환)
    /// </summary>
    [HttpPost("conventions/{conventionId}/schedule-templates/confirm")]
    [ProducesResponseType(typeof(ScheduleTemplateUploadResult), 200)]
    public async Task<IActionResult> ConfirmScheduleTemplates(int conventionId, [FromBody] DTOs.UploadModels.ScheduleTemplateConfirmRequest request)
    {
        try
        {
            var result = await _scheduleTemplateUploadService.ConfirmScheduleTemplatesAsync(conventionId, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm schedule templates");
            return StatusCode(500, new { error = "저장 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 멀티시트 미리보기 확인 후 전체 시트 일괄 저장
    /// </summary>
    [HttpPost("conventions/{conventionId}/schedule-templates/confirm-multi")]
    [ProducesResponseType(typeof(ScheduleTemplateUploadResult), 200)]
    public async Task<IActionResult> ConfirmMultiSheetSchedules(int conventionId, [FromBody] DTOs.UploadModels.MultiSheetConfirmRequest request)
    {
        try
        {
            var result = await _scheduleTemplateUploadService.ConfirmMultiSheetAsync(conventionId, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm multi-sheet schedule templates");
            return StatusCode(500, new { error = "저장 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 참석자 속성 업로드
    /// Excel 형식: [이름|전화번호|속성1|속성2|...]
    /// 통계 정보 포함
    /// </summary>
    [HttpPost("conventions/{conventionId}/attributes")]
    [ProducesResponseType(typeof(UserAttributeUploadResult), 200)]
    public async Task<IActionResult> UploadAttributes(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "파일이 비어있습니다." });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });
        }

        _logger.LogInformation("Uploading attributes for convention {ConventionId}, file: {FileName}",
            conventionId, file.FileName);

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _attributeUploadService.UploadAttributesAsync(conventionId, stream);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload attributes");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 그룹-일정 매핑 (일괄 배정)
    /// 특정 그룹의 모든 참석자에게 여러 일정을 한 번에 배정
    /// </summary>
    [HttpPost("schedule-mapping/group")]
    [ProducesResponseType(typeof(GroupScheduleMappingResult), 200)]
    public async Task<IActionResult> MapGroupToSchedules([FromBody] GroupScheduleMappingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Mapping group '{Group}' to {ActionCount} actions in convention {ConventionId}",
            request.UserGroup, request.ActionIds.Count, request.ConventionId);

        try
        {
            var result = await _groupScheduleMappingService.MapGroupToSchedulesAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map group to schedules");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 행사의 모든 그룹 목록 조회
    /// </summary>
    [HttpGet("conventions/{conventionId}/groups")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public async Task<IActionResult> GetGroups(int conventionId)
    {
        _logger.LogInformation("Getting groups for convention {ConventionId}", conventionId);

        try
        {
            var groups = await _groupScheduleMappingService.GetGroupsInConventionAsync(conventionId);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get groups");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 옵션투어 업로드
    /// JSON 형식: { options: [...], participantMappings: [...] }
    /// 프론트엔드에서 엑셀을 파싱하여 JSON으로 전송
    /// </summary>
    [HttpPost("conventions/{conventionId}/option-tours")]
    [ProducesResponseType(typeof(OptionTourUploadResult), 200)]
    public async Task<IActionResult> UploadOptionTours(int conventionId, [FromBody] OptionTourUploadRequest request)
    {
        if (request == null || request.Options == null || request.ParticipantMappings == null)
        {
            return BadRequest(new { error = "요청 데이터가 올바르지 않습니다." });
        }

        _logger.LogInformation(
            "Uploading option tours for convention {ConventionId}: {OptionCount} options, {MappingCount} mappings",
            conventionId,
            request.Options.Count,
            request.ParticipantMappings.Count);

        try
        {
            var result = await _optionTourUploadService.UploadOptionToursAsync(conventionId, request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload option tours");
            return StatusCode(500, new { error = "서버 오류가 발생했습니다." });
        }
    }

    // ============================================================
    // 다운로드 엔드포인트
    // ============================================================

    /// <summary>
    /// 현재 참석자 목록 다운로드 (업로드 형식과 동일)
    /// 시트1: 참석자 (고정 7컬럼 + 가변 속성 컬럼)
    /// 시트2: 그룹-일정매핑
    /// </summary>
    [HttpGet("conventions/{conventionId}/guests/download")]
    public async Task<IActionResult> DownloadGuests(int conventionId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var guests = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
                .ThenInclude(u => u.GuestAttributes)
            .OrderBy(uc => uc.GroupName).ThenBy(uc => uc.User.Name)
            .ToListAsync();

        // 이 행사 참석자들의 모든 속성 키 수집 (정렬)
        var allAttributeKeys = guests
            .SelectMany(uc => uc.User.GuestAttributes.Select(a => a.AttributeKey))
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        using var package = new ExcelPackage();

        // ===== 시트1: 참석자 (고정 7컬럼 + 가변 속성 컬럼) =====
        var sheet1 = package.Workbook.Worksheets.Add("참석자");
        sheet1.Cells[1, 1].Value = "번호";
        sheet1.Cells[1, 2].Value = "소속/부서";
        sheet1.Cells[1, 3].Value = "이름";
        sheet1.Cells[1, 4].Value = "주민등록번호";
        sheet1.Cells[1, 5].Value = "전화번호";
        sheet1.Cells[1, 6].Value = "그룹명";
        sheet1.Cells[1, 7].Value = "비고";

        // 가변 속성 헤더 (8번째 컬럼~)
        for (int i = 0; i < allAttributeKeys.Count; i++)
        {
            sheet1.Cells[1, 8 + i].Value = allAttributeKeys[i];
        }

        var row = 2;
        var seq = 1;
        foreach (var uc in guests)
        {
            var u = uc.User;
            sheet1.Cells[row, 1].Value = seq++;
            sheet1.Cells[row, 2].Value = u.CorpPart ?? u.Affiliation;
            sheet1.Cells[row, 3].Value = u.Name;
            sheet1.Cells[row, 4].Value = u.ResidentNumber;
            sheet1.Cells[row, 5].Value = u.Phone;
            sheet1.Cells[row, 6].Value = uc.GroupName;
            sheet1.Cells[row, 7].Value = u.Remarks;

            // 가변 속성 값
            var attrDict = u.GuestAttributes.ToDictionary(a => a.AttributeKey, a => a.AttributeValue);
            for (int i = 0; i < allAttributeKeys.Count; i++)
            {
                attrDict.TryGetValue(allAttributeKeys[i], out var attrVal);
                sheet1.Cells[row, 8 + i].Value = attrVal;
            }

            row++;
        }
        sheet1.Cells[sheet1.Dimension?.Address ?? "A1"].AutoFitColumns();

        // ===== 시트2: 그룹-일정매핑 =====
        var sheet2 = package.Workbook.Worksheets.Add("그룹-일정매핑");
        sheet2.Cells[1, 1].Value = "그룹명";
        sheet2.Cells[1, 2].Value = "일정코스명";
        sheet2.Cells[1, 3].Value = "코스설명";

        // 현재 행사의 그룹별 코스 배정 조회
        var groupCourseMappings = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId && uc.GroupName != null)
            .Include(uc => uc.User)
                .ThenInclude(u => u.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
            .SelectMany(uc => uc.User.GuestScheduleTemplates
                .Where(gst => gst.ScheduleTemplate != null
                              && gst.ScheduleTemplate.ConventionId == conventionId)
                .Select(gst => new
                {
                    GroupName = uc.GroupName!,
                    CourseName = gst.ScheduleTemplate!.CourseName,
                    Description = gst.ScheduleTemplate!.Description
                }))
            .Distinct()
            .OrderBy(x => x.GroupName).ThenBy(x => x.CourseName)
            .ToListAsync();

        var row2 = 2;
        foreach (var m in groupCourseMappings)
        {
            sheet2.Cells[row2, 1].Value = m.GroupName;
            sheet2.Cells[row2, 2].Value = m.CourseName;
            sheet2.Cells[row2, 3].Value = m.Description;
            row2++;
        }
        sheet2.Cells[sheet2.Dimension?.Address ?? "A1"].AutoFitColumns();

        var bytes = package.GetAsByteArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"참석자_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// 현재 일정 템플릿 다운로드
    /// </summary>
    [HttpGet("conventions/{conventionId}/schedules/download")]
    public async Task<IActionResult> DownloadSchedules(int conventionId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var templates = await _unitOfWork.ScheduleTemplates.Query
            .Where(t => t.ConventionId == conventionId)
            .Include(t => t.ScheduleItems)
            .OrderBy(t => t.OrderNum)
            .ToListAsync();

        using var package = new ExcelPackage();

        foreach (var template in templates)
        {
            var sheetName = template.CourseName.Length > 31
                ? template.CourseName[..31]
                : template.CourseName;
            var sheet = package.Workbook.Worksheets.Add(sheetName);

            sheet.Cells[1, 1].Value = "날짜";
            sheet.Cells[1, 2].Value = "시작시간";
            sheet.Cells[1, 3].Value = "종료시간";
            sheet.Cells[1, 4].Value = "장소";
            sheet.Cells[1, 5].Value = "지도링크";
            sheet.Cells[1, 6].Value = "일정명";
            sheet.Cells[1, 7].Value = "내용";
            sheet.Cells[1, 8].Value = "노출_개인정보";

            var row = 2;
            foreach (var item in template.ScheduleItems.OrderBy(i => i.ScheduleDate).ThenBy(i => i.OrderNum))
            {
                sheet.Cells[row, 1].Value = item.ScheduleDate.ToString("yyyy-MM-dd");
                sheet.Cells[row, 2].Value = item.StartTime;
                sheet.Cells[row, 3].Value = item.EndTime;
                sheet.Cells[row, 4].Value = item.Location;
                sheet.Cells[row, 5].Value = null; // 지도링크 (엔티티 미지원)
                sheet.Cells[row, 6].Value = item.Title;
                sheet.Cells[row, 7].Value = item.Content;
                sheet.Cells[row, 8].Value = item.VisibleAttributes;
                row++;
            }

            sheet.Cells[sheet.Dimension?.Address ?? "A1"].AutoFitColumns();
        }

        var bytes = package.GetAsByteArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"일정_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// 현재 옵션투어 + 참석자 매핑 다운로드
    /// </summary>
    [HttpGet("conventions/{conventionId}/option-tours/download")]
    public async Task<IActionResult> DownloadOptionTours(int conventionId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var optionTours = await _unitOfWork.OptionTours.Query
            .Where(ot => ot.ConventionId == conventionId)
            .OrderBy(ot => ot.Date).ThenBy(ot => ot.StartTime)
            .ToListAsync();

        var userOptionTours = await _unitOfWork.UserOptionTours.Query
            .Where(uot => uot.ConventionId == conventionId)
            .ToListAsync();

        var userIds = userOptionTours.Select(uot => uot.UserId).Distinct().ToList();
        var users = await _unitOfWork.Users.Query
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name, u.Phone })
            .ToListAsync();

        var userMap = users.ToDictionary(u => u.Id);

        using var package = new ExcelPackage();

        // 시트1: 옵션 목록
        var sheet1 = package.Workbook.Worksheets.Add("옵션");
        sheet1.Cells[1, 1].Value = "날짜";
        sheet1.Cells[1, 2].Value = "시작시간";
        sheet1.Cells[1, 3].Value = "종료시간";
        sheet1.Cells[1, 4].Value = "옵션명";
        sheet1.Cells[1, 5].Value = "옵션내용";

        // optionTour.Id → 엑셀 행번호 매핑
        var tourRowMap = new Dictionary<int, int>();
        var row = 2;
        foreach (var ot in optionTours)
        {
            sheet1.Cells[row, 1].Value = ot.Date.ToString("yyyy-MM-dd");
            sheet1.Cells[row, 2].Value = ot.StartTime;
            sheet1.Cells[row, 3].Value = ot.EndTime;
            sheet1.Cells[row, 4].Value = ot.Name;
            sheet1.Cells[row, 5].Value = ot.Content;
            tourRowMap[ot.Id] = row;
            row++;
        }
        sheet1.Cells[sheet1.Dimension?.Address ?? "A1"].AutoFitColumns();

        // 시트2: 참석자별 매핑
        var sheet2 = package.Workbook.Worksheets.Add("참석자매핑");
        sheet2.Cells[1, 1].Value = "이름";
        sheet2.Cells[1, 2].Value = "전화번호";
        sheet2.Cells[1, 3].Value = "옵션번호";

        var userGroups = userOptionTours.GroupBy(uot => uot.UserId);
        row = 2;
        foreach (var group in userGroups)
        {
            if (!userMap.TryGetValue(group.Key, out var user)) continue;

            var optionRows = group
                .Where(uot => tourRowMap.ContainsKey(uot.OptionTourId))
                .Select(uot => tourRowMap[uot.OptionTourId])
                .OrderBy(r => r)
                .ToList();

            sheet2.Cells[row, 1].Value = user.Name;
            sheet2.Cells[row, 2].Value = user.Phone;
            sheet2.Cells[row, 3].Value = string.Join(",", optionRows);
            row++;
        }
        sheet2.Cells[sheet2.Dimension?.Address ?? "A1"].AutoFitColumns();

        var bytes = package.GetAsByteArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"옵션투어_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    // === 여권 일괄 업로드 ===

    /// <summary>
    /// 여권 정보 엑셀 + 이미지 ZIP 일괄 업로드
    /// 엑셀: A=이름, B=영문성, C=영문이름, D=만료일, E=파일명
    /// ZIP: E열 파일명과 매칭되는 이미지 파일들
    /// </summary>
    [HttpPost("conventions/{conventionId}/passport/bulk")]
    [RequestSizeLimit(524_288_000)] // 500MB
    public async Task<IActionResult> BulkUploadPassport(
        int conventionId,
        [FromForm] IFormFile excelFile,
        [FromForm] IFormFile? zipFile)
    {
        if (excelFile == null)
            return BadRequest(new { success = false, errors = new[] { "엑셀 파일은 필수입니다." } });

        var result = await _passportUploadService.ValidateAndUploadAsync(conventionId, excelFile, zipFile);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
