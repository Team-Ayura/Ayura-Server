using Ayura.API.Features.OTP.DTOs;
using Ayura.API.Features.OTP.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.OTP.Controllers;

[Route("api/otp")]
public class OtpController : Controller
{
    private readonly IOtpService _otpService;
    
    public OtpController(IOtpService otpService)
    {
        _otpService = otpService;
    }
    
    // POST
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] OtpRequestDTO otpRequestDTO)
    {
        
    }
    
    // POST
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] OtpVerifierDTO otpVerifierDTO)
    {
        
    }

}

