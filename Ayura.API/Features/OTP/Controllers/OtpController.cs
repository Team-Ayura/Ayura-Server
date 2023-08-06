using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.OTP.Controllers;

public class OtpController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}