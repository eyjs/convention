using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocalRAG.DTOs.SurveyModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Entities.Action;
using LocalRAG.Repositories;
using System.Text.Json;
using OfficeOpenXml;

namespace LocalRAG.Services.Convention
{
    public class SurveyService : ISurveyService
    {
        private static readonly TimeZoneInfo KstTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SurveyService> _logger;

        public SurveyService(IUnitOfWork unitOfWork, ILogger<SurveyService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<SurveyDto>> GetAllSurveysAsync(string? surveyType = null)
        {
            var query = _unitOfWork.Surveys.Query.AsNoTracking();

            if (!string.IsNullOrEmpty(surveyType) && Enum.TryParse<SurveyType>(surveyType, true, out var typeFilter))
            {
                query = query.Where(s => s.SurveyType == typeFilter);
            }

            var surveys = await query
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
                    ConventionId = s.ConventionId,
                    SurveyType = s.SurveyType.ToString()
                })
                .ToListAsync();

            return surveys;
        }

        public async Task<IEnumerable<SurveyDto>> GetSurveysForUserAsync(int conventionId, int userId)
        {
            // 행사 멤버십 확인
            var isMember = await _unitOfWork.UserConventions.Query
                .AnyAsync(uc => uc.ConventionId == conventionId && uc.UserId == userId);
            if (!isMember)
            {
                return Enumerable.Empty<SurveyDto>();
            }

            // 설문 날짜는 KST(UTC+9)로 저장되므로 KST 기준으로 비교
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, KstTimeZone);

