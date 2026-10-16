using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Domain.Entities;
using NomiWrite.AICoordinator.Application.UnitTests.Persistence;
using NomiWrite.AICoordinator.Infrastructure.Options;
using NomiWrite.AICoordinator.Infrastructure.Services;

namespace NomiWrite.AICoordinator.Application.UnitTests;

public class OpenRouterGradingProviderTests
{
    private const string ApiKey = "test-key";
    private const string Endpoint = "https://openrouter.ai/api/v1/chat/completions";
    private const string PrimaryModel = "nvidia/nemotron-3.5-lightning:free";
    private const string SecondaryModel = "liquid/lfm-2.5-2.6b:free";

    private static readonly string ValidGradingJson = JsonSerializer.Serialize(new
    {
        overallBand = 7.0,
        criteria = new[]
        {
            new { name = "Task Achievement", score = 7.0, comment = "Clear." }
        },
        overallFeedback = "Great essay.",
        grammarErrors = new object[] { },
        vocabularySuggestions = new object[] { },
        restructuringSuggestions = new object[] { }
    });

    private static OpenRouterSettings DefaultSettings(string models = PrimaryModel + "," + SecondaryModel) => new()
    {
        ApiKey = ApiKey,
        Models = models,
        FallbackModels = "openrouter/free",
        Endpoint = Endpoint
    };

    private static OpenRouterGradingProvider Build(string body)
    {
        var handler = new StubHttpMessageHandler(body);
        return new OpenRouterGradingProvider(
            new HttpClient(handler),
            Options.Create(DefaultSettings()),
            TestGradingDbContext.Create(),
            new TestSecretProtector(),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<OpenRouterGradingProvider>.Instance);
    }

    // Skips the real exponential backoff wait so retry-exhaustion tests stay fast.
    private static OpenRouterGradingProvider BuildNoBackoff(HttpMessageHandler handler, OpenRouterSettings? settings = null)
    {
        return new NoBackoffGradingProvider(
            new HttpClient(handler),
            Options.Create(settings ?? DefaultSettings()),
            TestGradingDbContext.Create(),
            new TestSecretProtector(),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<OpenRouterGradingProvider>.Instance);
    }

