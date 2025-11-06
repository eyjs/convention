using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using LocalRAG.DTOs.SurveyModels;
using LocalRAG.Interfaces;

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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSurveys()
        {
            var surveys = await _surveyService.GetAllSurveysAsync();
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
            return Ok(survey);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSurvey(int id, [FromBody] SurveyCreateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedSurvey = await _surveyService.UpdateSurveyAsync(id, updateDto);
                return Ok(updatedSurvey);
            }
            catch (global::System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (global::System.Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred.", details = ex.Message });
            }
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitSurvey(int id, [FromBody] SurveySubmissionDto submissionDto)
        {
            if (id != submissionDto.SurveyId)
            {
                return BadRequest("Survey ID mismatch.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                await _surveyService.SubmitSurveyAsync(id, submissionDto, userId);
                return Ok(new { message = "Survey submitted successfully." });
            }
            catch (global::System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (global::System.Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                return StatusCode(500, new { message = "An internal error occurred.", details = ex.Message });
            }
        }

        [HttpGet("{id}/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSurveyStats(int id)
        {
            var stats = await _surveyService.GetSurveyStatsAsync(id);
            if (stats == null)
            {
                return NotFound();
            }
            return Ok(stats);
        }
    }
}