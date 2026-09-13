using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
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
    public async Task GradeEssayAsync_MarkdownFencedJson_Throws()
    {
        var fenced = $"```json\n{ValidGradingJson}\n```";
        var sut = Build(GeminiBody(fenced));

        var act = () => sut.GradeEssayAsync("essay");

        await act.Should().ThrowAsync<JsonException>();
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
    public async Task GradeEssayAsync_ProseAroundJson_Throws()
    {
        var prose = $"Here is your band: {ValidGradingJson}";
        var sut = Build(GeminiBody(prose));

        var act = () => sut.GradeEssayAsync("essay");

        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task GradeEssayAsync_NonSuccessHttp_Throws()
    {
        var handler = new StubHttpMessageHandler(
            "{\"error\":{\"code\":429,\"message\":\"Quota exceeded\"}}",
            HttpStatusCode.TooManyRequests);
        var httpClient = new HttpClient(handler);
        var sut = new GeminiGradingProvider(
            httpClient,
            Options.Create(new GeminiSettings { ApiKey = ApiKey, Model = "gemini-2.5-flash", Endpoint = Endpoint }),
            TestGradingDbContext.Create(),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<GeminiGradingProvider>.Instance);

        var act = () => sut.GradeEssayAsync("essay");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

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
}