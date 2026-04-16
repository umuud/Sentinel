using MediatR;
using Sentinel.Application.Features.Auth.Login;

namespace Sentinel.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommand : IRequest<LoginResult>
{
    public string RefreshToken { get; set; } = default!;
}