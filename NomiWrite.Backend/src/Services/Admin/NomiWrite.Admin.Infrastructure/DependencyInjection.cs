using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Interfaces;
using NomiWrite.Admin.Application.Options;
using NomiWrite.Admin.Application.Services;
using NomiWrite.Admin.Application.Validation;
using NomiWrite.Admin.Infrastructure.Options;
using NomiWrite.Admin.Infrastructure.Persistence;
using NomiWrite.Admin.Infrastructure.Services;

namespace NomiWrite.Admin.Infrastructure;

// EF Core migration command (run from NomiWrite.Backend; do not run — reference only):
//   dotnet ef migrations add InitialCreate \
//     --project src/Services/Admin/NomiWrite.Admin.Infrastructure \
//     --startup-project src/Services/Admin/NomiWrite.Admin.API \
//     --context AdminDbContext

public static class DependencyInjection
{
    public static IServiceCollection AddAdminInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AdminDb")
            ?? throw new InvalidOperationException("Connection string 'AdminDb' is not configured.");

        services.AddDbContext<AdminDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IAdminDbContext>(sp => sp.GetRequiredService<AdminDbContext>());

        services.Configure<ServiceUrls>(configuration.GetSection(ServiceUrls.SectionName));
        services.Configure<AnalyticsSettings>(configuration.GetSection(AnalyticsSettings.SectionName));

        services.AddHttpClient<AnalyticsApiClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(
                configuration.GetSection(AnalyticsSettings.SectionName).Get<AnalyticsSettings>()?.TimeoutSeconds
                    ?? 5);
        });

        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        services.AddScoped<IAnalyticsService, AnalyticsAggregatorService>();
        services.AddScoped<IContentReportService, ContentReportService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();

        services.AddScoped<IValidator<CreateReportRequestDto>, CreateReportRequestValidator>();
        services.AddScoped<IValidator<ResolveReportRequestDto>, ResolveReportRequestValidator>();
        services.AddScoped<IValidator<AnnouncementRequestDto>, AnnouncementRequestValidator>();

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