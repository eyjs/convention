using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 일정 템플릿 업로드 서비스 인터페이스
/// Excel 파일에서 일정 템플릿을 읽어 ConventionAction 생성
/// </summary>
public interface IScheduleTemplateUploadService
{
    /// <summary>
    /// 일정 템플릿이 담긴 Excel 파일을 업로드하여 ConventionAction 생성
    /// </summary>
    /// <param name="conventionId">행사 ID</param>
    /// <param name="excelStream">Excel 파일 스트림</param>
    /// <returns>업로드 결과</returns>
    Task<ScheduleTemplateUploadResult> UploadScheduleTemplatesAsync(int conventionId, Stream excelStream);
}
