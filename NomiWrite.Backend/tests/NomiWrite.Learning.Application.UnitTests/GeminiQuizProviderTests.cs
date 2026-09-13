using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
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
            Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent"
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
        var sut = Build("{\"error\":{\"code\":429}}", HttpStatusCode.TooManyRequests);

        var act = () => sut.GenerateQuestionsAsync(new QuizGenerationRequest(), 5);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

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

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client) => _client = client;

        public HttpClient CreateClient(string name) => _client;
    }
}