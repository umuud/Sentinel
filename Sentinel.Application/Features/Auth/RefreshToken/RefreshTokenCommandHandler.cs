using MediatR;
using Sentinel.Application.Features.Auth.Login;
using Sentinel.Application.Interfaces;

namespace Sentinel.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IAccountRepository accountRepository,
        IJwtService jwtService,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _accountRepository = accountRepository;
        _jwtService = jwtService;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _refreshTokenRepository
            .GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (existingToken is null)
            throw new Exception("Refresh token bulunamadı");

        if (existingToken.IsRevoked)
            throw new Exception("Token revoke edilmiş");

        if (existingToken.IsExpired())
            throw new Exception("Token süresi dolmuş");

        var account = await _accountRepository
            .GetByIdAsync(existingToken.AccountId, cancellationToken);

        if (account is null)
            throw new Exception("Kullanıcı bulunamadı");
        
        if (!account.IsActive)
            throw new Exception("Hesap devre dışı");

        // 🔥 eski token iptal
        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        // 🔥 yeni token üret
        var newRefreshTokenValue = _refreshTokenGenerator.Generate();

        var newRefreshToken = new Sentinel.Domain.Entities.RefreshToken(
            account.Id,
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(7));

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        // 🔥 yeni access token
        var newAccessToken = _jwtService.GenerateToken(
            account.Id,
            account.Email,
            account.Username);

        return new LoginResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue
        };
    }
}