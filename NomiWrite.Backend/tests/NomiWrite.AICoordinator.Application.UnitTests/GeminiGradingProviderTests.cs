using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.UnitTests.Persistence;
using NomiWrite.AICoordinator.Infrastructure.Options;
using NomiWrite.AICoordinator.Infrastructure.Services;

namespace NomiWrite.AICoordinator.Application.UnitTests;

public class GeminiGradingProviderTests
{
    private const string ApiKey = "test-key";
    private const string Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

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

    private static GeminiGradingProvider Build(string body)
    {
        var handler = new StubHttpMessageHandler(body);
        var httpClient = new HttpClient(handler);
        var settings = Options.Create(new GeminiSettings
        {
            ApiKey = ApiKey,
            Model = "gemini-2.5-flash",
            Endpoint = Endpoint
        });

        return new GeminiGradingProvider(
            httpClient,
            settings,
            TestGradingDbContext.Create(),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<GeminiGradingProvider>.Instance);
    }

    // Skips the real exponential backoff wait so retry-exhaustion tests stay fast.
    private static GeminiGradingProvider BuildNoBackoff(HttpMessageHandler handler)
    {
        return new NoBackoffGradingProvider(
            new HttpClient(handler),
            Options.Create(new GeminiSettings { ApiKey = ApiKey, Model = "gemini-2.5-flash", Endpoint = Endpoint }),
            TestGradingDbContext.Create(),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<GeminiGradingProvider>.Instance);
    }

    private static string GeminiBody(string text)
    {
        return JsonSerializer.Serialize(new
        {
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        parts = new object[]
                        {
                            new { text }
                        }
                    }
                }
            }
        });
    }

    #region U-G8 — Gemini response parsing robustness

    [Fact]
    public async Task GradeEssayAsync_ValidJson_Parses()
    {
        var sut = Build(GeminiBody(ValidGradingJson));

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        result.Criteria.Should().ContainSingle(c => c.Name == "Task Achievement");
    }

    [Fact]
    public async Task GradeEssayAsync_MarkdownFencedJson_Parses()
    {
        var fenced = $"```json\n{ValidGradingJson}\n```";
        var sut = Build(GeminiBody(fenced));

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        result.Criteria.Should().ContainSingle(c => c.Name == "Task Achievement");
    }

    [Fact]
    public async Task GradeEssayAsync_MissingCandidates_Throws()
    {
        var sut = Build("{\"text\":\"nope\"}");

        var act = () => sut.GradeEssayAsync("essay");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*did not contain any text content*");
    }

    [Fact]
    public async Task GradeEssayAsync_EmptyParts_Throws()
    {
        var body = JsonSerializer.Serialize(new
        {
            candidates = new[]
            {
                new { content = new { parts = new object[] { } } }
            }
        });
        var sut = Build(body);

        var act = () => sut.GradeEssayAsync("essay");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GradeEssayAsync_ProseAroundJson_Parses()
    {
        var prose = $"Here is your band: {ValidGradingJson}";
        var sut = Build(GeminiBody(prose));

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        result.Criteria.Should().ContainSingle(c => c.Name == "Task Achievement");
    }

    [Fact]
    public async Task GradeEssayAsync_NonSuccessHttp_Throws_FriendlyMessage()
    {
        var sut = BuildNoBackoff(new StubHttpMessageHandler(
            "{\"error\":{\"code\":429,\"message\":\"Quota exceeded\"}}",
            HttpStatusCode.TooManyRequests));

        var act = () => sut.GradeEssayAsync("essay");

        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.And.Message.Should().Contain("temporarily unavailable");
        exception.And.Message.Should().NotContain("Quota exceeded");
    }

    [Fact]
    public async Task GradeEssayAsync_TransientThenSuccess_RetriesAndSucceeds()
    {
        var handler = new SequenceHttpMessageHandler(
            (HttpStatusCode.TooManyRequests, "{\"error\":{\"code\":429}}"),
            (HttpStatusCode.ServiceUnavailable, "{\"error\":{\"code\":503}}"),
            (HttpStatusCode.OK, GeminiBody(ValidGradingJson)));
        var sut = BuildNoBackoff(handler);

        var result = await sut.GradeEssayAsync("essay");

        result.OverallBand.Should().Be(7.0m);
        handler.CallCount.Should().Be(3);
    }

    #endregion

    private sealed class NoBackoffGradingProvider : GeminiGradingProvider
    {
        public NoBackoffGradingProvider(
            HttpClient httpClient,
            IOptions<GeminiSettings> settings,
            NomiWrite.AICoordinator.Application.Interfaces.IGradingDbContext dbContext,
            IMemoryCache cache,
            ILogger<GeminiGradingProvider> logger)
            : base(httpClient, settings, dbContext, cache, logger)
        {
        }

        protected override Task DelayBackoffAsync(TimeSpan delay, CancellationToken cancellationToken) =>
            Task.CompletedTask;
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
}
