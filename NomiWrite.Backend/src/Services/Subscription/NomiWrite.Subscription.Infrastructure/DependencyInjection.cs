using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Subscription.Application.Interfaces;
using NomiWrite.Subscription.Application.Services;
using NomiWrite.Subscription.Infrastructure.Consumers;
using NomiWrite.Subscription.Infrastructure.Options;
using NomiWrite.Subscription.Infrastructure.Persistence;

namespace NomiWrite.Subscription.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSubscriptionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SupabaseDb")
            ?? configuration["SUPABASE_DB_CONNECTION_STRING"]
            ?? configuration.GetConnectionString("SubscriptionDb")
            ?? throw new InvalidOperationException("Connection string 'SubscriptionDb' is not configured.");

        services.AddDbContext<SubscriptionDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<ISubscriptionDbContext>(sp => sp.GetRequiredService<SubscriptionDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<ISubscriptionService, SubscriptionService>();

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
