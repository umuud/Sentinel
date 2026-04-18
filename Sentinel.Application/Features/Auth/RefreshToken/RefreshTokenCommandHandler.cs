using MediatR;
using Microsoft.Extensions.Logging;
using Sentinel.Application.Exceptions;
using Sentinel.Application.Features.Auth.Login;
using Sentinel.Application.Interfaces;

namespace Sentinel.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IAccountRepository accountRepository,
        IJwtService jwtService,
        IRefreshTokenGenerator refreshTokenGenerator,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _accountRepository = accountRepository;
        _jwtService = jwtService;
        _refreshTokenGenerator = refreshTokenGenerator;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _refreshTokenRepository
            .GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (existingToken is null)
        {
            _logger.LogWarning("Token yenileme başarısız — token bulunamadı");
            throw new UnauthorizedException("Refresh token bulunamadı");
        }

        if (existingToken.IsRevoked)
        {
            _logger.LogWarning("Token yenileme başarısız — token revoke edilmiş: {TokenId}", existingToken.Id);
            throw new UnauthorizedException("Token revoke edilmiş");
        }

        if (existingToken.IsExpired())
        {
            _logger.LogWarning("Token yenileme başarısız — token süresi dolmuş: {TokenId}", existingToken.Id);
            throw new UnauthorizedException("Token süresi dolmuş");
        }

        var account = await _accountRepository
            .GetByIdAsync(existingToken.AccountId, cancellationToken);

        if (account is null)
        {
            _logger.LogWarning("Token yenileme başarısız — kullanıcı bulunamadı: {AccountId}", existingToken.AccountId);
            throw new UnauthorizedException("Kullanıcı bulunamadı");
        }

        if (!account.IsActive)
        {
            _logger.LogWarning("Token yenileme başarısız — hesap devre dışı: {AccountId}", account.Id);
            throw new UnauthorizedException("Hesap devre dışı");
        }

        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        var newRefreshTokenValue = _refreshTokenGenerator.Generate();

        var newRefreshToken = new Sentinel.Domain.Entities.RefreshToken(
            account.Id,
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(7));

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        var newAccessToken = _jwtService.GenerateToken(
            account.Id,
            account.Email,
            account.Username);

        _logger.LogInformation("Token yenileme başarılı — AccountId: {AccountId}", account.Id);

        return new LoginResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue
        };
    }
}