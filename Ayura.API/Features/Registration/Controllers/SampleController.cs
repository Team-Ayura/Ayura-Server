using Ayura.API.Models.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Controllers.Registration;

[ApiController]
[Route("api/[controller]")]
public class SampleController : Controller
{
    // GET
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Index()
    {
        return Ok(new { Status = "success"});
    }
}