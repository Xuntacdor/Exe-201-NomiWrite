using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Logging.Application.DTOs;
using NomiWrite.Logging.Application.Interfaces;
using NomiWrite.Logging.Application.Services;
using NomiWrite.Logging.Application.Validation;
using NomiWrite.Logging.Infrastructure.Consumers;
using NomiWrite.Logging.Infrastructure.Options;
using NomiWrite.Logging.Infrastructure.Persistence;
using NomiWrite.Logging.Infrastructure.Services;

namespace NomiWrite.Logging.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLoggingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb")
            ?? throw new InvalidOperationException("Connection string 'MongoDb' is not configured.");

        var mongoDbSettings = configuration
            .GetSection(MongoDbSettings.SectionName)
            .Get<MongoDbSettings>()
            ?? new MongoDbSettings();

        mongoDbSettings.ConnectionString = connectionString;

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<MongoDbContext>(sp =>
            new MongoDbContext(mongoDbSettings, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MongoDbContext>>()));

        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IValidator<ActivityLogQueryDto>, ActivityLogQueryValidator>();

        services.AddHostedService<MongoIndexInitializer>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserActivityLoggedConsumer>();

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