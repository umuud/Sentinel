using MediatR;
using Microsoft.Extensions.Logging;
using Sentinel.Application.Exceptions;
using Sentinel.Application.Interfaces;

namespace Sentinel.Application.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly ITokenBlacklistService _blacklistService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        ITokenBlacklistService blacklistService,
        IJwtService jwtService,
        ILogger<LogoutCommandHandler> logger)
    {
        _blacklistService = blacklistService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var jti = _jwtService.GetJtiFromToken(request.AccessToken);

        if (string.IsNullOrWhiteSpace(jti))
        {
            _logger.LogWarning("Logout başarısız — geçersiz token");
            throw new UnauthorizedException("Geçersiz token");
        }

        var expiry = _jwtService.GetExpiryFromToken(request.AccessToken);
        var remaining = (expiry ?? DateTime.UtcNow) - DateTime.UtcNow;

        if (remaining > TimeSpan.Zero)
            await _blacklistService.AddToBlacklistAsync(jti, remaining);

        _logger.LogInformation("Logout başarılı — Jti: {Jti}", jti);
    }
}