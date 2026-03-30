using Sentinel.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace Sentinel.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?
                .User.FindFirst("sub")?.Value;

            return userId != null ? Guid.Parse(userId) : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?
            .User.FindFirst("email")?.Value;

    public string? Username =>
        _httpContextAccessor.HttpContext?
            .User.FindFirst("username")?.Value;
}