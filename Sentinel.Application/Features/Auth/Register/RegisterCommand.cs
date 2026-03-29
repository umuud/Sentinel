using MediatR;

namespace Sentinel.Application.Features.Auth.Register;

public class RegisterCommand : IRequest<Guid>
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}