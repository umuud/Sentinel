using MediatR;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Email kontrol
        if (await _accountRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new Exception("Bu email zaten kayıtlı");
        //Username kontrol
        if (await _accountRepository.UsernameExistsAsync(request.Username, cancellationToken))
            throw new Exception("Bu username zaten kayıtlı");


        // Hash
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Domain
        var account = new Account(
            request.Username,
            request.Email,
            passwordHash
        );

        // Save
        await _accountRepository.AddAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        return account.Id;
    }
}