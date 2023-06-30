namespace Ayura.API.Features.Registration.DTOs;

public class SigninRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}