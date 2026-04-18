using MediatR;
using Sentinel.Application.Events;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventBus _eventBus;

    public RegisterCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IEventBus eventBus)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _accountRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new Exception("Bu email zaten kayıtlı");

        if (await _accountRepository.UsernameExistsAsync(request.Username, cancellationToken))
            throw new Exception("Bu username zaten kayıtlı");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var account = new Account(
            request.Username,
            request.Email,
            passwordHash);

        await _accountRepository.AddAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new UserRegisteredEvent(
            account.Id,
            account.Username,
            account.Email,
            account.CreatedDate), cancellationToken);

        return account.Id;
    }
}