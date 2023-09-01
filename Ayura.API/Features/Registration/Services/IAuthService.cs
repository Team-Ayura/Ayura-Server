using Ayura.API.Features.Registration.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.DTOs;

namespace Ayura.API.Services;

public interface IAuthService
{
    Task<LoginResponse> AuthenticateUser(string email, string password);

    Task<User> RegisterUser(SignupRequest signupRequest);
}