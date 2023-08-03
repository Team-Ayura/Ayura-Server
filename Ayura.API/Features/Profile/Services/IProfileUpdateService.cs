using Ayura.API.Features.Profile.DTOs;

namespace Ayura.API.Features.Profile.Services;

public interface IProfileUpdateService
{
    Task<ProfileDetailsDTO> UpdateProfileDetails(string email, UpdateDetailsDTO updateDetails);
}