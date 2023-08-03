using Ayura.API.Features.Profile.DTOs;
using Ayura.API.Models;

namespace Ayura.API.Features.Profile.Services;

public interface IProfileRetrieveService
{
    Task<ProfileDetailsDTO> RetrieveProfileDetails(string email);
}
