using Sentinel.Application.Interfaces;

namespace Sentinel.API.Middlewares;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public JwtBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService blacklistService, IJwtService jwtService)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader is not null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..].Trim();
            var jti = jwtService.GetJtiFromToken(token);

            if (jti is not null && await blacklistService.IsBlacklistedAsync(jti))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Token geçersiz kılınmış" });
                return;
            }
        }

        await _next(context);
    }
}