    private static string OpenRouterBody(string text)
    {
        return JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new { content = text }
                }
            }
        });
    }

    #region OpenRouter response parsing robustness

    [Fact]
    public async Task GradeEssayAsync_ValidJson_Parses()
    {
        var sut = Build(OpenRouterBody(ValidGradingJson));

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        result.Criteria.Should().ContainSingle(c => c.Name == "Task Achievement");
    }

    [Fact]
    public async Task GradeEssayAsync_MarkdownFencedJson_Parses()
    {
        var fenced = $"```json\n{ValidGradingJson}\n```";
        var sut = Build(OpenRouterBody(fenced));

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        result.Criteria.Should().ContainSingle(c => c.Name == "Task Achievement");
    }

    [Fact]
    public async Task GradeEssayAsync_ProseAroundJson_Parses()
    {
        var prose = $"Here is your band: {ValidGradingJson}";
        var sut = Build(OpenRouterBody(prose));

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        result.Criteria.Should().ContainSingle(c => c.Name == "Task Achievement");
    }

    [Fact]
    public async Task GradeEssayAsync_MissingChoices_Throws()
    {
        var sut = BuildNoBackoff(new StubHttpMessageHandler("{\"text\":\"nope\"}"));

        var act = () => sut.GradeEssayAsync("essay");

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*temporarily unavailable*");
    }

    [Fact]
    public async Task GradeEssayAsync_EmptyContent_Throws()
    {
        var body = JsonSerializer.Serialize(new
        {
            choices = new[] { new { message = new { content = string.Empty } } }
        });
        var sut = BuildNoBackoff(new StubHttpMessageHandler(body));

        var act = () => sut.GradeEssayAsync("essay");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GradeEssayAsync_SendsBearerTokenAndFreeModel()
    {
        string? authorization = null;
        string? requestedModel = null;
        var handler = new CallbackHttpMessageHandler(request =>
        {
            authorization = request.Headers.Authorization?.ToString();
            requestedModel = ExtractModelFromBody(request);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(OpenRouterBody(ValidGradingJson))
            };
        });
        var sut = BuildNoBackoff(handler);

        await sut.GradeEssayAsync("essay");

        authorization.Should().Be($"Bearer {ApiKey}");
        requestedModel.Should().Be(PrimaryModel);
    }

    #endregion

    #region Model switching

    [Fact]
    public async Task GradeEssayAsync_PaidOrUnavailableModel_SwitchesToNextFreeModel()
    {
        var requestedModels = new List<string>();
        var handler = new CallbackHttpMessageHandler(request =>
        {
            var model = ExtractModelFromBody(request)!;
            requestedModels.Add(model);
            return model == PrimaryModel
                ? new HttpResponseMessage(HttpStatusCode.PaymentRequired) { Content = new StringContent("paid only") }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OpenRouterBody(ValidGradingJson)) };
        });
        var sut = BuildNoBackoff(handler);

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        requestedModels.Should().Equal(PrimaryModel, SecondaryModel);
    }

    [Fact]
    public async Task GradeEssayAsync_RateLimitedThenSuccess_RetriesSameModel()
    {
        var handler = new SequenceHttpMessageHandler(
            (HttpStatusCode.TooManyRequests, "{\"error\":{\"message\":\"rate limited\"}}"),
            (HttpStatusCode.OK, OpenRouterBody(ValidGradingJson)));
        var sut = BuildNoBackoff(handler, DefaultSettings(PrimaryModel));

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        handler.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task GradeEssayAsync_AllModelsFail_ThrowsFriendlyMessage()
    {
        var sut = BuildNoBackoff(
            new StubHttpMessageHandler("{\"error\":{\"message\":\"no capacity\"}}", HttpStatusCode.TooManyRequests),
            DefaultSettings(PrimaryModel));

        var act = () => sut.GradeEssayAsync("essay");

        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.And.Message.Should().Contain("temporarily unavailable");
        exception.And.Message.Should().NotContain("no capacity");
    }

    [Fact]
    public async Task GradeEssayAsync_OnlyCallsFreeModels()
    {
        var requestedModels = new List<string>();
        var handler = new CallbackHttpMessageHandler(request =>
        {
            requestedModels.Add(ExtractModelFromBody(request)!);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(OpenRouterBody(ValidGradingJson))
            };
        });

        // A paid model configured in the environment must be ignored entirely.
        var sut = BuildNoBackoff(handler, new OpenRouterSettings
        {
            ApiKey = ApiKey,
            Models = "openai/gpt-4o,nvidia/nemotron-3.5-lightning:free",
            FallbackModels = "openrouter/free",
            Endpoint = Endpoint
        });

        await sut.GradeEssayAsync("essay");

        requestedModels.Should().OnlyContain(model =>
            model == "openrouter/free" || model.EndsWith(":free", StringComparison.OrdinalIgnoreCase));
        requestedModels.Should().NotContain("openai/gpt-4o");
    }

    [Fact]
    public async Task GradeEssayAsync_RuntimeConfig_FreeModelOverridesEnvironmentAndUsesStoredKey()
    {
        var context = TestGradingDbContext.Create();
        context.AiGradingConfigs.Add(new AiGradingConfig
        {
            ProviderName = "OpenRouter",
            ModelName = "google/gemma-4-31b-it:free",
            ApiKeyCiphertext = "protected:runtime-secret-key",
            IsActive = true
        });
        await context.SaveChangesAsync();

        string? requestedModel = null;
        string? requestedAuth = null;
        var handler = new CallbackHttpMessageHandler(request =>
        {
            requestedModel = ExtractModelFromBody(request);
            requestedAuth = request.Headers.Authorization?.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(OpenRouterBody(ValidGradingJson))
            };
        });
        var sut = new OpenRouterGradingProvider(
            new HttpClient(handler),
            Options.Create(new OpenRouterSettings
            {
                ApiKey = "environment-key",
                Models = "nvidia/nemotron-3.5-lightning:free",
                Endpoint = Endpoint
            }),
            context,
            new TestSecretProtector(),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<OpenRouterGradingProvider>.Instance);

        await sut.GradeEssayAsync("essay");

        requestedModel.Should().Be("google/gemma-4-31b-it:free");
        requestedAuth.Should().Be("Bearer runtime-secret-key");
    }

    [Fact]
    public async Task GradeEssayAsync_RuntimeConfig_PaidModelIsIgnored()
    {
        var context = TestGradingDbContext.Create();
        context.AiGradingConfigs.Add(new AiGradingConfig
        {
            ProviderName = "OpenRouter",
            ModelName = "openai/gpt-4o",
            IsActive = true
        });
        await context.SaveChangesAsync();

        string? requestedModel = null;
        var handler = new CallbackHttpMessageHandler(request =>
        {
            requestedModel = ExtractModelFromBody(request);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(OpenRouterBody(ValidGradingJson))
            };
        });
        var sut = new OpenRouterGradingProvider(
            new HttpClient(handler),
            Options.Create(DefaultSettings(PrimaryModel)),
            context,
            new TestSecretProtector(),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<OpenRouterGradingProvider>.Instance);

        await sut.GradeEssayAsync("essay");

        requestedModel.Should().Be(PrimaryModel);
    }

    #endregion

    private static string? ExtractModelFromBody(HttpRequestMessage request)
    {
        var json = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("model", out var model)
            ? model.GetString()
            : null;
    }

    private sealed class NoBackoffGradingProvider : OpenRouterGradingProvider
    {
        public NoBackoffGradingProvider(
            HttpClient httpClient,
            IOptions<OpenRouterSettings> settings,
            IGradingDbContext dbContext,
            IAiSecretProtector secretProtector,
            IMemoryCache cache,
            ILogger<OpenRouterGradingProvider> logger)
            : base(httpClient, settings, dbContext, secretProtector, cache, logger)
        {
        }

        protected override Task DelayBackoffAsync(TimeSpan delay, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class TestSecretProtector : IAiSecretProtector
    {
        public bool IsConfigured => true;
        public string Protect(string plaintext) => $"protected:{plaintext}";
        public string Unprotect(string ciphertext) => ciphertext.StartsWith("protected:") ? ciphertext[10..] : ciphertext;
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _status;

        public StubHttpMessageHandler(string body, HttpStatusCode status = HttpStatusCode.OK)
        {
            _body = body;
            _status = status;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    private sealed class SequenceHttpMessageHandler : HttpMessageHandler
    {
        private readonly (HttpStatusCode Status, string Body)[] _responses;
        private int _callCount;

        public SequenceHttpMessageHandler(params (HttpStatusCode Status, string Body)[] responses)
        {
            _responses = responses;
        }

        public int CallCount => _callCount;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var index = Math.Min(_callCount, _responses.Length - 1);
            _callCount++;
            var (status, body) = _responses[index];
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class CallbackHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(callback(request));
    }
}
