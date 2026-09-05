using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Application.Services;
using NomiWrite.Auth.Application.Validation;
using NomiWrite.Auth.Infrastructure.Options;
using NomiWrite.Auth.Infrastructure.Persistence;
using NomiWrite.Auth.Infrastructure.Security;

namespace NomiWrite.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AuthDb")
            ?? throw new InvalidOperationException("Connection string 'AuthDb' is not configured.");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<AuthDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IValidator<RegisterRequestDto>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", host =>
                {
                    host.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    host.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}