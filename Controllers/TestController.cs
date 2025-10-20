using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Test controller works!" });
    }
}
