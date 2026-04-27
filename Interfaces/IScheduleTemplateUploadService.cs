using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 일정 템플릿 업로드 서비스 인터페이스
/// Excel 파일에서 일정 템플릿을 읽어 ScheduleTemplate/ScheduleItem 생성
/// 멀티시트 지원: 시트 1개 = ScheduleTemplate 1개
/// </summary>
public interface IScheduleTemplateUploadService
{
    /// <summary>
    /// 일정 템플릿이 담긴 Excel 파일을 업로드하여 저장 (legacy 단순 업로드)
    /// </summary>
    Task<ScheduleTemplateUploadResult> UploadScheduleTemplatesAsync(int conventionId, Stream excelStream);

    /// <summary>
    /// 일정 Excel을 파싱만 수행 (저장 X) — 미리보기용
    /// 멀티시트 지원: Sheets 배열로 시트별 미리보기 제공
    /// 충돌 감지 포함
    /// </summary>
    Task<ScheduleTemplatePreviewResult> PreviewScheduleTemplatesAsync(int conventionId, Stream excelStream);

    /// <summary>
    /// 미리보기 확인 후 단일 시트 최종 저장 (기존 호환)
    /// </summary>
    Task<ScheduleTemplateUploadResult> ConfirmScheduleTemplatesAsync(int conventionId, ScheduleTemplateConfirmRequest request, bool replaceAll = false);

    /// <summary>
    /// 멀티시트 미리보기 확인 후 전체 시트 일괄 저장
    /// </summary>
    Task<ScheduleTemplateUploadResult> ConfirmMultiSheetAsync(int conventionId, MultiSheetConfirmRequest request, bool replaceAll = false);
}
