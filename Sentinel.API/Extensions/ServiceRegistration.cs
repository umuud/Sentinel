using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Sentinel.API.Services;
using Sentinel.Application.Interfaces;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using System.Text;
using System.Threading.RateLimiting;

namespace Sentinel.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostBuilder hostBuilder)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // SERILOG
        hostBuilder.UseSerilog((context, config) =>
        {
            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(new ElasticsearchJsonFormatter())
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                    new Uri(context.Configuration["Serilog:ElasticsearchUrl"]!))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = $"sentinel-logs-{DateTime.UtcNow:yyyy.MM.dd}",
                    NumberOfReplicas = 0,
                    NumberOfShards = 1
                });
        });

        // JWT
        var key = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = issuer,
                ValidAudience = audience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key!)),

                NameClaimType = "username",
            };
        });

        // Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter("login", limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.SegmentsPerWindow = 2;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.RejectionStatusCode = 429;
        });

        return services;
    }
}