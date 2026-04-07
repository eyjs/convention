using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 일정 템플릿 업로드 서비스 인터페이스
/// Excel 파일에서 일정 템플릿을 읽어 ConventionAction 생성
/// </summary>
public interface IScheduleTemplateUploadService
{
    /// <summary>
    /// 일정 템플릿이 담긴 Excel 파일을 업로드하여 ConventionAction 생성 (legacy)
    /// </summary>
    Task<ScheduleTemplateUploadResult> UploadScheduleTemplatesAsync(int conventionId, Stream excelStream);

    /// <summary>
    /// 일정 Excel을 파싱만 수행 (저장 X) — 미리보기용
    /// 충돌 감지 포함
    /// </summary>
    Task<ScheduleTemplatePreviewResult> PreviewScheduleTemplatesAsync(int conventionId, Stream excelStream);

    /// <summary>
    /// 미리보기 확인 후 최종 저장
    /// </summary>
    Task<ScheduleTemplateUploadResult> ConfirmScheduleTemplatesAsync(int conventionId, ScheduleTemplateConfirmRequest request);
}
