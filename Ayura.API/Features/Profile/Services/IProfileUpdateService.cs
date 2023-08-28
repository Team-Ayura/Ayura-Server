using Ayura.API.Features.Profile.DTOs;

namespace Ayura.API.Features.Profile.Services;

public interface IProfileUpdateService
{
    Task<ProfileDetailsDto> UpdateProfileDetails(string email, UpdateDetailsDto updateDetails);
}