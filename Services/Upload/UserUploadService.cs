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
                    // A열은 무시
                    var corpPart = sheet.Cells[row, 2].Text?.Trim(); // B열: 부서 -> 소속(Affiliation)으로 사용
                    var userName = sheet.Cells[row, 3].Text?.Trim();
                    var residentNumber = sheet.Cells[row, 4].Text?.Trim();
                    var telephone = sheet.Cells[row, 5].Text?.Trim();
                    // F열(6)은 그룹 정보였으나, 이제 사용하지 않음
                    var remarks = sheet.Cells[row, 7].Text?.Trim(); // G열(7): 비고

                    // 필수값 검증 (이름만 필수)
                    if (string.IsNullOrEmpty(userName))
                    {
                        result.Warnings.Add($"Row {row}: 이름이 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    // 그룹명은 이제 엑셀에서 읽지 않으므로 null로 설정
                    string? groupName = null;

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
                        // User 정보 업데이트
                        existingUser.Name = userName;
                        existingUser.Phone = formattedPhone;
                        existingUser.Affiliation = corpPart; // B열(부서)을 소속으로 업데이트
                        existingUser.CorpPart = corpPart;
                        existingUser.ResidentNumber = formattedResidentNumber;
                        existingUser.Remarks = remarks; // G열(비고)을 Remarks 필드에 저장
                        existingUser.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.Users.Update(existingUser);

                        // UserConvention 존재 확인 및 처리
                        var existingUserConvention = (await _unitOfWork.UserConventions
                            .FindAsync(uc => uc.UserId == existingUser.Id && uc.ConventionId == conventionId))
                            .FirstOrDefault();

                        if (existingUserConvention != null)
                        {
                            existingUserConvention.GroupName = groupName; // null로 설정
                            _unitOfWork.UserConventions.Update(existingUserConvention);
                            result.UsersUpdated++;
                        }
                        else
                        {
                            await _unitOfWork.UserConventions.AddAsync(new UserConvention
                            {
                                UserId = existingUser.Id,
                                ConventionId = conventionId,
                                GroupName = groupName, // null
                                AccessToken = Guid.NewGuid().ToString("N"),
                                CreatedAt = DateTime.UtcNow
                            });
                            result.UsersCreated++;
                        }

                        _logger.LogDebug("Updated user: {UserName} (Row {Row})", userName, row);
                    }
                    else
                    {
                        // 신규 User 생성
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
                            Affiliation = corpPart, // B열(부서)을 소속으로 저장
                            CorpPart = corpPart,
                            ResidentNumber = formattedResidentNumber,
                            Remarks = remarks, // G열(비고)을 Remarks 필드에 저장
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            UserConventions = new List<UserConvention>
                            {
                                new UserConvention
                                {
                                    ConventionId = conventionId,
                                    GroupName = groupName, // null
                                    AccessToken = Guid.NewGuid().ToString("N"),
                                    CreatedAt = DateTime.UtcNow
                                }
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
