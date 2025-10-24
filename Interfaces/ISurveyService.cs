using System.Threading.Tasks;
using LocalRAG.DTOs.SurveyModels;
using LocalRAG.Entities;

namespace LocalRAG.Interfaces
{
    public interface ISurveyService
    {
        Task<int> SaveSurveyResponse(string actionType, int guestId, SurveyResponseDto responseDto);
        Task<SurveyResponse?> GetSurveyResponse(string actionType, int guestId);
    }
}
