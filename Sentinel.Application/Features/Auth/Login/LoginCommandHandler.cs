using MediatR;
using Sentinel.Application.Interfaces;

namespace Sentinel.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Account bul
        var account = await _accountRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (account is null)
            throw new Exception("Email veya şifre hatalı");

        // 2. Password verify
        var isValid = _passwordHasher.Verify(request.Password, account.PasswordHash);

        if (!isValid)
            throw new Exception("Email veya şifre hatalı");

        // 3. Token üret
        var token = _jwtService.GenerateToken(
            account.Id,
            account.Email,
            account.Username
        );

        return token;
    }
}