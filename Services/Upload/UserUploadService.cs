using LocalRAG.DTOs.UploadModels;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Utilities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using BCrypt.Net;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 사용자 업로드 서비스
/// 시트1: 참석자 [번호|소속|이름|주민번호|전화번호|그룹명|비고|속성1|속성2|...] (가변 컬럼)
///   - 1~7열: 고정 컬럼 (번호, 소속, 이름, 주민번호, 전화번호, 그룹명, 비고)
///   - 8열~: 가변 속성 컬럼 (헤더=AttributeKey, 값=AttributeValue)
///   - 첫 번째 셀이 '※'로 시작하는 행은 안내 행으로 스킵
/// 시트2: 그룹-일정 매핑 [그룹명|일정코스명|코스설명] (선택)
/// </summary>
public class UserUploadService : IUserUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserUploadService> _logger;

    public UserUploadService(
        IUnitOfWork unitOfWork,
        ILogger<UserUploadService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<UserUploadResult> UploadUsersAsync(int conventionId, Stream excelStream)
    {
        var result = new UserUploadResult();

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

            var sheet = package.Workbook.Worksheets[0];

            if (sheet.Dimension == null)
            {
                result.Errors.Add("시트가 비어있습니다.");
                return result;
            }

            var rowCount = sheet.Dimension.Rows;
            var colCount = sheet.Dimension.Columns;

            // 가변 속성 컬럼 헤더 파싱 (8번째 컬럼부터)
            var attributeHeaders = new List<string>();
            for (int col = 8; col <= colCount; col++)
            {
                var header = sheet.Cells[1, col].Text?.Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    attributeHeaders.Add(header);
                }
            }

            _logger.LogInformation("Processing {RowCount} rows from Excel, {AttrCount} attribute columns",
                rowCount, attributeHeaders.Count);

            // SqlServerRetryingExecutionStrategy 호환 트랜잭션
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
            // ===== 교체 모드: 기존 convention 종속 데이터 삭제 =====
            {
                var existingUcs = (await _unitOfWork.UserConventions
                    .FindAsync(uc => uc.ConventionId == conventionId)).ToList();

                // 1) UserActionStatus 삭제 (해당 convention)
                var existingActionStatuses = (await _unitOfWork.UserActionStatuses
                    .FindAsync(uas => uas.ConventionAction.ConventionId == conventionId)).ToList();
                if (existingActionStatuses.Count > 0)
                {
                    _unitOfWork.UserActionStatuses.RemoveRange(existingActionStatuses);
                }

                // 2) GuestScheduleTemplate (그룹-일정 매핑) 삭제 (해당 convention)
                var existingGuestSchedules = (await _unitOfWork.GuestScheduleTemplates
                    .FindAsync(gst => gst.ScheduleTemplate.ConventionId == conventionId)).ToList();
                if (existingGuestSchedules.Count > 0)
                {
                    _unitOfWork.GuestScheduleTemplates.RemoveRange(existingGuestSchedules);
                }

                // 3) UserConvention 삭제 (해당 convention)
                if (existingUcs.Count > 0)
                {
                    _unitOfWork.UserConventions.RemoveRange(existingUcs);
                }

                await _unitOfWork.SaveChangesAsync();

                result.RemovedUserConventions = existingUcs.Count;
                _logger.LogInformation(
                    "Cleared convention {ConventionId}: {Ucs} UserConventions, {Actions} UserActionStatuses, {GstMaps} GuestScheduleTemplates",
                    conventionId, existingUcs.Count, existingActionStatuses.Count, existingGuestSchedules.Count);
            }

            // 시트1 처리 후 행별 UserId 매핑 (시트2/3에서 사용)
            var rowUserMap = new Dictionary<int, int>(); // excelRow -> userId
            var rowNewUserMap = new Dictionary<int, User>(); // excelRow -> 신규 User 객체 (SaveChanges 후 Id 할당)
            // 가변 속성 컬럼 임시 저장 (row -> (key, value) 목록)
            var rowAttributesMap = new Dictionary<int, List<(string Key, string Value)>>();

                for (int row = 2; row <= rowCount; row++)
                {
                    // 첫 번째 셀이 '※'로 시작하면 안내 행 스킵
                    var firstCell = sheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
                    if (firstCell.StartsWith("※"))
                    {
                        _logger.LogDebug("Skipping notice row {Row}: {Content}", row, firstCell);
                        continue;
                    }

                    var corpPart = sheet.Cells[row, 2].Text?.Trim();
                    var userName = sheet.Cells[row, 3].Text?.Trim();
                    var residentNumber = sheet.Cells[row, 4].Text?.Trim();
                    var telephone = sheet.Cells[row, 5].Text?.Trim();
                    var groupName = sheet.Cells[row, 6].Text?.Trim();
                    var remarks = sheet.Cells[row, 7].Text?.Trim();

                    if (string.IsNullOrEmpty(userName))
                    {
                        result.Warnings.Add($"Row {row}: 이름이 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(telephone) && string.IsNullOrEmpty(residentNumber))
                    {
                        result.Warnings.Add($"Row {row}: '{userName}' — 전화번호와 주민번호가 모두 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(groupName))
                    {
                        groupName = null;
                    }

                    string? formattedPhone = null;
                    string? normalizedPhone = null;

                    if (!string.IsNullOrEmpty(telephone))
                    {
                        if (!PhoneNumberFormatter.IsValidPhoneNumber(telephone))
                        {
                            result.Warnings.Add($"Row {row}: 잘못된 전화번호 형식입니다. ({telephone}) - 전화번호 없이 저장됩니다.");
                        }
                        else
                        {
                            formattedPhone = PhoneNumberFormatter.FormatPhoneNumber(telephone);
                            normalizedPhone = PhoneNumberFormatter.Normalize(telephone);
                        }
                    }

                    string? formattedResidentNumber = null;
                    if (!string.IsNullOrEmpty(residentNumber))
                    {
                        if (!PhoneNumberFormatter.IsValidResidentNumber(residentNumber))
                        {
                            result.Warnings.Add($"Row {row}: 잘못된 주민등록번호 형식입니다. ({residentNumber}) - 주민번호 없이 저장됩니다.");
                        }
                        else
                        {
                            formattedResidentNumber = PhoneNumberFormatter.FormatResidentNumber(residentNumber);
                        }
                    }

                    result.TotalProcessed++;

                    User? existingUser = null;

                    if (!string.IsNullOrEmpty(normalizedPhone))
                    {
                        var usersByPhone = await _unitOfWork.Users
                            .FindAsync(u => u.Name == userName && !string.IsNullOrEmpty(u.Phone));
                        existingUser = usersByPhone.FirstOrDefault(u =>
                            PhoneNumberFormatter.Normalize(u.Phone) == normalizedPhone);
                    }

                    if (existingUser == null && !string.IsNullOrEmpty(formattedResidentNumber))
                    {
                        var usersByResident = await _unitOfWork.Users
                            .FindAsync(u => u.Name == userName && !string.IsNullOrEmpty(u.ResidentNumber));
                        existingUser = usersByResident.FirstOrDefault(u =>
                            PhoneNumberFormatter.Normalize(u.ResidentNumber ?? "") ==
                            PhoneNumberFormatter.Normalize(formattedResidentNumber));
                    }

                    if (existingUser != null)
                    {
                        existingUser.Name = userName;
                        existingUser.Phone = formattedPhone;
                        existingUser.Affiliation = corpPart;
                        existingUser.CorpPart = corpPart;
                        existingUser.ResidentNumber = formattedResidentNumber;
                        existingUser.Remarks = remarks;
                        existingUser.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.Users.Update(existingUser);

                        // 교체 모드: 기존 UserConvention은 이미 삭제됨 → 항상 신규 생성
                        await _unitOfWork.UserConventions.AddAsync(new UserConvention
                        {
                            UserId = existingUser.Id,
                            ConventionId = conventionId,
                            GroupName = groupName,
                            AccessToken = Guid.NewGuid().ToString("N"),
                            CreatedAt = DateTime.UtcNow
                        });
                        result.UsersUpdated++; // 기존 User 재사용

                        rowUserMap[row] = existingUser.Id;
                        _logger.LogDebug("Reused user: {UserName} (Row {Row})", userName, row);
                    }
                    else
                    {
                        var baseLoginId = userName;
                        var loginId = baseLoginId;
                        var suffix = 2;
                        while (await _unitOfWork.Users.FindAsync(u => u.LoginId == loginId).ContinueWith(t => t.Result.Any()))
                        {
                            loginId = $"{baseLoginId}_{suffix}";
                            suffix++;
                        }

                        var newUser = new User
                        {
                            LoginId = loginId,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1111"),
                            Name = userName,
                            Phone = formattedPhone,
                            Role = "Guest",
                            Affiliation = corpPart,
                            CorpPart = corpPart,
                            ResidentNumber = formattedResidentNumber,
                            Remarks = remarks,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            UserConventions = new List<UserConvention>
                            {
                                new UserConvention
                                {
                                    ConventionId = conventionId,
                                    GroupName = groupName,
                                    AccessToken = Guid.NewGuid().ToString("N"),
                                    CreatedAt = DateTime.UtcNow
                                }
                            }
                        };

                        await _unitOfWork.Users.AddAsync(newUser);
                        rowNewUserMap[row] = newUser;
                        result.UsersCreated++;

                        _logger.LogDebug("Created user: {UserName} with LoginId: {LoginId} (Row {Row})", userName, loginId, row);
                    }

                    // 가변 속성 컬럼 수집 (8번째 컬럼부터)
                    if (attributeHeaders.Count > 0)
                    {
                        var attrs = new List<(string Key, string Value)>();
                        for (int col = 8; col <= colCount && (col - 8) < attributeHeaders.Count; col++)
                        {
                            var attrValue = sheet.Cells[row, col].Text?.Trim();
                            if (!string.IsNullOrEmpty(attrValue))
                            {
                                attrs.Add((attributeHeaders[col - 8], attrValue));
                            }
                        }
                        if (attrs.Count > 0)
                        {
                            rowAttributesMap[row] = attrs;
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // 신규 유저의 ID를 rowUserMap에 반영 (SaveChanges 후 Id 할당됨)
                foreach (var (row, newUser) in rowNewUserMap)
                {
                    rowUserMap[row] = newUser.Id;
                }

                // 가변 속성 컬럼 저장 (userId 확정 후)
                if (rowAttributesMap.Count > 0)
                {
                    await ProcessInlineAttributesAsync(rowAttributesMap, rowUserMap, result);
                    await _unitOfWork.SaveChangesAsync();
                }

                result.Success = true;
                _logger.LogInformation("User upload completed: {Created} created, {Updated} updated, {AttrCreated} attrs created, {AttrUpdated} attrs updated",
                    result.UsersCreated, result.UsersUpdated, result.AttributesCreated, result.AttributesUpdated);

            // 시트2: 그룹-일정 매핑 처리
            if (package.Workbook.Worksheets.Count >= 2)
            {
                await ProcessScheduleMappingSheet(conventionId, package.Workbook.Worksheets[1], result);
            }

            // 시트3(속성)은 처리하지 않음 — GuestAttribute는 User 단위 메타데이터이며
            // 행사에 종속되지 않으므로 별도 기능으로 분리

            }); // ExecuteInTransactionAsync — 자동 커밋/롤백
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User upload failed");
            result.Errors.Add($"업로드 실패: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.Errors.Add($"상세: {ex.InnerException.Message}");
            }
            result.Success = false;
        }

        return result;
    }

    /// <summary>
    /// 시트2: 그룹-일정코스 매핑
    /// 형식: A열=그룹명, B열=일정코스명, C열=코스설명 (선택)
    /// </summary>
    private async Task ProcessScheduleMappingSheet(int conventionId, ExcelWorksheet sheet, UserUploadResult result)
    {
        if (sheet.Dimension == null) return;

        try
        {
            var sheetRowCount = sheet.Dimension.Rows;
            var colCount = sheet.Dimension.Columns;
            if (sheetRowCount < 2) return;

            // 컬럼 수 검증 (최소 2개: 그룹명, 일정코스명)
            if (colCount < 2)
            {
                result.ScheduleWarnings.Add("시트2: 컬럼이 부족합니다. A열=그룹명, B열=일정코스명 형식이어야 합니다.");
                return;
            }

            // 행사의 일정 템플릿 로드
            var templates = await _unitOfWork.ScheduleTemplates.Query
                .Where(t => t.ConventionId == conventionId)
                .ToListAsync();

            var templateMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in templates)
            {
                templateMap[t.CourseName] = t.Id;
            }

            if (templateMap.Count == 0)
            {
                result.ScheduleWarnings.Add("행사에 등록된 일정 템플릿이 없습니다. 일정을 먼저 업로드해주세요.");
                return;
            }

            // 그룹-코스 매핑 파싱
            var groupCourseMap = new Dictionary<string, List<int>>(); // groupName -> templateIds
            for (int row = 2; row <= sheetRowCount; row++)
            {
                var groupName = sheet.Cells[row, 1].Text?.Trim();
                var courseName = sheet.Cells[row, 2].Text?.Trim();

                if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(courseName))
                {
                    result.ScheduleWarnings.Add($"시트2 Row {row}: 그룹명 또는 코스명이 비어있습니다.");
                    continue;
                }

                if (!templateMap.TryGetValue(courseName, out var templateId))
                {
                    result.ScheduleWarnings.Add($"시트2 Row {row}: 일정코스 '{courseName}'을 찾을 수 없습니다.");
                    continue;
                }

                if (!groupCourseMap.ContainsKey(groupName))
                    groupCourseMap[groupName] = new List<int>();

                if (!groupCourseMap[groupName].Contains(templateId))
                    groupCourseMap[groupName].Add(templateId);
            }

            if (groupCourseMap.Count == 0) return;

            // 그룹별 사용자 조회
            var userConventions = await _unitOfWork.UserConventions.Query
                .Where(uc => uc.ConventionId == conventionId && uc.GroupName != null)
                .Select(uc => new { uc.UserId, uc.GroupName })
                .ToListAsync();

            // 기존 배정 조회
            var existingAssignments = await _unitOfWork.GuestScheduleTemplates.Query
                .Where(gst => userConventions.Select(uc => uc.UserId).Contains(gst.UserId))
                .Select(gst => new { gst.UserId, gst.ScheduleTemplateId })
                .ToListAsync();

            var existingSet = new HashSet<string>(
                existingAssignments.Select(a => $"{a.UserId}_{a.ScheduleTemplateId}"));

            foreach (var (groupName, templateIds) in groupCourseMap)
            {
                var usersInGroup = userConventions
                    .Where(uc => uc.GroupName == groupName)
                    .Select(uc => uc.UserId)
                    .ToList();

                if (usersInGroup.Count == 0)
                {
                    result.ScheduleWarnings.Add($"그룹 '{groupName}'에 속한 참석자가 없습니다.");
                    continue;
                }

                foreach (var userId in usersInGroup)
                {
                    foreach (var templateId in templateIds)
                    {
                        var key = $"{userId}_{templateId}";
                        if (existingSet.Contains(key))
                        {
                            result.ScheduleDuplicatesSkipped++;
                            continue;
                        }

                        await _unitOfWork.GuestScheduleTemplates.AddAsync(new GuestScheduleTemplate
                        {
                            UserId = userId,
                            ScheduleTemplateId = templateId,
                            AssignedAt = DateTime.UtcNow
                        });

                        existingSet.Add(key);
                        result.ScheduleAssignmentsCreated++;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Schedule mapping completed: {Created} assignments, {Skipped} duplicates skipped",
                result.ScheduleAssignmentsCreated, result.ScheduleDuplicatesSkipped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schedule mapping failed");
            result.ScheduleWarnings.Add($"일정 매핑 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 시트3: 속성 처리 (시트1과 같은 행 = 같은 사람)
    /// 형식: 1행=속성명 헤더, 2행~=속성값
    /// </summary>
    private async Task ProcessAttributeSheet(ExcelWorksheet sheet, Dictionary<int, int> rowUserMap, UserUploadResult result)
    {
        if (sheet.Dimension == null) return;

        try
        {
            var sheetRowCount = sheet.Dimension.Rows;
            var colCount = sheet.Dimension.Columns;

            if (sheetRowCount < 2 || colCount < 1) return;

            // 1행 헤더가 전부 비어있으면 무시
            var hasHeader = false;
            for (int col = 1; col <= colCount; col++)
            {
                if (!string.IsNullOrEmpty(sheet.Cells[1, col].Text?.Trim()))
                {
                    hasHeader = true;
                    break;
                }
            }
            if (!hasHeader)
            {
                result.AttributeWarnings.Add("시트3: 1행에 속성명 헤더가 없습니다.");
                return;
            }

            // 1행: 속성명 헤더 파싱
            var headers = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                var header = sheet.Cells[1, col].Text?.Trim();
                headers.Add(!string.IsNullOrEmpty(header) ? header : $"Column_{col}");
            }

            for (int row = 2; row <= sheetRowCount; row++)
            {
                if (!rowUserMap.TryGetValue(row, out var userId))
                {
                    continue; // 시트1에서 스킵된 행
                }

                var hasValue = false;
                for (int col = 1; col <= colCount; col++)
                {
                    var attributeKey = headers[col - 1];
                    var attributeValue = sheet.Cells[row, col].Text?.Trim();

                    if (string.IsNullOrEmpty(attributeValue)) continue;

                    hasValue = true;

                    var existingAttribute = await _unitOfWork.GuestAttributes
                        .GetAttributeByKeyAsync(userId, attributeKey);

                    if (existingAttribute != null)
                    {
                        existingAttribute.AttributeValue = attributeValue;
                        _unitOfWork.GuestAttributes.Update(existingAttribute);
                        result.AttributesUpdated++;
                    }
                    else
                    {
                        await _unitOfWork.GuestAttributes.AddAsync(new GuestAttribute
                        {
                            UserId = userId,
                            AttributeKey = attributeKey,
                            AttributeValue = attributeValue
                        });
                        result.AttributesCreated++;
                    }
                }

                if (hasValue) result.AttributeUsersProcessed++;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Attribute processing completed: {Users} users, {Created} created, {Updated} updated",
                result.AttributeUsersProcessed, result.AttributesCreated, result.AttributesUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Attribute processing failed");
            result.AttributeWarnings.Add($"속성 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 시트1의 가변 속성 컬럼(8번째~)에서 수집된 속성을 GuestAttribute로 저장
    /// SaveChanges 전 호출 — 호출 측에서 SaveChangesAsync 수행
    /// </summary>
    private async Task ProcessInlineAttributesAsync(
        Dictionary<int, List<(string Key, string Value)>> rowAttributesMap,
        Dictionary<int, int> rowUserMap,
        UserUploadResult result)
    {
        try
        {
            foreach (var (row, attrs) in rowAttributesMap)
            {
                if (!rowUserMap.TryGetValue(row, out var userId))
                    continue;

                foreach (var (key, value) in attrs)
                {
                    var existing = await _unitOfWork.GuestAttributes
                        .GetAttributeByKeyAsync(userId, key);

                    if (existing != null)
                    {
                        existing.AttributeValue = value;
                        _unitOfWork.GuestAttributes.Update(existing);
                        result.AttributesUpdated++;
                    }
                    else
                    {
                        await _unitOfWork.GuestAttributes.AddAsync(new GuestAttribute
                        {
                            UserId = userId,
                            AttributeKey = key,
                            AttributeValue = value
                        });
                        result.AttributesCreated++;
                    }
                }

                result.AttributeUsersProcessed++;
            }

            _logger.LogInformation(
                "Inline attribute processing: {Users} users, {Created} created, {Updated} updated",
                result.AttributeUsersProcessed, result.AttributesCreated, result.AttributesUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inline attribute processing failed");
            result.AttributeWarnings.Add($"속성 처리 중 오류: {ex.Message}");
        }
    }
}
