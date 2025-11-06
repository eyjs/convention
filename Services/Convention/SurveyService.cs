using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.DTOs.SurveyModels;
using LocalRAG.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LocalRAG.Interfaces;
using LocalRAG.Entities.Action;

namespace LocalRAG.Services.Convention
{
    public class SurveyService : ISurveyService
    {
        private readonly ConventionDbContext _context;

        public SurveyService(ConventionDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SurveyDto>> GetAllSurveysAsync()
        {
            var surveys = await _context.Surveys
                .AsNoTracking()
                .OrderByDescending(s => s.Id)
                .Select(s => new SurveyDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    ConventionId = s.ConventionId
                })
                .ToListAsync();

            return surveys;
        }

        public async Task<SurveyDto> GetSurveyAsync(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
            {
                return null;
            }

            return new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                IsActive = survey.IsActive,
                CreatedAt = survey.CreatedAt,
                ConventionId = survey.ConventionId,
                Questions = survey.Questions.OrderBy(q => q.OrderIndex).Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Type = q.Type.ToString(),
                    IsRequired = q.IsRequired,
                    OrderIndex = q.OrderIndex,
                    Options = q.Options.OrderBy(o => o.OrderIndex).Select(o => new OptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        OrderIndex = o.OrderIndex
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<SurveyDto> CreateSurveyAsync(SurveyCreateDto createDto)
        {
            var survey = new Survey
            {
                Title = createDto.Title,
                Description = createDto.Description,
                IsActive = createDto.IsActive,
                ConventionId = createDto.ConventionId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var qDto in createDto.Questions)
            {
                var question = new SurveyQuestion
                {
                    QuestionText = qDto.QuestionText,
                    Type = Enum.Parse<QuestionType>(qDto.Type),
                    IsRequired = qDto.IsRequired,
                    OrderIndex = qDto.OrderIndex
                };

                foreach (var oDto in qDto.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        OptionText = oDto.OptionText,
                        OrderIndex = oDto.OrderIndex
                    });
                }
                survey.Questions.Add(question);
            }

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();

            return await GetSurveyAsync(survey.Id);
        }

        public async Task<SurveyDto> UpdateSurveyAsync(int id, SurveyCreateDto updateDto)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
            {
                throw new KeyNotFoundException("Survey not found.");
            }

            survey.Title = updateDto.Title;
            survey.Description = updateDto.Description;
            survey.IsActive = updateDto.IsActive;
            survey.ConventionId = updateDto.ConventionId;

            // --- 질문 업데이트 ---
            var existingQuestionIds = survey.Questions.Select(q => q.Id).ToList();
            var updatedQuestionIds = updateDto.Questions.Where(q => q.Id > 0).Select(q => q.Id).ToList();

            // updateDto에 없는 질문 제거
            foreach (var questionToRemove in survey.Questions.Where(q => !updatedQuestionIds.Contains(q.Id)).ToList())
            {
                _context.SurveyQuestions.Remove(questionToRemove);
            }

