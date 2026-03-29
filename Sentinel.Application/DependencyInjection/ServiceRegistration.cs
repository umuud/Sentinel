using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Behaviors;
using System.Reflection;

namespace Sentinel.Application.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}