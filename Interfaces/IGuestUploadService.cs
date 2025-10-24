using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 참석자 업로드 서비스 인터페이스
/// Excel 파일에서 참석자 정보를 읽어 Guest 엔티티 생성/업데이트
/// </summary>
public interface IGuestUploadService
{
    /// <summary>
    /// 참석자 정보가 담긴 Excel 파일을 업로드하여 Guest 생성/업데이트
    /// </summary>
    /// <param name="conventionId">행사 ID</param>
    /// <param name="excelStream">Excel 파일 스트림</param>
    /// <returns>업로드 결과</returns>
    Task<GuestUploadResult> UploadGuestsAsync(int conventionId, Stream excelStream);
}
