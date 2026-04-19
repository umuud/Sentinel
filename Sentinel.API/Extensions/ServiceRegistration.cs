using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sentinel.API.Options;
using Sentinel.API.Services;
using Sentinel.Application.Interfaces;
using Sentinel.Infrastructure.Options;
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

        // ⚙️ OPTIONS
        services.Configure<SerilogOptions>(configuration.GetSection(SerilogOptions.SectionName));

        // 📋 SERILOG
        hostBuilder.UseSerilog((context, sp, config) =>
        {
            var serilogOptions = sp.GetRequiredService<IOptions<SerilogOptions>>().Value;

            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(new ElasticsearchJsonFormatter())
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                    new Uri(serilogOptions.ElasticsearchUrl))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = $"sentinel-logs-{DateTime.UtcNow:yyyy.MM.dd}",
                    NumberOfReplicas = serilogOptions.NumberOfReplicas,
                    NumberOfShards = serilogOptions.NumberOfShards
                });
        });

        // 🔐 JWT
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

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

                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Key)),

                NameClaimType = "username",
            };
        });

        // 🚦 RATE LIMITING
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