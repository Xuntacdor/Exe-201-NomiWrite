using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Interfaces;
using NomiWrite.Subscription.Application.Services;
using NomiWrite.Subscription.Application.Validation;
using NomiWrite.Subscription.Infrastructure.Consumers;
using NomiWrite.Subscription.Infrastructure.Options;
using NomiWrite.Subscription.Infrastructure.Persistence;

namespace NomiWrite.Subscription.Infrastructure;

// UC73 — FeaturesJson column on SubscriptionPlan (do not run — reference only):
// dotnet ef migrations add AddPlanFeaturesJson --project NomiWrite.Subscription.Infrastructure --startup-project NomiWrite.Subscription.API --context SubscriptionDbContext

public static class DependencyInjection
{
    public static IServiceCollection AddSubscriptionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SubscriptionDb")
            ?? throw new InvalidOperationException("Connection string 'SubscriptionDb' is not configured.");

        services.AddDbContext<SubscriptionDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<ISubscriptionDbContext>(sp => sp.GetRequiredService<SubscriptionDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<ISubscriptionService, SubscriptionService>();

        services.AddScoped<IValidator<CreatePlanRequestDto>, CreatePlanRequestValidator>();
        services.AddScoped<IValidator<UpdatePlanRequestDto>, UpdatePlanRequestValidator>();
        services.AddScoped<IAdminPlanService, AdminPlanService>();
        services.AddScoped<IAdminSubscriptionService, AdminSubscriptionService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentCompletedEventConsumer>();

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
