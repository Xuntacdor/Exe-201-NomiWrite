using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Application.Services;
using NomiWrite.Writing.Application.Validation;
using NomiWrite.Writing.Infrastructure.Clients;
using NomiWrite.Writing.Infrastructure.Options;
using NomiWrite.Writing.Infrastructure.Persistence;

namespace NomiWrite.Writing.Infrastructure;

// To apply the new WritingPrompt (time_limit_minutes, image_url, sample_answer) and
// WritingSubmission (deadline_at, submitted_late) columns, run (do not run — reference only):
// dotnet ef migrations add AddTimingImageAndSampleAnswer --project NomiWrite.Writing.Infrastructure --startup-project NomiWrite.Writing.API --context WritingDbContext

public static class DependencyInjection
{
    public static IServiceCollection AddWritingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WritingDb")
            ?? throw new InvalidOperationException("Connection string 'WritingDb' is not configured.");

        services.AddDbContext<WritingDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IWritingDbContext>(sp => sp.GetRequiredService<WritingDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var serviceUrls = configuration.GetSection(ServiceUrls.SectionName).Get<ServiceUrls>() ?? new ServiceUrls();
        services.AddHttpClient<ISubscriptionStatusClient, SubscriptionServiceClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.SubscriptionService);
            client.Timeout = TimeSpan.FromSeconds(3);
        });

        services.AddScoped<IValidator<CreateSubmissionRequestDto>, CreateSubmissionRequestValidator>();
        services.AddScoped<IValidator<UpdateSubmissionRequestDto>, UpdateSubmissionRequestValidator>();
        services.AddScoped<IWritingService, WritingService>();

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
