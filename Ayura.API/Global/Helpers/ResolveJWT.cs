using System.IdentityModel.Tokens.Jwt;

namespace Ayura.API.Global.Helpers;

public class ResolveJwt
{
    public static string? ResolveIdFromJwt(HttpRequest request)
    {
        var jwtToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwtToken);
        var id = token.Claims.FirstOrDefault(claim => claim.Type == "nameid")?.Value;

        return id;
    }
}