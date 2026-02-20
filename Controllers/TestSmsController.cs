#if DEBUG
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.Constants;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/test-sms")]
[Authorize(Roles = Roles.Admin)]
public class TestSmsController : ControllerBase
{
    private readonly ISmsService _smsService;

    public TestSmsController(ISmsService smsService)
    {
        _smsService = smsService;
    }

    [HttpGet("send")]
    public async Task<IActionResult> SendTest([FromQuery] string phone, [FromQuery] string name = "테스트")
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(new { message = "전화번호를 입력해주세요." });

        string message = $"[StarTour] {name}님, 시스템 구축 완료 테스트 문자입니다.";
        bool result = await _smsService.SendSmsAsync(null, name, phone, message);

        return Ok(new { success = result, phone, name, message });
    }
}
#endif
