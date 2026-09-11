using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Notification.Application.Interfaces;
using NomiWrite.Notification.Application.Services;
using NomiWrite.Notification.Infrastructure.Consumers;
using NomiWrite.Notification.Infrastructure.Options;
using NomiWrite.Notification.Infrastructure.Persistence;

namespace NomiWrite.Notification.Infrastructure;

// UC78 — AllowNullableUserIdForBroadcast (do not run — reference only):
//   dotnet ef migrations add AllowNullableUserIdForBroadcast \
//     --project src/Services/Notification/NomiWrite.Notification.Infrastructure \
//     --startup-project src/Services/Notification/NomiWrite.Notification.API \
//     --context NotificationDbContext

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NotificationDb")
            ?? throw new InvalidOperationException("Connection string 'NotificationDb' is not configured.");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<INotificationDbContext>(sp => sp.GetRequiredService<NotificationDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<INotificationService, NotificationService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredEventConsumer>();
            x.AddConsumer<GradingCompletedEventConsumer>();
            x.AddConsumer<ForumCommentCreatedEventConsumer>();
            x.AddConsumer<PostLikedEventConsumer>();
            x.AddConsumer<SubscriptionExpiringEventConsumer>();
            x.AddConsumer<SubscriptionExpiredEventConsumer>();
            x.AddConsumer<SystemAnnouncementCreatedEventConsumer>();

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
