using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Infrastructure.Persistence;
using NomiWrite.Auth.Infrastructure.Repositories;
using NomiWrite.Auth.Infrastructure.Services;
using MassTransit;

namespace NomiWrite.Auth.Infrastructure;

/// <summary>
/// Auth service DI registration — PostgreSQL, MassTransit/RabbitMQ, services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── PostgreSQL (Auth's own database) ──
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("AuthDb"),
                o => o.EnableRetryOnFailure(maxRetryCount: 3)));

        // ── MassTransit + RabbitMQ ──
        services.AddMassTransit(x =>
        {
            // Register consumers here when Auth needs to consume events
            // x.AddConsumer<SomeEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        // ── Services & Repositories ──
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