            foreach (var qDto in updateDto.Questions)
            {
                var question = survey.Questions.FirstOrDefault(q => q.Id == qDto.Id);
                if (question == null || qDto.Id == 0) // 새 질문 (Id == 0은 새 질문을 의미)
                {
                    question = new SurveyQuestion
                    {
                        SurveyId = survey.Id,
                        QuestionText = qDto.QuestionText,
                        Type = Enum.Parse<QuestionType>(qDto.Type),
                        IsRequired = qDto.IsRequired,
                        OrderIndex = qDto.OrderIndex
                    };
                    survey.Questions.Add(question);
                }
                else // 기존 질문
                {
                    question.QuestionText = qDto.QuestionText;
                    question.Type = Enum.Parse<QuestionType>(qDto.Type);
                    question.IsRequired = qDto.IsRequired;
                    question.OrderIndex = qDto.OrderIndex;
                }

                // --- 이 질문에 대한 옵션 업데이트 ---
                var existingOptionIds = question.Options.Select(o => o.Id).ToList();
                var updatedOptionIds = qDto.Options.Where(o => o.Id > 0).Select(o => o.Id).ToList();

                // qDto에 없는 옵션 제거
                foreach (var optionToRemove in question.Options.Where(o => !updatedOptionIds.Contains(o.Id)).ToList())
                {
                    _context.QuestionOptions.Remove(optionToRemove);
                }

                foreach (var oDto in qDto.Options)
                {
                    var option = question.Options.FirstOrDefault(o => o.Id == oDto.Id);
                    if (option == null || oDto.Id == 0) // 새 옵션 (Id == 0은 새 옵션을 의미)
                    {
                        option = new QuestionOption
                        {
                            QuestionId = question.Id,
                            OptionText = oDto.OptionText,
                            OrderIndex = oDto.OrderIndex
                        };
                        question.Options.Add(option);
                    }
                    else // 기존 옵션
                    {
                        option.OptionText = oDto.OptionText;
                        option.OrderIndex = oDto.OrderIndex;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return await GetSurveyAsync(survey.Id);
        }

        public async Task SubmitSurveyAsync(int surveyId, SurveySubmissionDto submissionDto, string loginId)

        {

            var survey = await _context.Surveys.FindAsync(surveyId);

            if (survey == null)

            {

                throw new KeyNotFoundException("Survey not found.");

            }



            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.LoginId == loginId);

            if (user == null)

            {

                throw new KeyNotFoundException("User not found.");

            }

            var userId = user.Id; // Use the integer PK



            using var transaction = await _context.Database.BeginTransactionAsync();



            try

            {

                var response = new SurveyResponse

                {

                    SurveyId = surveyId,

                    UserId = userId, // Assign the integer PK

                    SubmittedAt = DateTime.UtcNow

                };



                foreach (var answer in submissionDto.Answers)

                {

                    if (answer.SelectedOptionIds != null && answer.SelectedOptionIds.Any())

                    {

                        foreach (var optionId in answer.SelectedOptionIds)

                        {

                            response.Details.Add(new ResponseDetail

                            {

                                QuestionId = answer.QuestionId,

                                SelectedOptionId = optionId

                            });

                        }

                    }

                    else if (!string.IsNullOrEmpty(answer.AnswerText))

                    {

                        response.Details.Add(new ResponseDetail

                        {

                            QuestionId = answer.QuestionId,

                            AnswerText = answer.AnswerText

                        });

                    }

                }



                _context.SurveyResponses.Add(response);

                await _context.SaveChangesAsync();



                // Generic Action 연동

                var surveyActions = await _context.ConventionActions

                    .Where(a => a.ActionType == "SURVEY" && a.ConventionId == survey.ConventionId)

                    .ToListAsync();



                var action = surveyActions.FirstOrDefault(a => a.MapsTo.Equals(survey.Id.ToString()));



                if (action != null)

                {

                    var userActionStatus = await _context.UserActionStatuses

                        .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id); // Compare with integer PK



                    if (userActionStatus == null)

                    {

                        userActionStatus = new UserActionStatus

                        {

                            UserId = userId, // Assign the integer PK

                            ConventionActionId = action.Id,

                            IsComplete = true,

                            UpdatedAt = DateTime.UtcNow

                        };

                        _context.UserActionStatuses.Add(userActionStatus);

                    }

                    else

                    {

                        userActionStatus.IsComplete = true;

                        userActionStatus.UpdatedAt = DateTime.UtcNow;
                    }
                    await _context.SaveChangesAsync();
                }
                await transaction.CommitAsync();

            }

            catch (Exception)

            {

                await transaction.RollbackAsync();

                throw;

            }

        }

        public async Task<SurveyStatsDto> GetSurveyStatsAsync(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .Include(s => s.Responses)
                .ThenInclude(r => r.Details)
                .ThenInclude(d => d.SelectedOption)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
            {
                return null;
            }

            var stats = new SurveyStatsDto
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                TotalResponses = survey.Responses.Count
            };

            foreach (var question in survey.Questions.OrderBy(q => q.OrderIndex))
            {
                var questionStats = new QuestionStatsDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    QuestionType = question.Type.ToString()
                };

                var detailsForQuestion = survey.Responses.SelectMany(r => r.Details).Where(d => d.QuestionId == question.Id).ToList();

                if (question.Type == QuestionType.SINGLE_CHOICE || question.Type == QuestionType.MULTIPLE_CHOICE)
                {
                    foreach (var option in question.Options.OrderBy(o => o.OrderIndex))
                    {
                        var count = detailsForQuestion.Count(d => d.SelectedOptionId == option.Id);
                        questionStats.Answers.Add(new AnswerStatDto
                        {
                            Answer = option.OptionText,
                            Count = count
                        });
                    }
                }
                else // SHORT_TEXT or LONG_TEXT
                {
                    var textAnswers = detailsForQuestion
                        .Where(d => !string.IsNullOrEmpty(d.AnswerText))
                        .Select(d => new AnswerStatDto { Answer = d.AnswerText, Count = 1 })
                        .ToList();
                    questionStats.Answers.AddRange(textAnswers);
                }

                stats.QuestionStats.Add(questionStats);
            }

            return stats;
        }
    }
}