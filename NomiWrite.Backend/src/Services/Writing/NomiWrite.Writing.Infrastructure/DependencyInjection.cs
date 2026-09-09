using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Application.Services;
using NomiWrite.Writing.Application.Validation;
using NomiWrite.Writing.Infrastructure.Options;
using NomiWrite.Writing.Infrastructure.Persistence;

namespace NomiWrite.Writing.Infrastructure;

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
