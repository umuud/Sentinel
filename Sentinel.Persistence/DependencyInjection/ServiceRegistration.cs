using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Interfaces;
using Sentinel.Persistence.Context;
using Sentinel.Persistence.Repositories;

namespace Sentinel.Persistence.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSqlConnection");

        services.AddDbContext<SentinelDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAccountRepository, AccountRepository>();

        return services;
    }
}