using LocalRAG.DTOs.SurveyModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalRAG.Interfaces
{
    public interface ISurveyService
    {
        Task<IEnumerable<SurveyDto>> GetAllSurveysAsync();
        Task<SurveyDto> GetSurveyAsync(int id);
        Task<SurveyDto> CreateSurveyAsync(SurveyCreateDto createDto);
        Task<SurveyDto> UpdateSurveyAsync(int id, SurveyCreateDto updateDto);
        Task SubmitSurveyAsync(int surveyId, SurveySubmissionDto submissionDto, int userId);
        Task<SurveyStatsDto> GetSurveyStatsAsync(int id);
        Task<SurveyResponseDto> GetUserSurveyResponseAsync(int surveyId, int userId);
    }
}