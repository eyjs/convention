using System.Threading.Tasks;
using LocalRAG.Data;
using LocalRAG.DTOs.SurveyModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Convention
{
    public class SurveyService : ISurveyService
    {
        private readonly ConventionDbContext _context;

        public SurveyService(ConventionDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveSurveyResponse(string actionType, int userId, SurveyResponseDto responseDto)
        {
            var action = await _context.ConventionActions.FirstOrDefaultAsync(a => a.ActionType == actionType);
            if (action == null)
            {
                throw new KeyNotFoundException("Action not found.");
            }

            var response = new SurveyResponse
            {
                ConventionActionId = action.Id,
                UserId = userId,
                SubmittedAt = DateTime.UtcNow
            };

            foreach (var answerDto in responseDto.Answers)
            {
                response.Answers.Add(new SurveyResponseAnswer
                {
                    Question = answerDto.Question,
                    Answer = answerDto.Answer
                });
            }

            _context.SurveyResponses.Add(response);

            var status = await _context.UserActionStatuses.FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);
            if (status == null)
            {
                status = new UserActionStatus
                {
                    UserId = userId,
                    ConventionActionId = action.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserActionStatuses.Add(status);
            }

            status.IsComplete = true;
            status.CompletedAt = DateTime.UtcNow;
            status.ResponseDataJson = System.Text.Json.JsonSerializer.Serialize(new { surveyResponseId = response.Id });
            status.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return response.Id;
        }

        public async Task<SurveyResponse?> GetSurveyResponse(string actionType, int userId)
        {
            var action = await _context.ConventionActions.FirstOrDefaultAsync(a => a.ActionType == actionType);
            if (action == null)
            {
                throw new KeyNotFoundException("Action not found.");
            }

            var response = await _context.SurveyResponses
                .Include(r => r.Answers)
                .FirstOrDefaultAsync(r => r.ConventionActionId == action.Id && r.UserId == userId);

            return response;
        }
    }
}
