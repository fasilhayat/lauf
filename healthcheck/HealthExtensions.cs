namespace MyCompany.Yggdrasil.HealthCheck;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Provides extension methods for configuring and exposing health checks in an ASP.NET Core application.
/// </summary>
public static class HealthExtensions
{
    /// <summary>
    /// Adds health check services to the application's dependency injection container.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder to configure.</param>
    /// <returns>The WebApplicationBuilder with health services added.</returns>
    public static WebApplicationBuilder AddHealth(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealth();
        return builder;
    }

    /// <summary>
    /// Adds a self health check to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the health check to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddHealth(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        return services;
    }

    /// <summary>
    /// Adds a health check endpoint at '/healthz' with a custom response writer and allows anonymous access.
    /// </summary>
    /// <param name="app">The endpoint route builder to configure.</param>
    /// <returns>The modified endpoint route builder.</returns>
    public static IEndpointRouteBuilder UseHealth(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthWriter.WriteHealthUiResponse,
            ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
        })
        .WithMetadata(new AllowAnonymousAttribute());

        return app;
    }
}