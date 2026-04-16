using MediatR;

namespace Sentinel.Application.Features.Auth.Login;

public class LoginCommand : IRequest<LoginResult>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}