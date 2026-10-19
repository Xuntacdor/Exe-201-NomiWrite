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

public class GroqQuizProviderTests
{
    private const string PrimaryModel = "openai/gpt-oss-120b";
    private const string SecondaryModel = "qwen/qwen3.8-27b";

    private static GroqSettings DefaultSettings(string models = PrimaryModel + "," + SecondaryModel) => new()
    {
        ApiKey = "test-key",
        Models = models,
        Endpoint = "https://api.groq.com/openai/v1/chat/completions"
    };

    private static GroqQuizProvider Build(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new StubHttpMessageHandler(body, status);
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        return new GroqQuizProvider(factory, Options.Create(DefaultSettings()), NullLogger<GroqQuizProvider>.Instance);
    }

    private static GroqQuizProvider BuildNoBackoff(HttpMessageHandler handler, GroqSettings? settings = null)
    {
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        return new NoBackoffQuizProvider(factory, Options.Create(settings ?? DefaultSettings()), NullLogger<GroqQuizProvider>.Instance);
    }

    private static string GroqBody(string text)
    {
        return JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new { message = new { content = text } }
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

    #region Quiz provider JSON hardening

    [Fact]
    public async Task GenerateQuestionsAsync_MarkdownFencedJson_StripsFences()
    {
        var fenced = $"```json\n{QuestionsJson()}\n```";
        var sut = Build(GroqBody(fenced));

        var result = await sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        result.Should().ContainSingle(q => q.Id == "q1");
        result.Single().CorrectAnswer.Should().Be("b");
    }

    [Fact]
    public async Task GenerateQuestionsAsync_EmptyQuestionsArray_Throws()
    {
        var sut = BuildNoBackoff(new StubHttpMessageHandler(GroqBody("{\"questions\":[]}"), HttpStatusCode.OK));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_MissingChoices_Throws()
    {
        var sut = BuildNoBackoff(new StubHttpMessageHandler("{}", HttpStatusCode.OK));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_EmptyTextPart_Throws()
    {
        var sut = BuildNoBackoff(new StubHttpMessageHandler(GroqBody(string.Empty), HttpStatusCode.OK));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_NonJsonProse_Throws()
    {
        var sut = BuildNoBackoff(new StubHttpMessageHandler(GroqBody("Sorry, here is some prose without JSON."), HttpStatusCode.OK));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_NonSuccessStatus_Throws()
    {
        var sut = BuildNoBackoff(new SequenceHttpMessageHandler(
            (HttpStatusCode.TooManyRequests, "{\"error\":{\"message\":\"rate limited\"}}")));

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GenerateQuestionsAsync_TransientThenSuccess_RetriesAndSucceeds()
    {
        var handler = new SequenceHttpMessageHandler(
            (HttpStatusCode.ServiceUnavailable, "{\"error\":{\"message\":\"overloaded\"}}"),
            (HttpStatusCode.OK, GroqBody(QuestionsJson())));
        var sut = BuildNoBackoff(handler, DefaultSettings(PrimaryModel));

        var result = await sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        result.Should().ContainSingle(q => q.Id == "q1");
        handler.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task GenerateQuestionsAsync_UnavailableModel_SwitchesToNextModel()
    {
        var requestedModels = new List<string>();
        var handler = new CallbackHttpMessageHandler(request =>
        {
            var model = ExtractModelFromBody(request)!;
            requestedModels.Add(model);
            return model == PrimaryModel
                ? new HttpResponseMessage(HttpStatusCode.PaymentRequired) { Content = new StringContent("unavailable") }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(GroqBody(QuestionsJson())) };
        });
        var sut = BuildNoBackoff(handler);

        var result = await sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        result.Should().ContainSingle(q => q.Id == "q1");
        requestedModels.Should().Equal(PrimaryModel, SecondaryModel);
    }

    #endregion

    private static string? ExtractModelFromBody(HttpRequestMessage request)
    {
        var json = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("model", out var model) ? model.GetString() : null;
    }

    private sealed class NoBackoffQuizProvider : GroqQuizProvider
    {
        public NoBackoffQuizProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<GroqSettings> groqOptions,
            ILogger<GroqQuizProvider> logger)
            : base(httpClientFactory, groqOptions, logger)
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

    private sealed class CallbackHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(callback(request));
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client) => _client = client;

        public HttpClient CreateClient(string name) => _client;
    }
}
