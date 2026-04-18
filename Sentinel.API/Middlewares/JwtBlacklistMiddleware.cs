using Sentinel.Application.Interfaces;

namespace Sentinel.API.Middlewares;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtBlacklistMiddleware> _logger;

    public JwtBlacklistMiddleware(RequestDelegate next, ILogger<JwtBlacklistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
                _logger.LogWarning("Blacklist'e takılan token tespit edildi — Jti: {Jti}, Path: {Path}",
                    jti, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Token geçersiz kılınmış" });
                return;
            }
        }

        await _next(context);
    }
}