using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 속성 업로드 서비스 인터페이스
/// Excel 파일에서 참석자 메타정보를 읽어 GuestAttribute 생성/업데이트
/// </summary>
public interface IAttributeUploadService
{
    /// <summary>
    /// 참석자 속성 정보가 담긴 Excel 파일을 업로드하여 GuestAttribute 생성/업데이트
    /// </summary>
    /// <param name="conventionId">행사 ID</param>
    /// <param name="excelStream">Excel 파일 스트림</param>
    /// <returns>업로드 결과 (통계 정보 포함)</returns>
    Task<AttributeUploadResult> UploadAttributesAsync(int conventionId, Stream excelStream);
}
