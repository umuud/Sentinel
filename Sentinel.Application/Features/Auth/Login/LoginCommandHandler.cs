using MediatR;
using Sentinel.Application.Interfaces;

namespace Sentinel.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public LoginCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (account is null)
            throw new Exception("Email veya şifre hatalı");

        var isValid = _passwordHasher.Verify(request.Password, account.PasswordHash);

        if (!isValid)
            throw new Exception("Email veya şifre hatalı");

        if (!account.IsActive)
            throw new Exception("Hesap devre dışı");

        var accessToken = _jwtService.GenerateToken(
            account.Id,
            account.Email,
            account.Username);

        var refreshTokenValue = _refreshTokenGenerator.Generate();

        var refreshToken = new Sentinel.Domain.Entities.RefreshToken(
            account.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7));

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue
        };
    }
}