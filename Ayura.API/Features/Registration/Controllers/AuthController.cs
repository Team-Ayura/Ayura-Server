using Ayura.API.Features.Registration.DTOs;
using Ayura.API.Models.DTOs;
using Ayura.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Controllers.Registration;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordHasher<string> _passwordHasher;

    public AuthController(IAuthService authService,
        IPasswordHasher<string> passwordHasher) // inject necessary services through interfaces
    {
        _authService = authService;
        _passwordHasher = passwordHasher;
    }

    // signin endpoint
    [HttpPost("signin")]
    public async Task<IActionResult> Signin([FromBody] SigninRequest signinRequest)
    {
        try
        {
            var userInfo = await _authService.AuthenticateUser(signinRequest.Email, signinRequest.Password);
            return Ok(userInfo); // Return the token in the response
        }
        catch (Exception ex)
        {
            var unauthorizedResponse = new { Reason = ex.Message };
            return Unauthorized(unauthorizedResponse);
        }
    }

    // signup endpoint
    [HttpPost("signup")]
    public async Task<ActionResult> Signup([FromBody] SignupRequest signupRequest)
    {
        try
        {
            var user = await _authService.RegisterUser(signupRequest);
            return Ok(user);
        }
        catch (Exception ex)
        {
            var unauthorizedResponse = new { Reason = ex.Message };
            return Unauthorized(unauthorizedResponse);
        }
    }
}