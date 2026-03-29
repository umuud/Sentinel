using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Interfaces;
using Sentinel.Infrastructure.Services;

namespace Sentinel.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}