using System.IdentityModel.Tokens.Jwt;
using Ayura.API.Features.Profile.DTOs;
using Ayura.API.Features.Profile.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.Profile.Controllers;

[Route("api/profile")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IProfileRetrieveService _profileRetrieveService;

    public ProfileController(IProfileRetrieveService profileRetrieveService)
    {
        _profileRetrieveService = profileRetrieveService;
    }

    public string ResolveEmailFromJWT()
    {
        string jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwtToken);
        string email = token.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

        return email;
    }

    [HttpGet("details")]
    public async Task<IActionResult> GetProfileDetails()
    {
        try
        {
            string email = ResolveEmailFromJWT();

            var profileDetails = await _profileRetrieveService.RetrieveProfileDetails(email);

            if (profileDetails == null)
            {
                return Ok("No User Details");
            }

            return Ok(profileDetails);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while processing the request.");
        }
    }
}