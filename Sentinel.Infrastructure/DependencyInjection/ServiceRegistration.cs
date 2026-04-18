using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Sentinel.Application.Interfaces;
using Sentinel.Infrastructure.Messaging;
using Sentinel.Infrastructure.Security;
using Sentinel.Infrastructure.Services;
using StackExchange.Redis;

namespace Sentinel.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 🔐 SERVICES
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

        // 🔴 REDIS
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]!));

        services.AddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>();
        services.AddSingleton<ILoginAttemptService, RedisLoginAttemptService>();

        // 🐇 RABBITMQ
        services.AddSingleton<IConnection>(_ =>
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"]!,
                Port = int.Parse(configuration["RabbitMQ:Port"]!),
                UserName = configuration["RabbitMQ:Username"]!,
                Password = configuration["RabbitMQ:Password"]!
            };
            return factory.CreateConnection();
        });

        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddHostedService<UserRegisteredConsumer>();

        return services;
    }
}