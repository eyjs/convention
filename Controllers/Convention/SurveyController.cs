using System.Security.Claims;
using System.Threading.Tasks;
using LocalRAG.Interfaces;
using LocalRAG.Models.DTOs;
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
            var guestIdClaim = User.FindFirst("GuestId");
            if (guestIdClaim == null || !int.TryParse(guestIdClaim.Value, out int guestId))
            {
                return Unauthorized("게스트 정보를 확인할 수 없습니다.");
            }

            try
            {
                var responseId = await _surveyService.SaveSurveyResponse(actionType, guestId, responseDto);
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
            var guestIdClaim = User.FindFirst("GuestId");
            if (guestIdClaim == null || !int.TryParse(guestIdClaim.Value, out int guestId))
            {
                return Unauthorized("게스트 정보를 확인할 수 없습니다.");
            }

            try
            {
                var response = await _surveyService.GetSurveyResponse(actionType, guestId);
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
