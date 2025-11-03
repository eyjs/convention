using LocalRAG.DTOs.UploadModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Utilities;
using OfficeOpenXml;
using BCrypt.Net;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 사용자 업로드 서비스
/// Excel 형식: [소속|부서|이름|사번(주민번호)|전화번호|그룹]
///
/// 매칭 로직: 이름 + (전화번호 OR 주민등록번호)가 같으면 Update, 없으면 Insert
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

            _logger.LogInformation("Processing {RowCount} rows from Excel", rowCount);

            try
            {
                for (int row = 2; row <= rowCount; row++) // 1행은 헤더
                {
                    var affiliation = sheet.Cells[row, 1].Text?.Trim();
                    var corpPart = sheet.Cells[row, 2].Text?.Trim();
                    var userName = sheet.Cells[row, 3].Text?.Trim();
                    var residentNumber = sheet.Cells[row, 4].Text?.Trim();
                    var telephone = sheet.Cells[row, 5].Text?.Trim();
                    var groupName = sheet.Cells[row, 6].Text?.Trim();

                    // 필수값 검증 (이름, 그룹만 필수)
                    if (string.IsNullOrEmpty(userName))
                    {
                        result.Warnings.Add($"Row {row}: 이름이 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(groupName))
                    {
                        result.Warnings.Add($"Row {row}: 그룹명이 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    // 전화번호 검증 및 포맷팅 (선택사항)
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

                    // 주민등록번호 검증 및 포맷팅 (선택사항)
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

                    // 기존 User 찾기
                    // 1순위: 이름 + 전화번호 매칭
                    // 2순위: 이름 + 주민번호 매칭
                    // 3순위: 전화번호/주민번호 둘 다 없으면 신규 생성
                    User? existingUser = null;

                    if (!string.IsNullOrEmpty(normalizedPhone))
                    {
                        // 전화번호로 매칭
                        var usersByPhone = await _unitOfWork.Users
                            .FindAsync(u => u.Name == userName &&
                                           !string.IsNullOrEmpty(u.Phone));

                        existingUser = usersByPhone.FirstOrDefault(u =>
                            PhoneNumberFormatter.Normalize(u.Phone) == normalizedPhone);
                    }

                    if (existingUser == null && !string.IsNullOrEmpty(formattedResidentNumber))
                    {
                        // 주민번호로 매칭
                        var usersByResident = await _unitOfWork.Users
                            .FindAsync(u => u.Name == userName &&
                                           !string.IsNullOrEmpty(u.ResidentNumber));

                        existingUser = usersByResident.FirstOrDefault(u =>
                            PhoneNumberFormatter.Normalize(u.ResidentNumber ?? "") ==
                            PhoneNumberFormatter.Normalize(formattedResidentNumber));
                    }

                    if (existingUser != null)
                    {
                        // User 정보 업데이트 (포맷팅된 값으로 저장)
                        existingUser.Name = userName;
                        existingUser.Phone = formattedPhone;
                        existingUser.Affiliation = affiliation;
                        existingUser.CorpPart = corpPart;
                        existingUser.ResidentNumber = formattedResidentNumber;
                        existingUser.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.Users.Update(existingUser);

                        // UserConvention 존재 확인
                        var existingUserConventions = await _unitOfWork.UserConventions
                            .FindAsync(uc => uc.UserId == existingUser.Id && uc.ConventionId == conventionId);

                        var existingUserConvention = existingUserConventions.FirstOrDefault();

                        if (existingUserConvention != null)
                        {
                            // UserConvention 업데이트
                            existingUserConvention.GroupName = groupName;
                            _unitOfWork.UserConventions.Update(existingUserConvention);
                            result.UsersUpdated++;
                        }
                        else
                        {
                            // UserConvention 생성
                            var newUserConvention = new Entities.UserConvention
                            {
                                UserId = existingUser.Id,
                                ConventionId = conventionId,
                                GroupName = groupName,
                                AccessToken = Guid.NewGuid().ToString("N"),
                                CreatedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.UserConventions.AddAsync(newUserConvention);
                            result.UsersCreated++;
                        }

                        _logger.LogDebug("Updated user: {UserName} (Row {Row})", userName, row);
                    }
                    else
                    {
                        // LoginId 중복 체크 및 생성
                        var baseLoginId = userName;
                        var loginId = baseLoginId;
                        var suffix = 2;

                        // LoginId 중복 체크 (이름, 이름_2, 이름_3... 형태로 생성)
                        while (await _unitOfWork.Users.FindAsync(u => u.LoginId == loginId).ContinueWith(t => t.Result.Any()))
                        {
                            loginId = $"{baseLoginId}_{suffix}";
                            suffix++;
                        }

                        // 신규 User 및 UserConvention 생성 (포맷팅된 값으로 저장)
                        var newUser = new Entities.User
                        {
                            LoginId = loginId, // 이름 기반 LoginId
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1111"), // 기본 비밀번호 '1111' 해싱
                            Name = userName,
                            Phone = formattedPhone, // 포맷팅된 전화번호 저장 (010-XXXX-XXXX)
                            Role = "Guest",
                            Affiliation = affiliation,
                            CorpPart = corpPart,
                            ResidentNumber = formattedResidentNumber, // 포맷팅된 주민번호 저장 (XXXXXX-XXXXXXX)
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        // UserConvention을 User의 네비게이션 속성에 추가 (EF Core가 자동으로 관계 설정)
                        newUser.UserConventions = new List<Entities.UserConvention>
                        {
                            new Entities.UserConvention
                            {
                                ConventionId = conventionId,
                                GroupName = groupName,
                                AccessToken = Guid.NewGuid().ToString("N"),
                                CreatedAt = DateTime.UtcNow
                            }
                        };

                        await _unitOfWork.Users.AddAsync(newUser);
                        result.UsersCreated++;

                        _logger.LogDebug("Created user: {UserName} with LoginId: {LoginId} (Row {Row})", userName, loginId, row);
                    }
                }

                // 한 번의 SaveChanges로 모든 변경사항 커밋 (암시적 트랜잭션)
                await _unitOfWork.SaveChangesAsync();

                result.Success = true;
                _logger.LogInformation("User upload completed: {Created} created, {Updated} updated",
                    result.UsersCreated, result.UsersUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User upload failed during processing");
                throw;
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
}
