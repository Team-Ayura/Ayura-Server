using System.IdentityModel.Tokens.Jwt;

namespace Ayura.API.Features.Profile.Helpers;

public class ResolveJWT
{
    public static string ResolveEmailFromJWT(HttpRequest request)
    {
        var jwtToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwtToken);
        var id = token.Claims.FirstOrDefault(claim => claim.Type == "nameid")?.Value;

        return id;
    }
}