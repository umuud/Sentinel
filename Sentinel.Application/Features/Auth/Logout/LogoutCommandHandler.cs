using MediatR;
using Sentinel.Application.Interfaces;

namespace Sentinel.Application.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly ITokenBlacklistService _blacklistService;
    private readonly IJwtService _jwtService;

    public LogoutCommandHandler(
        ITokenBlacklistService blacklistService,
        IJwtService jwtService)
    {
        _blacklistService = blacklistService;
        _jwtService = jwtService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var jti = _jwtService.GetJtiFromToken(request.AccessToken);

        if (string.IsNullOrWhiteSpace(jti))
            throw new Exception("Geçersiz token");

        var expiry = _jwtService.GetExpiryFromToken(request.AccessToken);
        var remaining = (expiry ?? DateTime.UtcNow) - DateTime.UtcNow;

        if (remaining > TimeSpan.Zero)
            await _blacklistService.AddToBlacklistAsync(jti, remaining);
    }
}