            var surveys = await _unitOfWork.Surveys.Query
                .AsNoTracking()
                .Where(s => s.ConventionId == conventionId && s.IsActive)
                .Where(s => !s.StartDate.HasValue || s.StartDate <= now)
                .Where(s => !s.EndDate.HasValue || s.EndDate >= now)
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
                    ConventionId = s.ConventionId,
                    SurveyType = s.SurveyType.ToString(),
                    IsCompleted = s.Responses.Any(r => r.UserId == userId)
                })
                .ToListAsync();

            return surveys;
        }

        public async Task<SurveyDto> GetSurveyAsync(int id)
        {
            var survey = await _unitOfWork.Surveys.Query
                .Include(s => s.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.Options.OrderBy(o => o.OrderIndex))
                .ThenInclude(o => o.OptionTour)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
            {
                _logger.LogWarning("Survey {SurveyId} not found", id);
                return null;
            }

            var responseCount = await _unitOfWork.SurveyResponses.Query
                .CountAsync(r => r.SurveyId == id);

            var dto = MapToSurveyDto(survey);
            dto.ResponseCount = responseCount;
            return dto;
        }

        public async Task<SurveyDto> CreateSurveyAsync(SurveyCreateDto createDto)
        {
            ValidateSurveyDto(createDto);

            var survey = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var newSurvey = new Survey
                {
                    Title = createDto.Title,
                    Description = createDto.Description,
                    IsActive = createDto.IsActive,
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
                    ConventionId = createDto.ConventionId,
                    SurveyType = ParseSurveyType(createDto.SurveyType),
                    CreatedAt = DateTime.UtcNow
                };

                // 1단계: 질문/옵션 생성 (ParentOptionId 제외)
                // TempKey → Option 매핑을 위해 추적
                var tempKeyToOption = new Dictionary<int, QuestionOption>();

                foreach (var qDto in createDto.Questions)
                {
                    var question = new SurveyQuestion
                    {
                        QuestionText = qDto.QuestionText,
                        Type = ParseQuestionType(qDto.Type),
                        IsRequired = qDto.IsRequired,
                        OrderIndex = qDto.OrderIndex
                    };

                    foreach (var oDto in qDto.Options)
                    {
                        var option = new QuestionOption
                        {
                            OptionText = oDto.OptionText,
                            OrderIndex = oDto.OrderIndex,
                            IsTerminating = oDto.IsTerminating,
                            OptionTourId = oDto.OptionTourId
                        };
                        question.Options.Add(option);

                        if (oDto.TempKey.HasValue && oDto.TempKey.Value < 0)
                        {
                            tempKeyToOption[oDto.TempKey.Value] = option;
                        }
                    }
                    newSurvey.Questions.Add(question);
                }

                await _unitOfWork.Surveys.AddAsync(newSurvey);
                await _unitOfWork.SaveChangesAsync();

                // 2단계: 꼬리질문 ParentOptionId 해소
                ResolveParentOptionIds(newSurvey, createDto.Questions, tempKeyToOption);

                await CreateSurveyActionAsync(newSurvey);
                return newSurvey;
            });

            _logger.LogInformation("Survey {SurveyId} '{Title}' created", survey.Id, survey.Title);
            return await GetSurveyAsync(survey.Id);
        }

        public async Task<SurveyDto> UpdateSurveyAsync(int id, SurveyCreateDto updateDto)
        {
            ValidateSurveyDto(updateDto);

            int surveyId = 0;
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var survey = await _unitOfWork.Surveys.GetSurveyWithQuestionsAndOptionsAsync(id);

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
                survey.SurveyType = ParseSurveyType(updateDto.SurveyType);

                var tempKeyToOption = new Dictionary<int, QuestionOption>();
                UpdateQuestions(survey, updateDto.Questions, tempKeyToOption);
                await _unitOfWork.SaveChangesAsync();

                ResolveParentOptionIds(survey, updateDto.Questions, tempKeyToOption);
                surveyId = survey.Id;

                await SyncSurveyActionAsync(survey);
            });

            _logger.LogInformation("Survey {SurveyId} updated", id);
            return await GetSurveyAsync(surveyId);
        }

        public async Task SubmitSurveyAsync(int surveyId, SurveySubmissionDto submissionDto, int userId)
        {
            var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
            if (survey == null)
            {
                throw new KeyNotFoundException("Survey not found.");
            }

            // 설문 날짜는 KST(UTC+9)로 저장되므로 KST 기준으로 비교
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, KstTimeZone);
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

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
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
                    _unitOfWork.SurveyResponseDetails.RemoveRange(response.Details);
                }

                response.SubmittedAt = DateTime.UtcNow;
                AddAnswerDetails(response, submissionDto);

                await UpdateSurveyActionStatusAsync(survey, submissionDto, userId);

                if (survey.SurveyType == SurveyType.OPTION_TOUR)
                {
                    await SyncUserOptionToursAsync(survey, submissionDto, userId);
                }
            });

            _logger.LogInformation("Survey {SurveyId} submitted by user {UserId}", surveyId, userId);
        }

        public async Task<SurveyStatsDto> GetSurveyStatsAsync(int id)
        {
            var survey = await _unitOfWork.Surveys.GetSurveyWithAllDataAsync(id);

            if (survey == null)
            {
                _logger.LogWarning("Survey {SurveyId} not found for stats", id);
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
                _logger.LogWarning("Survey {SurveyId} not found for deletion", id);
                return (false, "설문을 찾을 수 없습니다.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await DeleteSurveyActionsAsync(id);

                // Survey 삭제 — cascade delete로 Questions/Options/Responses/Details 자동 처리
                _unitOfWork.Surveys.Remove(survey);
            });

            _logger.LogInformation("Survey {SurveyId} '{Title}' deleted", id, survey.Title);
            return (true, "설문이 삭제되었습니다.");
        }

        public async Task<List<IndividualResponseDto>> GetSurveyResponsesAsync(int surveyId)
        {
            var survey = await _unitOfWork.Surveys.GetSurveyWithAllDataAsync(surveyId);

            if (survey == null)
            {
                return new List<IndividualResponseDto>();
            }

            var userIds = survey.Responses.Select(r => r.UserId).Distinct().ToList();
            var users = await _unitOfWork.Users.Query
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();

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

            // ILookup으로 O(1) 접근 (O(n²) 제거)
            var detailLookup = survey.Responses.ToDictionary(
                r => r.Id,
                r => r.Details.ToLookup(d => d.QuestionId));

            return survey.Responses.OrderByDescending(r => r.SubmittedAt).Select(r => new IndividualResponseDto
            {
                ResponseId = r.Id,
                UserId = r.UserId,
                UserName = userMap.ContainsKey(r.UserId) ? userMap[r.UserId].Name : "알 수 없음",
                GroupName = groupMap.ContainsKey(r.UserId) ? groupMap[r.UserId] : null,
                SubmittedAt = r.SubmittedAt,
                Answers = questionsOrdered.Select(q =>
                {
                    var details = detailLookup[r.Id][q.Id].ToList();
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
            var responses = await GetSurveyResponsesAsync(surveyId);
            if (responses.Count == 0)
            {
                return Array.Empty<byte>();
            }

            // 질문 순서는 첫 번째 응답의 Answers에서 추출 (이미 정렬됨)
            var questions = responses[0].Answers;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("설문 응답");

            // 헤더
            worksheet.Cells[1, 1].Value = "이름";
            worksheet.Cells[1, 2].Value = "그룹";
            worksheet.Cells[1, 3].Value = "제출일";
            for (int i = 0; i < questions.Count; i++)
            {
                worksheet.Cells[1, 4 + i].Value = questions[i].QuestionText;
            }

            // 데이터
            for (int row = 0; row < responses.Count; row++)
            {
                var resp = responses[row];
                worksheet.Cells[row + 2, 1].Value = resp.UserName;
                worksheet.Cells[row + 2, 2].Value = resp.GroupName ?? "";
                worksheet.Cells[row + 2, 3].Value = resp.SubmittedAt.ToString("yyyy-MM-dd HH:mm");

                for (int col = 0; col < resp.Answers.Count; col++)
                {
                    var answer = resp.Answers[col];
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
            using var headerRange = worksheet.Cells[1, 1, 1, 3 + questions.Count];
            headerRange.Style.Font.Bold = true;
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        #region Private Helpers — Validation

        private static void ValidateSurveyDto(SurveyCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ArgumentException("설문 제목은 필수입니다.");
            }

            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
            {
                throw new ArgumentException("시작일이 종료일보다 늦을 수 없습니다.");
            }

            // OPTION_TOUR일 때 StartDate/EndDate 필수
            if (ParseSurveyType(dto.SurveyType) == SurveyType.OPTION_TOUR)
            {
                if (!dto.StartDate.HasValue || !dto.EndDate.HasValue)
                {
                    throw new ArgumentException("옵션투어 설문은 시작일과 종료일이 필수입니다.");
                }
            }

            if (dto.Questions == null || dto.Questions.Count == 0)
            {
                throw new ArgumentException("질문이 1개 이상 필요합니다.");
            }

            foreach (var q in dto.Questions)
            {
                if (string.IsNullOrWhiteSpace(q.QuestionText))
                {
                    throw new ArgumentException("질문 텍스트는 비어있을 수 없습니다.");
                }

                if (!Enum.TryParse<QuestionType>(q.Type, out var questionType))
                {
                    throw new ArgumentException($"유효하지 않은 질문 유형: {q.Type}");
                }

                if ((questionType == QuestionType.SINGLE_CHOICE || questionType == QuestionType.MULTIPLE_CHOICE)
                    && (q.Options == null || q.Options.Count == 0))
                {
                    throw new ArgumentException($"선택형 질문 '{q.QuestionText}'에는 옵션이 1개 이상 필요합니다.");
                }
            }
        }

        private static QuestionType ParseQuestionType(string type)
        {
            if (!Enum.TryParse<QuestionType>(type, out var result))
            {
                throw new ArgumentException($"유효하지 않은 질문 유형: {type}");
            }
            return result;
        }

        private static SurveyType ParseSurveyType(string? type)
        {
            if (string.IsNullOrEmpty(type) || !Enum.TryParse<SurveyType>(type, true, out var result))
            {
                return SurveyType.GENERAL;
            }
            return result;
        }

        #endregion

        #region Private Helpers — ConventionAction 연동

        private async Task CreateSurveyActionAsync(Survey survey)
        {
            if (!survey.ConventionId.HasValue) return;

            var conventionAction = new ConventionAction
            {
                ConventionId = survey.ConventionId.Value,
                Title = survey.Title,
                Description = $"'{survey.Title}' 설문조사에 참여해주세요.",
                BehaviorType = BehaviorType.ModuleLink,
                TargetModuleId = survey.Id,
                IsActive = survey.IsActive,
                MapsTo = $"/surveys/{survey.Id}",
                ActionCategory = "CARD",
                TargetLocation = "HOME_CONTENT_TOP",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.ConventionActions.AddAsync(conventionAction);
        }

        /// <summary>
        /// 설문 수정 시 연결된 ConventionAction도 동기화 (없으면 생성)
        /// </summary>
        private async Task SyncSurveyActionAsync(Survey survey)
        {
            if (!survey.ConventionId.HasValue) return;

            var action = await _unitOfWork.ConventionActions.Query
                .FirstOrDefaultAsync(a => a.ConventionId == survey.ConventionId &&
                                          a.BehaviorType == BehaviorType.ModuleLink &&
                                          a.TargetModuleId == survey.Id);

            if (action == null)
            {
                await CreateSurveyActionAsync(survey);
                return;
            }

            action.Title = survey.Title;
            action.Description = $"'{survey.Title}' 설문조사에 참여해주세요.";
            action.IsActive = survey.IsActive;
            action.MapsTo = $"/surveys/{survey.Id}";
            action.ActionCategory ??= "CARD";
            action.TargetLocation ??= "HOME_CONTENT_TOP";
            action.UpdatedAt = DateTime.UtcNow;
        }

        private async Task UpdateSurveyActionStatusAsync(Survey survey, SurveySubmissionDto submissionDto, int userId)
        {
            var action = await _unitOfWork.ConventionActions.Query
                .FirstOrDefaultAsync(a => a.ConventionId == survey.ConventionId &&
                                          a.BehaviorType == BehaviorType.ModuleLink &&
                                          a.TargetModuleId == survey.Id);

            if (action == null) return;

            // ActionSubmission 저장
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

            // UserActionStatus 완료 표시
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

        private async Task SyncUserOptionToursAsync(Survey survey, SurveySubmissionDto submissionDto, int userId)
        {
            if (!survey.ConventionId.HasValue) return;

            // 이 설문의 모든 옵션에 연결된 OptionTourId 목록
            var surveyOptionTourIds = await _unitOfWork.SurveyQuestions.Query
                .Where(q => q.SurveyId == survey.Id)
                .SelectMany(q => q.Options)
                .Where(o => o.OptionTourId != null)
                .Select(o => o.OptionTourId!.Value)
                .Distinct().ToListAsync();

            if (surveyOptionTourIds.Count == 0) return;

            // 기존 선택 삭제 (ExecuteDeleteAsync = 즉시 SQL 실행, 같은 트랜잭션 내에서 동작)
            await _unitOfWork.UserOptionTours.Query
                .Where(uot => uot.UserId == userId
                    && uot.ConventionId == survey.ConventionId.Value
                    && surveyOptionTourIds.Contains(uot.OptionTourId))
                .ExecuteDeleteAsync();

            // 선택된 옵션의 OptionTourId로 새 UserOptionTour 생성
            var allSelectedOptionIds = submissionDto.Answers
                .Where(a => a.SelectedOptionIds != null)
                .SelectMany(a => a.SelectedOptionIds).ToHashSet();

            var selectedTourIds = await _unitOfWork.QuestionOptions.Query
                .Where(o => allSelectedOptionIds.Contains(o.Id) && o.OptionTourId != null)
                .Select(o => o.OptionTourId!.Value)
                .Distinct().ToListAsync();

            foreach (var tourId in selectedTourIds)
            {
                await _unitOfWork.UserOptionTours.AddAsync(new UserOptionTour
                {
                    UserId = userId,
                    OptionTourId = tourId,
                    ConventionId = survey.ConventionId.Value
                });
            }

            _logger.LogInformation("Synced {Count} option tours for user {UserId} from survey {SurveyId}",
                selectedTourIds.Count, userId, survey.Id);
        }

        private async Task DeleteSurveyActionsAsync(int surveyId)
        {
            var actionIds = await _unitOfWork.ConventionActions.Query
                .Where(a => a.BehaviorType == BehaviorType.ModuleLink && a.TargetModuleId == surveyId)
                .Select(a => a.Id)
                .ToListAsync();

            if (actionIds.Count == 0) return;

            // 배치 삭제 (ExecuteDeleteAsync = 즉시 SQL 실행, tracked 변경 없음)
            // 호출부(DeleteSurveyAsync)에서 Remove(survey)와 함께 트랜잭션으로 묶임
            await _unitOfWork.UserActionStatuses.Query
                .Where(s => actionIds.Contains(s.ConventionActionId))
                .ExecuteDeleteAsync();

            await _unitOfWork.ActionSubmissions.Query
                .Where(s => actionIds.Contains(s.ConventionActionId))
                .ExecuteDeleteAsync();

            await _unitOfWork.ConventionActions.Query
                .Where(a => actionIds.Contains(a.Id))
                .ExecuteDeleteAsync();
        }

        #endregion

        #region Private Helpers — Question Update

        private void UpdateQuestions(Survey survey, List<QuestionCreateDto> questionDtos, Dictionary<int, QuestionOption> tempKeyToOption)
        {
            var updatedQuestionIds = questionDtos.Where(q => q.Id > 0).Select(q => q.Id).ToHashSet();

            // 삭제 대상 질문 제거 (꼬리질문의 ParentOption도 정리)
            foreach (var questionToRemove in survey.Questions.Where(q => !updatedQuestionIds.Contains(q.Id)).ToList())
            {
                _unitOfWork.SurveyQuestions.Remove(questionToRemove);
            }

            foreach (var qDto in questionDtos)
            {
                var question = survey.Questions.FirstOrDefault(q => q.Id == qDto.Id);
                if (question == null || qDto.Id == 0)
                {
                    question = new SurveyQuestion
                    {
                        SurveyId = survey.Id,
                        QuestionText = qDto.QuestionText,
                        Type = ParseQuestionType(qDto.Type),
                        IsRequired = qDto.IsRequired,
                        OrderIndex = qDto.OrderIndex
                    };
                    survey.Questions.Add(question);
                }
                else
                {
                    question.QuestionText = qDto.QuestionText;
                    question.Type = ParseQuestionType(qDto.Type);
                    question.IsRequired = qDto.IsRequired;
                    question.OrderIndex = qDto.OrderIndex;
                    // 기존 양수 ParentOptionId 직접 설정
                    question.ParentOptionId = qDto.ParentOptionId > 0 ? qDto.ParentOptionId : null;
                }

                UpdateOptions(question, qDto.Options, tempKeyToOption);
            }
        }

        private void UpdateOptions(SurveyQuestion question, List<OptionCreateDto> optionDtos, Dictionary<int, QuestionOption> tempKeyToOption)
        {
            var updatedOptionIds = optionDtos.Where(o => o.Id > 0).Select(o => o.Id).ToHashSet();

            foreach (var optionToRemove in question.Options.Where(o => !updatedOptionIds.Contains(o.Id)).ToList())
            {
                _unitOfWork.QuestionOptions.Remove(optionToRemove);
            }

            foreach (var oDto in optionDtos)
            {
                var option = question.Options.FirstOrDefault(o => o.Id == oDto.Id);
                if (option == null || oDto.Id == 0)
                {
                    option = new QuestionOption
                    {
                        QuestionId = question.Id,
                        OptionText = oDto.OptionText,
                        OrderIndex = oDto.OrderIndex,
                        IsTerminating = oDto.IsTerminating,
                        OptionTourId = oDto.OptionTourId
                    };
                    question.Options.Add(option);
                }
                else
                {
                    option.OptionText = oDto.OptionText;
                    option.OrderIndex = oDto.OrderIndex;
                    option.IsTerminating = oDto.IsTerminating;
                    option.OptionTourId = oDto.OptionTourId;
                }

                if (oDto.TempKey.HasValue && oDto.TempKey.Value < 0)
                {
                    tempKeyToOption[oDto.TempKey.Value] = option;
                }
            }
        }

        /// <summary>
        /// 꼬리질문의 ParentOptionId를 해소 (음수 임시키 → 실제 ID)
        /// </summary>
        private static void ResolveParentOptionIds(Survey survey, List<QuestionCreateDto> questionDtos, Dictionary<int, QuestionOption> tempKeyToOption)
        {
            var questionsByOrder = survey.Questions.OrderBy(q => q.OrderIndex).ToList();
            var dtosOrdered = questionDtos.OrderBy(q => q.OrderIndex).ToList();

            for (int i = 0; i < dtosOrdered.Count && i < questionsByOrder.Count; i++)
            {
                var dto = dtosOrdered[i];
                var question = questionsByOrder[i];

                if (!dto.ParentOptionId.HasValue)
                {
                    question.ParentOptionId = null;
                    continue;
                }

                if (dto.ParentOptionId.Value > 0)
                {
                    // 기존 옵션 ID
                    question.ParentOptionId = dto.ParentOptionId.Value;
                }
                else if (tempKeyToOption.TryGetValue(dto.ParentOptionId.Value, out var resolvedOption))
                {
                    // 임시키 → 실제 ID
                    question.ParentOptionId = resolvedOption.Id;
                }
            }
        }

        #endregion

        #region Private Helpers — Mapping & Stats

        private static SurveyDto MapToSurveyDto(Survey survey)
        {
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
                SurveyType = survey.SurveyType.ToString(),
                Questions = survey.Questions.OrderBy(q => q.OrderIndex).Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Type = q.Type.ToString(),
                    IsRequired = q.IsRequired,
                    OrderIndex = q.OrderIndex,
                    ParentOptionId = q.ParentOptionId,
                    Options = q.Options.OrderBy(o => o.OrderIndex).Select(o => new OptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        OrderIndex = o.OrderIndex,
                        IsTerminating = o.IsTerminating,
                        OptionTourId = o.OptionTourId,
                        OptionTourName = o.OptionTour?.Name
                    }).ToList()
                }).ToList()
            };
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
            else
            {
                var textAnswers = detailsForQuestion
                    .Where(d => !string.IsNullOrEmpty(d.AnswerText))
                    .Select(d => new AnswerStatDto { Answer = d.AnswerText, Count = 1 })
                    .ToList();
                questionStats.Answers.AddRange(textAnswers);
            }

            return questionStats;
        }

        #endregion
    }
}
