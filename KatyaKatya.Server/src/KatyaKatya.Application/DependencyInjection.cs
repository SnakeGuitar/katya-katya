using FluentValidation;
using MediatR;
using KatyaKatya.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace KatyaKatya.Application;

/// <summary>
/// Registers all Application-layer services: MediatR handlers, FluentValidation
/// validators, and pipeline behaviors.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application-layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
