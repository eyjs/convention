using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 옵션투어 업로드 서비스 인터페이스
/// </summary>
public interface IOptionTourUploadService
{
    /// <summary>
    /// 옵션투어 및 참석자 매핑 정보를 업로드
    /// </summary>
    /// <param name="conventionId">행사 ID</param>
    /// <param name="request">옵션투어 업로드 요청 (옵션 정보 + 참석자 매핑)</param>
    /// <returns>업로드 결과</returns>
    Task<OptionTourUploadResult> UploadOptionToursAsync(int conventionId, OptionTourUploadRequest request);
}
