using LocalRAG.DTOs.UploadModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using OfficeOpenXml;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 참석자 업로드 서비스
/// Excel 형식: [소속|부서|이름|사번(주민번호)|전화번호|그룹]
/// </summary>
public class GuestUploadService : IGuestUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GuestUploadService> _logger;

    public GuestUploadService(
        IUnitOfWork unitOfWork,
        ILogger<GuestUploadService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<GuestUploadResult> UploadGuestsAsync(int conventionId, Stream excelStream)
    {
        var result = new GuestUploadResult();

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

            // 트랜잭션으로 처리
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                for (int row = 2; row <= rowCount; row++) // 1행은 헤더
                {
                    var affiliation = sheet.Cells[row, 1].Text?.Trim();
                    var corpPart = sheet.Cells[row, 2].Text?.Trim();
                    var guestName = sheet.Cells[row, 3].Text?.Trim();
                    var residentNumber = sheet.Cells[row, 4].Text?.Trim();
                    var telephone = sheet.Cells[row, 5].Text?.Trim();
                    var groupName = sheet.Cells[row, 6].Text?.Trim();

                    // 필수값 검증
                    if (string.IsNullOrEmpty(guestName))
                    {
                        result.Warnings.Add($"Row {row}: 이름이 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(telephone))
                    {
                        result.Warnings.Add($"Row {row}: 전화번호가 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(groupName))
                    {
                        result.Warnings.Add($"Row {row}: 그룹명이 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    result.TotalProcessed++;

                    // 기존 Guest 찾기 (ConventionId + 이름 + 전화번호로 unique 판단)
                    var existingGuests = await _unitOfWork.Guests
                        .FindAsync(g => g.ConventionId == conventionId
                                     && g.GuestName == guestName
                                     && g.Telephone == telephone);

                    var existingGuest = existingGuests.FirstOrDefault();

                    if (existingGuest != null)
                    {
                        // 업데이트
                        existingGuest.Affiliation = affiliation;
                        existingGuest.CorpPart = corpPart;
                        existingGuest.ResidentNumber = residentNumber;
                        existingGuest.GroupName = groupName;

                        _unitOfWork.Guests.Update(existingGuest);
                        result.GuestsUpdated++;

                        _logger.LogDebug("Updated guest: {GuestName} (Row {Row})", guestName, row);
                    }
                    else
                    {
                        // 신규 생성
                        var newGuest = new Entities.Guest
                        {
                            ConventionId = conventionId,
                            GuestName = guestName,
                            Telephone = telephone,
                            Affiliation = affiliation,
                            CorpPart = corpPart,
                            ResidentNumber = residentNumber,
                            GroupName = groupName,
                            IsRegisteredUser = false,
                            AccessToken = Guid.NewGuid().ToString("N")
                        };

                        await _unitOfWork.Guests.AddAsync(newGuest);
                        result.GuestsCreated++;

                        _logger.LogDebug("Created guest: {GuestName} (Row {Row})", guestName, row);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.Success = true;
                _logger.LogInformation("Guest upload completed: {Created} created, {Updated} updated",
                    result.GuestsCreated, result.GuestsUpdated);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Transaction failed during guest upload");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Guest upload failed");
            result.Errors.Add($"업로드 실패: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.Errors.Add($"상세: {ex.InnerException.Message}");
            }
        }

        return result;
    }
}
