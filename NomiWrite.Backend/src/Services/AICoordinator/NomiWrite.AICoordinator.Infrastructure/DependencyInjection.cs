using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Application.Services;
using NomiWrite.AICoordinator.Infrastructure.Consumers;
using NomiWrite.AICoordinator.Infrastructure.Options;
using NomiWrite.AICoordinator.Infrastructure.Persistence;
using NomiWrite.AICoordinator.Infrastructure.Services;

namespace NomiWrite.AICoordinator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAICoordinatorInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SupabaseDb")
            ?? configuration["SUPABASE_DB_CONNECTION_STRING"]
            ?? configuration.GetConnectionString("GradingDb")
            ?? throw new InvalidOperationException("Connection string 'GradingDb' is not configured.");

        services.AddDbContext<GradingDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IGradingDbContext>(sp => sp.GetRequiredService<GradingDbContext>());

        services.Configure<GeminiSettings>(configuration.GetSection(GeminiSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHttpClient<IAiGradingProvider, GeminiGradingProvider>();

        services.AddScoped<IGradingService, GradingService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<WritingSubmittedEventConsumer>();

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
