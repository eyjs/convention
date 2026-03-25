using LocalRAG.DTOs.SurveyModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalRAG.Interfaces
{
    public interface ISurveyService
    {
        Task<IEnumerable<SurveyDto>> GetAllSurveysAsync(string? surveyType = null);
        Task<IEnumerable<SurveyDto>> GetSurveysForUserAsync(int conventionId, int userId);
        Task<SurveyDto> GetSurveyAsync(int id);
        Task<SurveyDto> CreateSurveyAsync(SurveyCreateDto createDto);
        Task<SurveyDto> UpdateSurveyAsync(int id, SurveyCreateDto updateDto);
        Task SubmitSurveyAsync(int surveyId, SurveySubmissionDto submissionDto, int userId);
        Task<SurveyStatsDto> GetSurveyStatsAsync(int id);
        Task<SurveyResponseDto> GetUserSurveyResponseAsync(int surveyId, int userId);
        Task<(bool Success, string Message)> DeleteSurveyAsync(int id);
        Task<List<IndividualResponseDto>> GetSurveyResponsesAsync(int surveyId);
        Task<byte[]> ExportSurveyResponsesAsync(int surveyId);
    }
}