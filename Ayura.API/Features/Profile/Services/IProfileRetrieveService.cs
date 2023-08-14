using Ayura.API.Features.Profile.DTOs;

namespace Ayura.API.Features.Profile.Services;

public interface IProfileRetrieveService
{
    Task<ProfileDetailsDTO> RetrieveProfileDetails(string email);
}