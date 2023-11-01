using Ayura.API.Global.Helpers;

public class AuthMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var excludedPaths = new List<string>
        {
            "/api/profile/testnoauth",
            "/api/profile/testwithauth",
            "/api/auth/signin",
            "/api/auth/signup",
            "/api/gpt",
        };

        // Check if the request path is in the excluded list
        if (excludedPaths.Contains(context.Request.Path.Value))
        {
            await next(context);
            return;
        }

        // Check if the Authorization header is present
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authorization header is missing - By Middleware");
            return;
        }

        var userId = ResolveJwt.ResolveIdFromJwt(context.Request);
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 401; // Unauthorized
            return;
        }

        // If the JWT is valid, you can optionally set user information in the HttpContext for further processing.
        context.Items["UserId"] = userId;

        await next(context);
    }
}