using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.User.Application.DTOs;
using NomiWrite.User.Application.Interfaces;
using NomiWrite.User.Application.Services;
using NomiWrite.User.Application.Validation;
using NomiWrite.User.Infrastructure.Clients;
using NomiWrite.User.Infrastructure.Consumers;
using NomiWrite.User.Infrastructure.Options;
using NomiWrite.User.Infrastructure.Persistence;

namespace NomiWrite.User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UserDb")
            ?? throw new InvalidOperationException("Connection string 'UserDb' is not configured.");

        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IUserDbContext>(sp => sp.GetRequiredService<UserDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var serviceUrls = configuration.GetSection(ServiceUrls.SectionName).Get<ServiceUrls>() ?? new ServiceUrls();
        services.AddHttpClient<ISubscriptionStatusClient, SubscriptionServiceClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.SubscriptionService);
            client.Timeout = TimeSpan.FromSeconds(3);
        });

        services.AddHttpClient<IWritingHistoryClient, WritingHistoryClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.WritingService);
            client.Timeout = TimeSpan.FromSeconds(3);
        });

        services.AddHttpClient<IGradingHistoryClient, GradingHistoryClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.AICoordinatorService);
            client.Timeout = TimeSpan.FromSeconds(3);
        });

        services.AddScoped<IValidator<UpdateProfileRequestDto>, UpdateProfileRequestValidator>();
        services.AddScoped<IUserProfileService, UserProfileService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredEventConsumer>();

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
