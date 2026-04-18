using MediatR;
using Microsoft.Extensions.Logging;
using Sentinel.Application.Exceptions;
using Sentinel.Application.Interfaces;

namespace Sentinel.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator,
        ILoginAttemptService loginAttemptService,
        ILogger<LoginCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _loginAttemptService = loginAttemptService;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (await _loginAttemptService.IsBlockedAsync(request.Email))
        {
            _logger.LogWarning("Giriş engellendi — hesap bloke: {Email}", request.Email);
            throw new UnauthorizedException("Çok fazla başarısız deneme. 15 dakika sonra tekrar deneyin.");
        }

        var account = await _accountRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (account is null)
        {
            await _loginAttemptService.RecordFailedAttemptAsync(request.Email);
            _logger.LogWarning("Giriş başarısız — hesap bulunamadı: {Email}", request.Email);
            throw new UnauthorizedException("Email veya şifre hatalı");
        }

        var isValid = _passwordHasher.Verify(request.Password, account.PasswordHash);

        if (!isValid)
        {
            await _loginAttemptService.RecordFailedAttemptAsync(request.Email);
            _logger.LogWarning("Giriş başarısız — hatalı şifre: {Email}", request.Email);
            throw new UnauthorizedException("Email veya şifre hatalı");
        }

        if (!account.IsActive)
        {
            _logger.LogWarning("Giriş başarısız — hesap devre dışı: {Email}", request.Email);
            throw new UnauthorizedException("Hesap devre dışı");
        }

        await _loginAttemptService.ResetAttemptsAsync(request.Email);

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

        _logger.LogInformation("Giriş başarılı — AccountId: {AccountId}, Email: {Email}", account.Id, account.Email);

        return new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue
        };
    }
}