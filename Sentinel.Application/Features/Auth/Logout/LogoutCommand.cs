using MediatR;

namespace Sentinel.Application.Features.Auth.Logout;

public record LogoutCommand(string AccessToken) : IRequest;