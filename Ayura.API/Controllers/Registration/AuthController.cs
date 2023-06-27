using Ayura.API.Models.DTOs;
using Ayura.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Controllers.Registration;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var token = await _authService.AuthenticateUser(loginRequest.Email, loginRequest.Password);

        if (token == null)
            return Unauthorized(); // Invalid credentials

        // Return the token in the response
        return Ok(new { Token = token });
    }
}
