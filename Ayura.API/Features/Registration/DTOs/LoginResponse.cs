namespace Ayura.API.Features.Registration.DTOs;

public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}