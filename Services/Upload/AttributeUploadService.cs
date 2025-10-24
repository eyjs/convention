using LocalRAG.DTOs.UploadModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using OfficeOpenXml;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 속성 업로드 서비스
/// Excel 형식:
/// - A열 (또는 1-2열): 참석자 식별 정보 (이름, 전화번호 등)
/// - 나머지 열: 각 속성 (헤더=AttributeKey, 셀 값=AttributeValue)
///
/// 예시:
/// | 이름    | 전화번호      | 나이 | 성별 | 직급   | 선호음식 |
/// | 홍길동  | 010-1234-5678 | 30  | 남   | 과장   | 한식     |
///
/// 통계 정보를 생성하여 같은 속성값을 가진 사람들의 분포를 파악
/// </summary>
public class AttributeUploadService : IAttributeUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttributeUploadService> _logger;

    public AttributeUploadService(
        IUnitOfWork unitOfWork,
        ILogger<AttributeUploadService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<AttributeUploadResult> UploadAttributesAsync(int conventionId, Stream excelStream)
    {
        var result = new AttributeUploadResult();

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
            var colCount = sheet.Dimension.Columns;

            if (colCount < 3)
            {
                result.Errors.Add("속성 열이 부족합니다. 최소 3개 열이 필요합니다. (이름|전화번호|속성1...)");
                return result;
            }

            _logger.LogInformation("Processing {RowCount} rows and {ColCount} columns from Excel", rowCount, colCount);

            // 헤더 파싱 (1행)
            var headers = new List<string>();
            for (int col = 3; col <= colCount; col++) // 1-2열은 참석자 식별정보, 3열부터 속성
            {
                var header = sheet.Cells[1, col].Text?.Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    headers.Add(header);
                }
                else
                {
                    headers.Add($"Column_{col}"); // 헤더 없으면 기본값
                }
            }

            _logger.LogInformation("Attribute headers: {Headers}", string.Join(", ", headers));

            // 트랜잭션으로 처리
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 통계용 딕셔너리
                var statistics = new Dictionary<string, Dictionary<string, int>>();
                foreach (var header in headers)
                {
                    statistics[header] = new Dictionary<string, int>();
                }

                for (int row = 2; row <= rowCount; row++) // 1행은 헤더
                {
                    var guestName = sheet.Cells[row, 1].Text?.Trim();
                    var telephone = sheet.Cells[row, 2].Text?.Trim();

                    if (string.IsNullOrEmpty(guestName) || string.IsNullOrEmpty(telephone))
                    {
                        result.Warnings.Add($"Row {row}: 이름 또는 전화번호가 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    // Guest 찾기
                    var guests = await _unitOfWork.Guests
                        .FindAsync(g => g.ConventionId == conventionId
                                     && g.GuestName == guestName
                                     && g.Telephone == telephone);

                    var guest = guests.FirstOrDefault();

                    if (guest == null)
                    {
                        result.Warnings.Add($"Row {row}: 참석자를 찾을 수 없습니다. ({guestName}, {telephone})");
                        continue;
                    }

                    result.GuestsProcessed++;

                    // 속성 처리 (3열부터)
                    for (int col = 3; col <= colCount; col++)
                    {
                        var attributeKey = headers[col - 3];
                        var attributeValue = sheet.Cells[row, col].Text?.Trim();

                        if (string.IsNullOrEmpty(attributeValue))
                            continue; // 빈 값은 건너뛰기

                        // 기존 속성 찾기
                        var existingAttribute = await _unitOfWork.GuestAttributes
                            .GetAttributeByKeyAsync(guest.Id, attributeKey);

                        if (existingAttribute != null)
                        {
                            // 업데이트
                            existingAttribute.AttributeValue = attributeValue;
                            _unitOfWork.GuestAttributes.Update(existingAttribute);
                            result.AttributesUpdated++;
                        }
                        else
                        {
                            // 생성
                            var newAttribute = new GuestAttribute
                            {
                                GuestId = guest.Id,
                                AttributeKey = attributeKey,
                                AttributeValue = attributeValue
                            };

                            await _unitOfWork.GuestAttributes.AddAsync(newAttribute);
                            result.AttributesCreated++;
                        }

                        // 통계 업데이트
                        if (!statistics[attributeKey].ContainsKey(attributeValue))
                        {
                            statistics[attributeKey][attributeValue] = 0;
                        }
                        statistics[attributeKey][attributeValue]++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.Success = true;
                result.Statistics = statistics;

                _logger.LogInformation("Attribute upload completed: {Created} created, {Updated} updated for {Guests} guests",
                    result.AttributesCreated, result.AttributesUpdated, result.GuestsProcessed);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Transaction failed during attribute upload");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Attribute upload failed");
            result.Errors.Add($"업로드 실패: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.Errors.Add($"상세: {ex.InnerException.Message}");
            }
        }

        return result;
    }
}
