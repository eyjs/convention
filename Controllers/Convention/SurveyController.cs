using System.Security.Claims;
using System.Threading.Tasks;
using LocalRAG.DTOs.SurveyModels;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("{actionType}")]
        public async Task<IActionResult> SubmitSurvey(string actionType, [FromBody] SurveyResponseDto responseDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("사용자 정보를 확인할 수 없습니다.");
            }

            try
            {
                var responseId = await _surveyService.SaveSurveyResponse(actionType, userId, responseDto);
                return Ok(new { message = "Survey submitted successfully.", surveyResponseId = responseId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{actionType}")]
        public async Task<IActionResult> GetSurveyResponse(string actionType)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("사용자 정보를 확인할 수 없습니다.");
            }

            try
            {
                var response = await _surveyService.GetSurveyResponse(actionType, userId);
                if (response == null)
                {
                    return NotFound("Response not found.");
                }
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
