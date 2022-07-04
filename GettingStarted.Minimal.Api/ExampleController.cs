using Microsoft.AspNetCore.Mvc;

namespace GettingStarted.Minimal.Api;

[ApiController]
[Route("[controller]")]
public class ExampleController : ControllerBase
{
    [HttpGet("testfromcontroller")]
    public IActionResult TestMethod()
    {
        return Ok("Hello from controller");
    }
}
