using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.DTOs.SurveyModels;
using LocalRAG.Interfaces;
using LocalRAG.Constants;
using LocalRAG.Extensions;

namespace LocalRAG.Controllers.Convention
{
    [ApiController]
    [Route("api/surveys")]
    [Authorize]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet("admin/{conventionId}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllSurveys(int conventionId, [FromQuery] string? type = null)
        {
            var surveys = await _surveyService.GetAllSurveysAsync(conventionId, type);
            return Ok(surveys);
        }

        [HttpGet("convention/{conventionId}")]
        [Authorize]
        public async Task<IActionResult> GetSurveysForConvention(int conventionId)
        {
            var userId = User.GetUserId();
            var surveys = await _surveyService.GetSurveysForUserAsync(conventionId, userId);
            return Ok(surveys);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSurvey(int id)
        {
            var survey = await _surveyService.GetSurveyAsync(id);
            if (survey == null)
            {
                return NotFound();
            }

            // 비관리자: 날짜 범위 검증
            if (!User.IsInRole(Roles.Admin))
            {
                var now = DateTime.UtcNow;
                if (survey.StartDate.HasValue && now < survey.StartDate.Value)
                {
                    return StatusCode(403, new { message = "설문 응답 기간이 아직 시작되지 않았습니다.", startDate = survey.StartDate });
                }
                if (survey.EndDate.HasValue && now > survey.EndDate.Value)
                {
                    return StatusCode(403, new { message = "설문 응답 기간이 종료되었습니다.", endDate = survey.EndDate });
                }
            }

            return Ok(survey);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> CreateSurvey([FromBody] SurveyCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdSurvey = await _surveyService.CreateSurveyAsync(createDto);
            return CreatedAtAction(nameof(GetSurvey), new { id = createdSurvey.Id }, createdSurvey);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateSurvey(int id, [FromBody] SurveyCreateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedSurvey = await _surveyService.UpdateSurveyAsync(id, updateDto);
            return Ok(updatedSurvey);
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitSurvey(int id, [FromBody] SurveySubmissionDto submissionDto)
        {
            if (id != submissionDto.SurveyId)
            {
                return BadRequest("Survey ID mismatch.");
            }

            var userId = User.GetUserId();
            await _surveyService.SubmitSurveyAsync(id, submissionDto, userId);
            return Ok(new { message = "설문이 제출되었습니다." });
        }

        [HttpGet("{surveyId}/responses/me")]
        [Authorize]
        public async Task<ActionResult<SurveyResponseDto>> GetUserSurveyResponse(int surveyId)
        {
            var userId = User.GetUserId();
            var response = await _surveyService.GetUserSurveyResponseAsync(surveyId, userId);

            if (response == null)
            {
                return NotFound("User response not found for this survey.");
            }

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteSurvey(int id)
        {
            var (success, message) = await _surveyService.DeleteSurveyAsync(id);
            if (!success)
            {
                return NotFound(new { message });
            }
            return Ok(new { message });
        }

        [HttpGet("{id}/responses")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetSurveyResponses(int id)
        {
            var responses = await _surveyService.GetSurveyResponsesAsync(id);
            return Ok(responses);
        }

        [HttpGet("{id}/responses/export")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ExportSurveyResponses(int id)
        {
            var bytes = await _surveyService.ExportSurveyResponsesAsync(id);
            if (bytes.Length == 0)
            {
                return NotFound(new { message = "설문을 찾을 수 없습니다." });
            }
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"survey_{id}_responses.xlsx");
        }

        [HttpGet("{id}/stats")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetSurveyStats(int id)
        {
            var stats = await _surveyService.GetSurveyStatsAsync(id);
            if (stats == null)
            {
                return NotFound();
            }
            return Ok(stats);
        }

        [HttpGet("{id}/status")]
        [Authorize]
        public async Task<IActionResult> GetSurveyStatus(int id)
        {
            var userId = User.GetUserId();
            var response = await _surveyService.GetUserSurveyResponseAsync(id, userId);
            string status = response != null ? "Completed" : "NotStarted";
            return Ok(new { status });
        }

        [HttpGet("{id}/summary")]
        [Authorize]
        public async Task<IActionResult> GetSurveySummary(int id)
        {
            var userId = User.GetUserId();

            var survey = await _surveyService.GetSurveyAsync(id);
            if (survey == null)
            {
                return NotFound("Survey not found.");
            }

            var response = await _surveyService.GetUserSurveyResponseAsync(id, userId);

            string summary;
            if (response != null)
            {
                var totalQuestions = survey.Questions?.Count ?? 0;
                var answeredQuestions = response.Answers?.Count ?? 0;
                summary = $"{answeredQuestions}/{totalQuestions} 항목 응답 완료";
            }
            else
            {
                summary = "미응답";
            }

            return Ok(new { summary });
        }
    }
}
