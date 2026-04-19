using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sentinel.Application.Interfaces;
using Sentinel.Infrastructure.Messaging;
using Sentinel.Infrastructure.Options;
using Sentinel.Infrastructure.Security;
using Sentinel.Infrastructure.Services;
using StackExchange.Redis;

namespace Sentinel.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ⚙️ OPTIONS
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        // 🔐 SERVICES
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

        // 🔴 REDIS
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        });

        services.AddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>();
        services.AddSingleton<ILoginAttemptService, RedisLoginAttemptService>();

        // 🐇 RABBITMQ
        services.AddSingleton<IConnection>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password
            };
            return factory.CreateConnection();
        });

        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddHostedService<UserRegisteredConsumer>();

        return services;
    }
}