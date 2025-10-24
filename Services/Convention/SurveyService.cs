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

        public async Task<int> SaveSurveyResponse(string actionType, int guestId, SurveyResponseDto responseDto)
        {
            var action = await _context.ConventionActions.FirstOrDefaultAsync(a => a.ActionType == actionType);
            if (action == null)
            {
                throw new KeyNotFoundException("Action not found.");
            }

            var response = new SurveyResponse
            {
                ConventionActionId = action.Id,
                GuestId = guestId,
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

            var status = await _context.GuestActionStatuses.FirstOrDefaultAsync(s => s.GuestId == guestId && s.ConventionActionId == action.Id);
            if (status == null)
            {
                status = new GuestActionStatus
                {
                    GuestId = guestId,
                    ConventionActionId = action.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.GuestActionStatuses.Add(status);
            }

            status.IsComplete = true;
            status.CompletedAt = DateTime.UtcNow;
            status.ResponseDataJson = System.Text.Json.JsonSerializer.Serialize(new { surveyResponseId = response.Id });
            status.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return response.Id;
        }

        public async Task<SurveyResponse?> GetSurveyResponse(string actionType, int guestId)
        {
            var action = await _context.ConventionActions.FirstOrDefaultAsync(a => a.ActionType == actionType);
            if (action == null)
            {
                throw new KeyNotFoundException("Action not found.");
            }

            var response = await _context.SurveyResponses
                .Include(r => r.Answers)
                .FirstOrDefaultAsync(r => r.ConventionActionId == action.Id && r.GuestId == guestId);

            return response;
        }
    }
}
