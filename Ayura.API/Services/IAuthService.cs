namespace Ayura.API.Services;

public interface IAuthService
{
    Task<string> AuthenticateUser(string email, string password);
}