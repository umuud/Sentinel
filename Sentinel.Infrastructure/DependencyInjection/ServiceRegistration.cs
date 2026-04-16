using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Interfaces;
using Sentinel.Infrastructure.Security;
using Sentinel.Infrastructure.Services;

namespace Sentinel.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // 🔐 SERVICES
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

        return services;
    }
}