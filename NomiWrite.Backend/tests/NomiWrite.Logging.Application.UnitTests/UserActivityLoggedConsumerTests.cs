using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Logging.Application.Interfaces;
using NomiWrite.Logging.Domain.Entities;
using NomiWrite.Logging.Infrastructure.Consumers;
using NomiWrite.Shared.Contracts.Events.Logging;

namespace NomiWrite.Logging.Application.UnitTests;

public class UserActivityLoggedConsumerTests
{
    private readonly IActivityLogRepository _repository = Substitute.For<IActivityLogRepository>();
    private readonly UserActivityLoggedConsumer _sut;

    public UserActivityLoggedConsumerTests()
    {
        _sut = new UserActivityLoggedConsumer(_repository, NullLogger<UserActivityLoggedConsumer>.Instance);
    }

    private static ConsumeContext<UserActivityLoggedEvent> Context(
        UserActivityLoggedEvent evt,
        CancellationToken token = default)
    {
        var ctx = Substitute.For<ConsumeContext<UserActivityLoggedEvent>>();
        ctx.Message.Returns(evt);
        ctx.CancellationToken.Returns(token);
        return ctx;
    }

    [Fact]
    public async Task Consume_ValidEvent_PersistsMappedLog()
    {
        var timestamp = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc);
        var evt = new UserActivityLoggedEvent(
            UserId: "user-42",
            Action: "LOGIN",
            ServiceName: "Auth",
            IpAddress: "203.0.113.9",
            UserAgent: "Mozilla/5.0",
            Metadata: new Dictionary<string, object>
            {
                ["score"] = 95,
                ["provider"] = "google",
                ["details"] = new Dictionary<string, object> { ["tier"] = "premium" },
                ["tags"] = new object[] { "a", "b" }
            },
            Timestamp: timestamp);

        await _sut.Consume(Context(evt));

        await _repository.Received(1).AddAsync(
            Arg.Is<UserActivityLog>(log =>
                log.UserId == "user-42" &&
                log.Action == "LOGIN" &&
                log.ServiceName == "Auth" &&
                log.IpAddress == "203.0.113.9" &&
                log.UserAgent == "Mozilla/5.0" &&
                log.Timestamp == timestamp &&
                log.Metadata!["score"].AsInt32 == 95 &&
                log.Metadata!["provider"].AsString == "google" &&
                log.Metadata!["details"].AsBsonDocument["tier"].AsString == "premium" &&
                log.Metadata!["tags"].AsBsonArray.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_DefaultTimestamp_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var evt = new UserActivityLoggedEvent("u-1", "SUBMIT_ESSAY", "Writing", null, null, null, default);

        await _sut.Consume(Context(evt));

        await _repository.Received(1).AddAsync(
            Arg.Is<UserActivityLog>(log => log.Timestamp >= before && log.Timestamp <= DateTime.UtcNow),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_NullMetadata_PersistsNullMetadata()
    {
        var evt = new UserActivityLoggedEvent("u-1", "LOGOUT", "Auth", null, null, null, DateTime.UtcNow);

        await _sut.Consume(Context(evt));

        await _repository.Received(1).AddAsync(
            Arg.Is<UserActivityLog>(log =>
                log.UserId == "u-1" &&
                log.Action == "LOGOUT" &&
                log.Metadata == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_EmptyMetadata_PersistsNullMetadata()
    {
        var evt = new UserActivityLoggedEvent(
            "u-1", "LOGIN", "Auth", null, null, new Dictionary<string, object>(), DateTime.UtcNow);

        await _sut.Consume(Context(evt));

        await _repository.Received(1).AddAsync(
            Arg.Is<UserActivityLog>(log => log.Metadata == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_NonStandardValue_FallsBackToString()
    {
        var evt = new UserActivityLoggedEvent(
            "u-1", "LOGIN", "Auth", null, null,
            new Dictionary<string, object> { ["uri"] = new Uri("https://example.com/path") },
            DateTime.UtcNow);

        await _sut.Consume(Context(evt));

        await _repository.Received(1).AddAsync(
            Arg.Is<UserActivityLog>(log => log.Metadata!["uri"].AsString == "https://example.com/path"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var evt = new UserActivityLoggedEvent("u-1", "LOGIN", "Auth", null, null, null, DateTime.UtcNow);

        await _sut.Consume(Context(evt, cts.Token));

        await _repository.Received(1).AddAsync(Arg.Any<UserActivityLog>(), cts.Token);
    }
}