using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Domain.Entities;
using NomiWrite.Learning.Infrastructure.Options;
using NomiWrite.Learning.Infrastructure.Services;

namespace NomiWrite.Learning.Application.UnitTests;

public class GeminiQuizProviderTests
{
    private static GeminiQuizProvider Build(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new StubHttpMessageHandler(body, status);
        var client = new HttpClient(handler);
        var factory = new StubHttpClientFactory(client);
        var options = Options.Create(new GeminiSettings
        {
            ApiKey = "test-key",
            Model = "gemini-3.6-flash",
            Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent"
        });
        return new GeminiQuizProvider(factory, options, NullLogger<GeminiQuizProvider>.Instance);
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
                        parts = new object[] { new { text } }
                    }
                }
            }
        });
    }

    private static string QuestionsJson() => JsonSerializer.Serialize(new
    {
        questions = new[]
        {
            new
            {
                id = "q1",
                category = "Grammar",
                type = "multiple_choice",
                question = "Pick the best option.",
                sentence = "",
                options = new[] { "a", "b", "c", "d" },
                correctAnswer = "b",
                explanation = "because"
            }
        }
    });

    #region U-L10 — Gemini quiz provider JSON hardening

    [Fact]
    public async Task GenerateQuestionsAsync_MarkdownFencedJson_StripsFences()
    {
        var fenced = $"```json\n{QuestionsJson()}\n```";
        var sut = Build(GeminiBody(fenced));

        var result = await sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        result.Should().ContainSingle(q => q.Id == "q1");

        // The QuizService would then run NormalizeQuestions; here we also verify the
        // provider returns the answer key so the service can persist it safely.
        result.Single().CorrectAnswer.Should().Be("b");
    }

    [Fact]
    public async Task GenerateQuestionsAsync_EmptyQuestionsArray_Throws()
    {
        var sut = Build(GeminiBody("{\"questions\":[]}"));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_MissingCandidates_Throws()
    {
        var sut = Build("{}");

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_EmptyTextPart_Throws()
    {
        var sut = Build(GeminiBody(string.Empty));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*empty quiz response*");
    }

    [Fact]
    public async Task GenerateQuestionsAsync_NonJsonProse_Throws()
    {
        var sut = Build(GeminiBody("Sorry, here is some prose without JSON."));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_NonSuccessStatus_Throws()
    {
        var sut = BuildNoBackoff(new SequenceHttpMessageHandler(
            (HttpStatusCode.TooManyRequests, "{\"error\":{\"code\":429}}")));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_TransientThenSuccess_RetriesAndSucceeds()
    {
        var handler = new SequenceHttpMessageHandler(
            (HttpStatusCode.ServiceUnavailable, "{\"error\":{\"code\":503}}"),
            (HttpStatusCode.TooManyRequests, "{\"error\":{\"code\":429}}"),
            (HttpStatusCode.OK, GeminiBody(QuestionsJson())));
        var sut = BuildNoBackoff(handler);

        var result = await sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        result.Should().ContainSingle(q => q.Id == "q1");
        handler.CallCount.Should().Be(3);
    }

    #endregion

    // Skips the real exponential backoff wait so retry tests stay fast.
    private static GeminiQuizProvider BuildNoBackoff(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        var factory = new StubHttpClientFactory(client);
        var options = Options.Create(new GeminiSettings
        {
            ApiKey = "test-key",
            Model = "gemini-3.6-flash",
            Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent"
        });
        return new NoBackoffQuizProvider(factory, options, NullLogger<GeminiQuizProvider>.Instance);
    }

    private sealed class NoBackoffQuizProvider : GeminiQuizProvider
    {
        public NoBackoffQuizProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<GeminiSettings> geminiOptions,
            ILogger<GeminiQuizProvider> logger)
            : base(httpClientFactory, geminiOptions, logger)
        {
        }

        protected override Task DelayBackoffAsync(TimeSpan delay, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _status;

        public StubHttpMessageHandler(string body, HttpStatusCode status)
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

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client) => _client = client;

        public HttpClient CreateClient(string name) => _client;
    }
}
