using System.Threading.Tasks;
using LocalRAG.Models;
using LocalRAG.Models.DTOs;

namespace LocalRAG.Interfaces
{
    public interface ISurveyService
    {
        Task<int> SaveSurveyResponse(string actionType, int guestId, SurveyResponseDto responseDto);
        Task<SurveyResponse?> GetSurveyResponse(string actionType, int guestId);
    }
}
