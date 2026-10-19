using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Infrastructure.Options;
using NomiWrite.Writing.Infrastructure.Services;

namespace NomiWrite.Writing.Application.UnitTests;

public class GroqAutocompleteProviderTests
{
    private const string PrimaryModel = "qwen/qwen3.8-27b";
    private const string SecondaryModel = "allam-2-7b";

    private static GroqSettings Settings(string models = PrimaryModel + "," + SecondaryModel) => new()
    {
        ApiKey = "test-key",
        Models = models,
        Endpoint = "https://api.groq.com/openai/v1/chat/completions"
    };

    private static string Body(string text) => JsonSerializer.Serialize(new
    {
        choices = new[]
        {
            new { message = new { content = text } }
        }
    });

    private static GroqAutocompleteProvider Build(HttpMessageHandler handler, GroqSettings? settings = null)
    {
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        return new NoBackoffProvider(
            factory,
            Options.Create(settings ?? Settings()),
            NullLogger<GroqAutocompleteProvider>.Instance);
    }

    [Fact]
    public async Task CompleteAsync_Success_ReturnsTrimmedSuggestion()
    {
        var sut = Build(new StubHandler(Body("\"a natural continuation.\""), HttpStatusCode.OK));

        var result = await sut.CompleteAsync(
            new AutocompleteContext("prompt", "In my view,"), CancellationToken.None);

        result.Should().Be("a natural continuation.");
    }

    [Fact]
    public async Task CompleteAsync_BlankPrefix_ReturnsNullWithoutCallingApi()
    {
        var handler = new StubHandler(Body("unused"), HttpStatusCode.OK);
        var sut = Build(handler);

        var result = await sut.CompleteAsync(
            new AutocompleteContext("prompt", "   "), CancellationToken.None);

        result.Should().BeNull();
        handler.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task CompleteAsync_EmptyContent_TriesFallbackModelThenReturnsNull()
    {
        var handler = new StubHandler(Body(string.Empty), HttpStatusCode.OK);
        var sut = Build(handler);

        var result = await sut.CompleteAsync(
            new AutocompleteContext("prompt", "In my view,"), CancellationToken.None);

        result.Should().BeNull();
        handler.CallCount.Should().Be(2);
        handler.Models.Should().Equal(PrimaryModel, SecondaryModel);
    }

    [Fact]
    public async Task CompleteAsync_PrimaryUnavailable_SwitchesToFallback()
    {
        var handler = new CallbackHandler(request =>
        {
            var model = ExtractModel(request);
            return model == PrimaryModel
                ? new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("{\"error\":{\"message\":\"model_not_found\"}}")
                }
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(Body("fallback continuation"))
                };
        });
        var sut = Build(handler);

        var result = await sut.CompleteAsync(
            new AutocompleteContext("prompt", "In my view,"), CancellationToken.None);

        result.Should().Be("fallback continuation");
    }

    [Fact]
    public async Task CompleteAsync_TransientThenSuccess_Retries()
    {
        var handler = new SequenceHandler(
            (HttpStatusCode.ServiceUnavailable, "{}"),
            (HttpStatusCode.OK, Body("recovered continuation")));
        var sut = Build(handler);

        var result = await sut.CompleteAsync(
            new AutocompleteContext("prompt", "In my view,"), CancellationToken.None);

        result.Should().Be("recovered continuation");
        handler.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task CompleteAsync_AllModelsFail_ReturnsNull()
    {
        var sut = Build(new StubHandler("not json", HttpStatusCode.InternalServerError));

        var result = await sut.CompleteAsync(
            new AutocompleteContext("prompt", "In my view,"), CancellationToken.None);

        result.Should().BeNull();
    }

    private static string? ExtractModel(HttpRequestMessage request)
    {
        var json = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("model", out var model) ? model.GetString() : null;
    }

    private sealed class NoBackoffProvider : GroqAutocompleteProvider
    {
        public NoBackoffProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<GroqSettings> groqOptions,
            Microsoft.Extensions.Logging.ILogger<GroqAutocompleteProvider> logger)
            : base(httpClientFactory, groqOptions, logger)
        {
        }

        protected override Task DelayBackoffAsync(TimeSpan delay, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _status;

        public StubHandler(string body, HttpStatusCode status)
        {
            _body = body;
            _status = status;
        }

        public int CallCount { get; private set; }
        public List<string> Models { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Models.Add(ExtractModel(request) ?? string.Empty);
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly (HttpStatusCode Status, string Body)[] _responses;
        private int _callCount;

        public SequenceHandler(params (HttpStatusCode Status, string Body)[] responses)
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

    private sealed class CallbackHandler(Func<HttpRequestMessage, HttpResponseMessage> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(callback(request));
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client) => _client = client;

        public HttpClient CreateClient(string name) => _client;
    }
}
