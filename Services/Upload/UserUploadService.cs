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
/// 시트1: 참석자 [소속|부서|이름|주민번호|전화번호|그룹|비고]
/// 시트2: 그룹-일정 매핑 [그룹명|일정코스명] (선택)
/// 시트3: 속성 [속성1|속성2|...] — 시트1과 같은 행 = 같은 사람 (선택)
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

            _logger.LogInformation("Processing {RowCount} rows from Excel", rowCount);

            // 시트1 처리 후 행별 UserId 매핑 (시트2/3에서 사용)
            var rowUserMap = new Dictionary<int, int>(); // excelRow -> userId
            var rowNewUserMap = new Dictionary<int, User>(); // excelRow -> 신규 User 객체 (SaveChanges 후 Id 할당)

            try
            {
                for (int row = 2; row <= rowCount; row++)
                {
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

                        var existingUserConvention = (await _unitOfWork.UserConventions
                            .FindAsync(uc => uc.UserId == existingUser.Id && uc.ConventionId == conventionId))
                            .FirstOrDefault();

                        if (existingUserConvention != null)
                        {
                            existingUserConvention.GroupName = groupName;
                            _unitOfWork.UserConventions.Update(existingUserConvention);
                            result.UsersUpdated++;
                        }
                        else
                        {
                            await _unitOfWork.UserConventions.AddAsync(new UserConvention
                            {
                                UserId = existingUser.Id,
                                ConventionId = conventionId,
                                GroupName = groupName,
                                AccessToken = Guid.NewGuid().ToString("N"),
                                CreatedAt = DateTime.UtcNow
                            });
                            result.UsersCreated++;
                        }

                        rowUserMap[row] = existingUser.Id;
                        _logger.LogDebug("Updated user: {UserName} (Row {Row})", userName, row);
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
                }

                await _unitOfWork.SaveChangesAsync();

                // 신규 유저의 ID를 rowUserMap에 반영 (SaveChanges 후 Id 할당됨)
                foreach (var (row, newUser) in rowNewUserMap)
                {
                    rowUserMap[row] = newUser.Id;
                }

                result.Success = true;
                _logger.LogInformation("User upload completed: {Created} created, {Updated} updated",
                    result.UsersCreated, result.UsersUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User upload failed during processing");
                throw;
            }

            // 시트2: 그룹-일정 매핑 처리
            if (package.Workbook.Worksheets.Count >= 2)
            {
                await ProcessScheduleMappingSheet(conventionId, package.Workbook.Worksheets[1], result);
            }

            // 시트3: 속성 처리 (시트1과 같은 행 = 같은 사람)
            if (package.Workbook.Worksheets.Count >= 3)
            {
                await ProcessAttributeSheet(package.Workbook.Worksheets[2], rowUserMap, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User upload failed");
            result.Errors.Add($"업로드 실패: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.Errors.Add($"상세: {ex.InnerException.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// 시트2: 그룹-일정코스 매핑
    /// 형식: A열=그룹명, B열=일정코스명
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
}
