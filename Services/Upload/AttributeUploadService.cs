using LocalRAG.DTOs.UploadModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Utilities;
using OfficeOpenXml;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 속성 업로드 서비스
/// Excel 형식:
/// - A열: 이름 (필수, 매칭에 사용)
/// - B열: 전화번호 (필수, 매칭에 사용)
/// - C열부터: 각 속성 (헤더=AttributeKey, 셀 값=AttributeValue)
///
/// 예시:
/// | 이름    | 전화번호      | 룸메이트 | 식이제한 | 알러지 |
/// | 홍길동  | 010-1234-5678 | 김철수   | 채식     | 없음   |
/// | 김영희  | 010-2345-6789 | -        | 없음     | 갑각류 |
///
/// 매칭 방식: 이름 + 전화번호(정규화)로 기존 참석자를 찾아서 속성 매핑
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

    public async Task<UserAttributeUploadResult> UploadAttributesAsync(int conventionId, Stream excelStream)
    {
        var result = new UserAttributeUploadResult();

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

            // 헤더 파싱 (1행) - C열(3열)부터 속성
            var headers = new List<string>();
            for (int col = 3; col <= colCount; col++) // 1-2열은 이름/전화번호(매칭용), 3열부터 속성
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
                    var userName = sheet.Cells[row, 1].Text?.Trim(); // A열: 이름
                    var phoneNumber = sheet.Cells[row, 2].Text?.Trim(); // B열: 전화번호

                    if (string.IsNullOrEmpty(userName))
                    {
                        result.Warnings.Add($"Row {row}: 이름이 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(phoneNumber))
                    {
                        result.Warnings.Add($"Row {row}: 전화번호가 비어있습니다. 건너뜁니다.");
                        continue;
                    }

                    // 전화번호 정규화 (하이픈, 공백 제거)
                    var normalizedPhone = PhoneNumberFormatter.Normalize(phoneNumber);

                    // 이름으로 먼저 후보 찾기
                    var candidateUsers = await _unitOfWork.Users
                        .FindAsync(u => u.Name == userName);

                    // 메모리에서 전화번호 정규화 비교
                    var user = candidateUsers.FirstOrDefault(u =>
                        !string.IsNullOrEmpty(u.Phone) &&
                        PhoneNumberFormatter.Normalize(u.Phone) == normalizedPhone);

                    if (user == null)
                    {
                        result.Warnings.Add($"Row {row}: '{userName}' + '{phoneNumber}'에 해당하는 참석자를 찾을 수 없습니다.");
                        continue;
                    }

                    // UserConvention 확인 (해당 행사에 참석자인지)
                    var userConventions = await _unitOfWork.UserConventions
                        .FindAsync(uc => uc.ConventionId == conventionId && uc.UserId == user.Id);
                    var userConvention = userConventions.FirstOrDefault();

                    if (userConvention == null)
                    {
                        result.Warnings.Add($"Row {row}: '{userName}'님은 이 행사의 참석자가 아닙니다.");
                        continue;
                    }

                    result.UsersProcessed++;

                    // 속성 처리 (3열부터 = C열부터)
                    for (int col = 3; col <= colCount; col++)
                    {
                        var attributeKey = headers[col - 3];
                        var attributeValue = sheet.Cells[row, col].Text?.Trim();

                        if (string.IsNullOrEmpty(attributeValue))
                            continue; // 빈 값은 건너뛰기

                        // 기존 속성 찾기
                        var existingAttribute = await _unitOfWork.GuestAttributes
                            .GetAttributeByKeyAsync(userConvention.UserId, attributeKey);

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
                                UserId = userConvention.UserId,
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

                // 한 번의 SaveChanges로 모든 변경사항 커밋 (암시적 트랜잭션)
                await _unitOfWork.SaveChangesAsync();

                result.Success = true;
                result.Statistics = statistics;

                _logger.LogInformation("Attribute upload completed: {Created} created, {Updated} updated for {Users} users",
                    result.AttributesCreated, result.AttributesUpdated, result.UsersProcessed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Attribute upload failed during processing");
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
