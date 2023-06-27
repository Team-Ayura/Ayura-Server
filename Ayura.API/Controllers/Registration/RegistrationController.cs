using Ayura.API.Models;
using Ayura.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Controllers.Registration;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPasswordHasher<string> _passwordHasher;

    public RegistrationController(IUserService userService, IPasswordHasher<string> passwordHasher)
    {
        _userService = userService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    public ActionResult Register([FromBody] User user)
    {
        // Hash the password
        string hashedPassword = _passwordHasher.HashPassword(null, user.Password);
        user.Password = hashedPassword;

        User newUser = _userService.Create(user);
        if (newUser != null)
        {
            return Ok(newUser);
        }
        return NotFound();
    }

    
}