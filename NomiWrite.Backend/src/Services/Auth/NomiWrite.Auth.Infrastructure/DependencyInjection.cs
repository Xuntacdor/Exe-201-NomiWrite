using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Application.Options;
using NomiWrite.Auth.Application.Services;
using NomiWrite.Auth.Application.Validation;
using NomiWrite.Auth.Infrastructure.Options;
using NomiWrite.Auth.Infrastructure.Persistence;
using NomiWrite.Auth.Infrastructure.Security;
using NomiWrite.Auth.Infrastructure.Services;

namespace NomiWrite.Auth.Infrastructure;

// To apply the new EmailVerificationTokens / PasswordResetTokens tables and any schema changes, run (do not run — reference only):
// dotnet ef migrations add AddEmailVerificationAndPasswordReset --project NomiWrite.Auth.Infrastructure --startup-project NomiWrite.Auth.API --context AuthDbContext

// UC71 — AccountStatus + Moderator role column (do not run — reference only):
// dotnet ef migrations add AddAccountStatusAndRoles --project NomiWrite.Auth.Infrastructure --startup-project NomiWrite.Auth.API --context AuthDbContext

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
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
        services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IValidator<RegisterRequestDto>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();
        services.AddScoped<IValidator<VerifyEmailRequestDto>, VerifyEmailRequestValidator>();
        services.AddScoped<IValidator<ResendVerificationEmailRequestDto>, ResendVerificationEmailRequestValidator>();
        services.AddScoped<IValidator<ForgotPasswordRequestDto>, ForgotPasswordRequestValidator>();
        services.AddScoped<IValidator<ResetPasswordRequestDto>, ResetPasswordRequestValidator>();

        services.AddScoped<IValidator<UpdateUserStatusRequestDto>, UpdateUserStatusRequestValidator>();
        services.AddScoped<IValidator<UpdateUserRoleRequestDto>, UpdateUserRoleRequestValidator>();
        services.AddScoped<IAdminUserService, AdminUserService>();

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
