using LocalRAG.DTOs.FormBuilder;
using LocalRAG.Entities.FormBuilder;

namespace LocalRAG.Interfaces;

public interface IFormBuilderService
{
    /// <summary>
    /// 폼 설계도 조회
    /// </summary>
    Task<FormDefinitionDto?> GetFormDefinitionAsync(int formDefinitionId);

    /// <summary>
    /// 사용자의 기존 응답 조회
    /// </summary>
    Task<string?> GetUserSubmissionAsync(int formDefinitionId, int userId);

    /// <summary>
    /// 폼 데이터 제출 (Create/Update)
    /// </summary>
    Task<bool> SubmitFormDataAsync(int formDefinitionId, int userId, string formDataJson, IFormFile? file, string? fileFieldKey);

    /// <summary>
    /// [관리자용] 특정 폼의 모든 제출 데이터 조회
    /// </summary>
    Task<List<FormSubmissionDto>> GetAllSubmissionsAsync(int formDefinitionId);

    /// <summary>
    /// 업로드된 파일 경로 검증
    /// </summary>
    bool ValidateFilePath(string path);

    /// <summary>
    /// 파일의 Content-Type 가져오기
    /// </summary>
    string GetContentType(string fileName);
}
