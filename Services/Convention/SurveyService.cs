using Microsoft.EntityFrameworkCore;
using LocalRAG.DTOs.SurveyModels;
using LocalRAG.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LocalRAG.Interfaces;
using LocalRAG.Entities.Action;
using LocalRAG.Repositories;
using System.Text.Json;
using OfficeOpenXml;

namespace LocalRAG.Services.Convention
{
    public class SurveyService : ISurveyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SurveyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SurveyDto>> GetAllSurveysAsync()
        {
            var surveys = await _unitOfWork.Surveys.Query
                .AsNoTracking()
                .OrderByDescending(s => s.Id)
                .Select(s => new SurveyDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    ResponseCount = s.Responses.Count,
                    CreatedAt = s.CreatedAt,
                    ConventionId = s.ConventionId
                })
                .ToListAsync();

            return surveys;
        }

        public async Task<SurveyDto> GetSurveyAsync(int id)
        {
            var survey = await _unitOfWork.Surveys.Query
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .Include(s => s.Responses)
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
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                ResponseCount = survey.Responses?.Count ?? 0,
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
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
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

            await _unitOfWork.Surveys.AddAsync(survey);
            await _unitOfWork.SaveChangesAsync();

            // TODO: domain boundary violation — ConventionAction 생성은 Action 도메인 영역
            if (survey.ConventionId.HasValue)
            {
                var conventionAction = new ConventionAction
                {
                    ConventionId = survey.ConventionId.Value,
                    Title = survey.Title,
                    Description = $"'{survey.Title}' 설문조사에 참여해주세요.",
                    BehaviorType = BehaviorType.ModuleLink,
                    TargetModuleId = survey.Id,
                    IsActive = survey.IsActive,
                    MapsTo = $"/surveys/{survey.Id}", // A sensible default route
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.ConventionActions.AddAsync(conventionAction);
                await _unitOfWork.SaveChangesAsync();
            }

            return await GetSurveyAsync(survey.Id);
        }

        public async Task<SurveyDto> UpdateSurveyAsync(int id, SurveyCreateDto updateDto)
        {
            var survey = await _unitOfWork.Surveys.Query
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
            survey.StartDate = updateDto.StartDate;
            survey.EndDate = updateDto.EndDate;
            survey.ConventionId = updateDto.ConventionId;

            // --- 질문 업데이트 ---
            var existingQuestionIds = survey.Questions.Select(q => q.Id).ToList();
            var updatedQuestionIds = updateDto.Questions.Where(q => q.Id > 0).Select(q => q.Id).ToList();

            // updateDto에 없는 질문 제거
            foreach (var questionToRemove in survey.Questions.Where(q => !updatedQuestionIds.Contains(q.Id)).ToList())
            {
                _unitOfWork.SurveyQuestions.Remove(questionToRemove);
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
                    _unitOfWork.QuestionOptions.Remove(optionToRemove);
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

            await _unitOfWork.SaveChangesAsync();

            return await GetSurveyAsync(survey.Id);
        }

        public async Task SubmitSurveyAsync(int surveyId, SurveySubmissionDto submissionDto, int userId)
        {
            var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
            if (survey == null)
            {
                throw new KeyNotFoundException("Survey not found.");
            }

            // 날짜 범위 검증
            var now = DateTime.UtcNow;
            if (survey.StartDate.HasValue && now < survey.StartDate.Value)
            {
                throw new InvalidOperationException("설문 응답 기간이 아직 시작되지 않았습니다.");
            }
            if (survey.EndDate.HasValue && now > survey.EndDate.Value)
            {
                throw new InvalidOperationException("설문 응답 기간이 종료되었습니다.");
            }

            var user = await _unitOfWork.Users.Query.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. SurveyResponse를 찾거나 새로 생성 (Idempotent)
                var response = await _unitOfWork.SurveyResponses.Query
                    .Include(r => r.Details)
                    .FirstOrDefaultAsync(r => r.SurveyId == surveyId && r.UserId == userId);

                if (response == null)
                {
                    response = new SurveyResponse
                    {
                        SurveyId = surveyId,
                        UserId = userId,
                    };
                    await _unitOfWork.SurveyResponses.AddAsync(response);
                }
                else
                {
                    // 기존 답변 삭제
                    _unitOfWork.SurveyResponseDetails.RemoveRange(response.Details);
                }

                response.SubmittedAt = DateTime.UtcNow; // 제출 시간 업데이트

                // 새로운 답변 추가
                AddAnswerDetails(response, submissionDto);

                // 2. ConventionAction 연동 로직
                await UpdateConventionActionStatusAsync(survey, submissionDto, userId);

                // 3. 모든 변경사항을 한 번에 커밋
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

        }

        public async Task<SurveyStatsDto> GetSurveyStatsAsync(int id)
        {
            var survey = await _unitOfWork.Surveys.Query
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

            var allDetails = survey.Responses.SelectMany(r => r.Details).ToList();
            foreach (var question in survey.Questions.OrderBy(q => q.OrderIndex))
            {
                var questionStats = BuildQuestionStats(question, allDetails);
                stats.QuestionStats.Add(questionStats);
            }

            return stats;
        }

        public async Task<SurveyResponseDto> GetUserSurveyResponseAsync(int surveyId, int userId)
        {
            var response = await _unitOfWork.SurveyResponses.Query
                .Include(r => r.Details)
                .FirstOrDefaultAsync(r => r.SurveyId == surveyId && r.UserId == userId);

            if (response == null)
            {
                return null;
            }

            return new SurveyResponseDto
            {
                Id = response.Id,
                SurveyId = response.SurveyId,
                UserId = response.UserId,
                SubmittedAt = response.SubmittedAt,
                Answers = response.Details.Select(d => new SurveyResponseDetailDto
                {
                    QuestionId = d.QuestionId,
                    SelectedOptionId = d.SelectedOptionId,
                    AnswerText = d.AnswerText
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message)> DeleteSurveyAsync(int id)
        {
            var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
            if (survey == null)
            {
                return (false, "설문을 찾을 수 없습니다.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 연관 ConventionAction + UserActionStatus + ActionSubmission 삭제
                var relatedActions = await _unitOfWork.ConventionActions.Query
                    .Where(a => a.BehaviorType == BehaviorType.ModuleLink && a.TargetModuleId == id)
                    .ToListAsync();

                foreach (var action in relatedActions)
                {
                    var statuses = await _unitOfWork.UserActionStatuses.Query
                        .Where(s => s.ConventionActionId == action.Id)
                        .ToListAsync();
                    _unitOfWork.UserActionStatuses.RemoveRange(statuses);

                    var submissions = await _unitOfWork.ActionSubmissions.Query
                        .Where(s => s.ConventionActionId == action.Id)
                        .ToListAsync();
                    _unitOfWork.ActionSubmissions.RemoveRange(submissions);

                    _unitOfWork.ConventionActions.Remove(action);
                }

                // 설문 응답 상세 + 응답 삭제
                var responses = await _unitOfWork.SurveyResponses.Query
                    .Include(r => r.Details)
                    .Where(r => r.SurveyId == id)
                    .ToListAsync();

                foreach (var response in responses)
                {
                    _unitOfWork.SurveyResponseDetails.RemoveRange(response.Details);
                    _unitOfWork.SurveyResponses.Remove(response);
                }

                // 질문 옵션 + 질문 삭제
                var questions = await _unitOfWork.SurveyQuestions.Query
                    .Include(q => q.Options)
                    .Where(q => q.SurveyId == id)
                    .ToListAsync();

                foreach (var question in questions)
                {
                    _unitOfWork.QuestionOptions.RemoveRange(question.Options);
                    _unitOfWork.SurveyQuestions.Remove(question);
                }

                _unitOfWork.Surveys.Remove(survey);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (true, "설문이 삭제되었습니다.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<IndividualResponseDto>> GetSurveyResponsesAsync(int surveyId)
        {
            var survey = await _unitOfWork.Surveys.Query
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .Include(s => s.Responses)
                .ThenInclude(r => r.Details)
                .ThenInclude(d => d.SelectedOption)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
            {
                return new List<IndividualResponseDto>();
            }

            // 응답한 유저 ID 목록
            var userIds = survey.Responses.Select(r => r.UserId).Distinct().ToList();
            var users = await _unitOfWork.Users.Query
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();

            // UserConvention에서 GroupName 가져오기
            var groupMap = new Dictionary<int, string?>();
            if (survey.ConventionId.HasValue)
            {
                var userConventions = await _unitOfWork.UserConventions.Query
                    .AsNoTracking()
                    .Where(uc => uc.ConventionId == survey.ConventionId.Value && userIds.Contains(uc.UserId))
                    .Select(uc => new { uc.UserId, uc.GroupName })
                    .ToListAsync();
                groupMap = userConventions.ToDictionary(uc => uc.UserId, uc => (string?)uc.GroupName);
            }

            var userMap = users.ToDictionary(u => u.Id);

            var questionsOrdered = survey.Questions.OrderBy(q => q.OrderIndex).ToList();

            return survey.Responses.OrderByDescending(r => r.SubmittedAt).Select(r => new IndividualResponseDto
            {
                ResponseId = r.Id,
                UserId = r.UserId,
                UserName = userMap.ContainsKey(r.UserId) ? userMap[r.UserId].Name : "알 수 없음",
                GroupName = groupMap.ContainsKey(r.UserId) ? groupMap[r.UserId] : null,
                SubmittedAt = r.SubmittedAt,
                Answers = questionsOrdered.Select(q =>
                {
                    var details = r.Details.Where(d => d.QuestionId == q.Id).ToList();
                    return new IndividualAnswerDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        QuestionType = q.Type.ToString(),
                        AnswerText = details.FirstOrDefault(d => !string.IsNullOrEmpty(d.AnswerText))?.AnswerText,
                        SelectedOptions = details
                            .Where(d => d.SelectedOption != null)
                            .Select(d => d.SelectedOption!.OptionText)
                            .ToList()
                    };
                }).ToList()
            }).ToList();
        }

        public async Task<byte[]> ExportSurveyResponsesAsync(int surveyId)
        {
            var survey = await _unitOfWork.Surveys.Query
                .Include(s => s.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
            {
                return Array.Empty<byte>();
            }

            var responses = await GetSurveyResponsesAsync(surveyId);
            var questionsOrdered = survey.Questions.OrderBy(q => q.OrderIndex).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("설문 응답");

            // 헤더
            worksheet.Cells[1, 1].Value = "이름";
            worksheet.Cells[1, 2].Value = "그룹";
            worksheet.Cells[1, 3].Value = "제출일";
            for (int i = 0; i < questionsOrdered.Count; i++)
            {
                worksheet.Cells[1, 4 + i].Value = questionsOrdered[i].QuestionText;
            }

            // 데이터
            for (int row = 0; row < responses.Count; row++)
            {
                var resp = responses[row];
                worksheet.Cells[row + 2, 1].Value = resp.UserName;
                worksheet.Cells[row + 2, 2].Value = resp.GroupName ?? "";
                worksheet.Cells[row + 2, 3].Value = resp.SubmittedAt.ToString("yyyy-MM-dd HH:mm");

                for (int col = 0; col < questionsOrdered.Count; col++)
                {
                    var answer = resp.Answers.FirstOrDefault(a => a.QuestionId == questionsOrdered[col].Id);
                    if (answer == null) continue;

                    if (answer.SelectedOptions.Any())
                    {
                        worksheet.Cells[row + 2, 4 + col].Value = string.Join(", ", answer.SelectedOptions);
                    }
                    else if (!string.IsNullOrEmpty(answer.AnswerText))
                    {
                        worksheet.Cells[row + 2, 4 + col].Value = answer.AnswerText;
                    }
                }
            }

            // 헤더 스타일
            using var headerRange = worksheet.Cells[1, 1, 1, 3 + questionsOrdered.Count];
            headerRange.Style.Font.Bold = true;
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        private static void AddAnswerDetails(SurveyResponse response, SurveySubmissionDto submissionDto)
        {
            foreach (var answer in submissionDto.Answers)
            {
                if (answer.SelectedOptionIds != null && answer.SelectedOptionIds.Any())
                {
                    foreach (var optionId in answer.SelectedOptionIds)
                    {
                        response.Details.Add(new SurveyResponseDetail
                        {
                            QuestionId = answer.QuestionId,
                            SelectedOptionId = optionId
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(answer.AnswerText))
                {
                    response.Details.Add(new SurveyResponseDetail
                    {
                        QuestionId = answer.QuestionId,
                        AnswerText = answer.AnswerText
                    });
                }
            }
        }

        // TODO: domain boundary violation — ConventionAction/UserActionStatus는 Action 도메인 영역
        private async Task UpdateConventionActionStatusAsync(Survey survey, SurveySubmissionDto submissionDto, int userId)
        {
            var action = await _unitOfWork.ConventionActions.Query
                .FirstOrDefaultAsync(a => a.ConventionId == survey.ConventionId &&
                                          a.BehaviorType == BehaviorType.ModuleLink &&
                                          a.TargetModuleId == survey.Id);

            if (action != null)
            {
                // ActionSubmissions 테이블에 JSON 데이터 저장
                var submissionJson = JsonSerializer.Serialize(submissionDto);
                var actionSubmission = await _unitOfWork.ActionSubmissions.Query
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);

                if (actionSubmission == null)
                {
                    actionSubmission = new ActionSubmission
                    {
                        UserId = userId,
                        ConventionActionId = action.Id,
                        SubmissionDataJson = submissionJson,
                        SubmittedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.ActionSubmissions.AddAsync(actionSubmission);
                }
                else
                {
                    actionSubmission.SubmissionDataJson = submissionJson;
                    actionSubmission.UpdatedAt = DateTime.UtcNow;
                }

                // UserActionStatus를 완료로 표시
                var userActionStatus = await _unitOfWork.UserActionStatuses.Query
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);

                if (userActionStatus == null)
                {
                    userActionStatus = new UserActionStatus
                    {
                        UserId = userId,
                        ConventionActionId = action.Id,
                        IsComplete = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.UserActionStatuses.AddAsync(userActionStatus);
                }
                else
                {
                    userActionStatus.IsComplete = true;
                    userActionStatus.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        private static QuestionStatsDto BuildQuestionStats(SurveyQuestion question, List<SurveyResponseDetail> allDetails)
        {
            var questionStats = new QuestionStatsDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.Type.ToString()
            };

            var detailsForQuestion = allDetails.Where(d => d.QuestionId == question.Id).ToList();

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

            return questionStats;
        }
    }
}
