using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Application.Services;
using NomiWrite.Learning.Infrastructure.Clients;
using NomiWrite.Learning.Infrastructure.Consumers;
using NomiWrite.Learning.Infrastructure.Options;
using NomiWrite.Learning.Infrastructure.Persistence;
using NomiWrite.Learning.Infrastructure.Services;

namespace NomiWrite.Learning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GeminiSettings>(configuration.GetSection(GeminiSettings.SectionName));

        var serviceUrls = configuration.GetSection(ServiceUrls.SectionName).Get<ServiceUrls>() ?? new ServiceUrls();

        services.AddDbContext<LearningDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("LearningDb"),
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<ILearningDbContext>(sp => sp.GetRequiredService<LearningDbContext>());

        services.AddScoped<IVocabularyService, VocabularyService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IStudyGuideService, StudyGuideService>();

        services.AddScoped<IAiQuizProvider, GeminiQuizProvider>();
        services.AddScoped<IFallbackQuizProvider, DeterministicQuizProvider>();
        services.AddScoped<IStudyGuideAiProvider, GeminiStudyGuideProvider>();
        services.AddScoped<IFallbackStudyGuideProvider, DeterministicStudyGuideProvider>();

        services.AddScoped<IEssayHistoryClient, EssayHistoryClient>();
        services.AddHttpClient("GradingService", client =>
        {
            client.BaseAddress = new Uri(serviceUrls.GradingService);
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient("WritingService", client =>
        {
            client.BaseAddress = new Uri(serviceUrls.WritingService);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddHttpClient("Gemini", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<GradingCompletedEventConsumer>();

            busConfigurator.UsingRabbitMq((context, rabbitConfig) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var port = ushort.TryParse(configuration["RabbitMQ:Port"], out var p) ? p : (ushort)5672;
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                rabbitConfig.Host(host, port, "/", hostConfig =>
                {
                    hostConfig.Username(username);
                    hostConfig.Password(password);
                });

                rabbitConfig.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
