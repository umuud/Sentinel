using MediatR;
using Microsoft.Extensions.Logging;
using Sentinel.Application.Events;
using Sentinel.Application.Exceptions;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventBus _eventBus;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IEventBus eventBus,
        ILogger<RegisterCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _accountRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("Kayıt başarısız — email zaten kayıtlı: {Email}", request.Email);
            throw new BusinessException("Bu email zaten kayıtlı");
        }

        if (await _accountRepository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            _logger.LogWarning("Kayıt başarısız — username zaten kayıtlı: {Username}", request.Username);
            throw new BusinessException("Bu username zaten kayıtlı");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var account = new Account(
            request.Username,
            request.Email,
            passwordHash);

        await _accountRepository.AddAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Kayıt başarılı — AccountId: {AccountId}, Email: {Email}", account.Id, account.Email);

        await _eventBus.PublishAsync(new UserRegisteredEvent(
            account.Id,
            account.Username,
            account.Email,
            account.CreatedDate), cancellationToken);

        _logger.LogInformation("UserRegisteredEvent publish edildi — AccountId: {AccountId}", account.Id);

        return account.Id;
    }
}