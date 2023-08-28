using Ayura.API.Features.Profile.DTOs;
using Ayura.API.Features.Profile.Helpers;
using Ayura.API.Features.Profile.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.Profile.Controllers;

[Route("api/profile")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IProfileRetrieveService _profileRetrieveService;
    private readonly IProfileUpdateService _profileUpdateService;

    public ProfileController(IProfileRetrieveService profileRetrieveService, IProfileUpdateService profileUpdateService)
    {
        _profileRetrieveService = profileRetrieveService;
        _profileUpdateService = profileUpdateService;
    }

    [HttpGet("details")]
    public async Task<IActionResult> GetProfileDetails()
    {
        var _userId = ResolveJwt.ResolveIdFromJWT(Request);

        try
        {
            var profileDetails = await _profileRetrieveService.RetrieveProfileDetails(_userId);

            if (profileDetails == null) return Ok("No User Details");

            return Ok(profileDetails);
        }
        catch (Exception ex)
        {
            var response = new
            {
                ErrorMessage = "An error occurred while processing the request",
                ExceptionMessage = ex.Message,
            };

            return StatusCode(500, response);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfileDetails([FromBody] UpdateDetailsDto updateDetailsDto)
    {
        var _userId = ResolveJwt.ResolveIdFromJWT(Request);

        try
        {
            Console.Write($"ID is {_userId}\n");
            var updatedProfileDetails = await _profileUpdateService.UpdateProfileDetails(_userId, updateDetailsDto);

            Console.Write("Function Done!!");

            return Ok(updatedProfileDetails);
        }
        catch (Exception ex)
        {
            var response = new
            {
                ErrorMessage = "An error occurred while processing the request",
                ExceptionMessage = ex.Message,
            };
            return StatusCode(500, response);
        }
    }
}