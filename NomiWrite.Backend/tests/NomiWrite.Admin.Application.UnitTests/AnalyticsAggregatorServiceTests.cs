using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Admin.Application.Options;
using NomiWrite.Admin.Infrastructure.Options;
using NomiWrite.Admin.Infrastructure.Services;
using OptionsSet = Microsoft.Extensions.Options.Options;

namespace NomiWrite.Admin.Application.UnitTests;

public class AnalyticsAggregatorServiceTests
{
    private static ServiceUrls Urls() => new()
    {
        AuthService = "http://auth.local/",
        WritingService = "http://writing.local/",
        PaymentService = "http://payment.local/",
        SubscriptionService = "http://subscription.local/"
    };

    private static AnalyticsAggregatorService Build(StubHttpMessageHandler handler, string? authorization = null)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://x.local/") };
        var apiClient = new AnalyticsApiClient(client);
        var httpContext = Substitute.For<IHttpContextAccessor>();
        if (authorization is null)
        {
            httpContext.HttpContext.Returns((HttpContext?)null);
        }
        else
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Authorization = authorization;
            httpContext.HttpContext.Returns(context);
        }

        return new AnalyticsAggregatorService(
            apiClient,
            OptionsSet.Create(Urls()),
            OptionsSet.Create(new AnalyticsSettings { CacheTtlMinutes = 5 }),
            new MemoryCache(new MemoryCacheOptions()),
            httpContext,
            NullLogger<AnalyticsAggregatorService>.Instance);
    }

    private const string AuthJson =
        """{"totalUsers":120,"activeUsers":45,"bannedUsers":3}""";
    private const string WritingJson =
        """{"totalSubmissions":380,"gradedSubmissions":260,"pendingSubmissions":40}""";
    private const string PaymentJson =
        """{"totalRevenue":1250000.50,"completedTransactions":61}""";
    private const string SubscriptionJson =
        """{"activeVipMembers":18}""";

    #region U-AD1 — parallel aggregation + fail-soft

    [Fact]
    public async Task GetOverview_AllUpstreamsOk_PopulatesAllMetrics()
    {
        var handler = new StubHttpMessageHandler();
        var sut = Build(handler);

        var result = await sut.GetOverviewAsync();

        result.TotalUsers.Should().Be(120);
        result.ActiveUsers.Should().Be(45);
        result.BannedUsers.Should().Be(3);
        result.TotalSubmissions.Should().Be(380);
        result.GradedSubmissions.Should().Be(260);
        result.PendingSubmissions.Should().Be(40);
        result.TotalRevenue.Should().Be(1250000.50m);
        result.CompletedTransactions.Should().Be(61);
        result.ActiveVipMembers.Should().Be(18);
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOverview_AuthFails_WarnsButOthersPopulated()
    {
        var handler = new StubHttpMessageHandler();
        handler.SetFailure("auth", (int)HttpStatusCode.InternalServerError);
        var sut = Build(handler);

        var result = await sut.GetOverviewAsync();

        result.Warnings.Should().ContainSingle(w => w.Contains("Auth service"));
        result.TotalUsers.Should().Be(0);
        result.TotalSubmissions.Should().Be(380);
        result.ActiveVipMembers.Should().Be(18);
    }

    [Fact]
    public async Task GetOverview_AllUpstreamsFail_WarningsListedDataEmpty()
    {
        var handler = new StubHttpMessageHandler();
        handler.SetFailure("auth", 500);
        handler.SetFailure("writing", 500);
        handler.SetFailure("payment", 503);
        handler.SetFailure("subscription", 502);
        var sut = Build(handler);

        var result = await sut.GetOverviewAsync();

        result.Warnings.Should().HaveCount(4);
        result.TotalUsers.Should().Be(0);
        result.TotalSubmissions.Should().Be(0);
    }

    [Fact]
    public async Task GetOverview_SecondCallServedFromCache()
    {
        var handler = new StubHttpMessageHandler();
        var sut = Build(handler);

        var first = await sut.GetOverviewAsync();
        var second = await sut.GetOverviewAsync();

        second.GeneratedAt.Should().Be(first.GeneratedAt);
        handler.CallCount.Should().Be(4);
    }

    [Fact]
    public async Task GetOverview_ForwardsCallerAuthorizationToAdminUpstreams()
    {
        var handler = new StubHttpMessageHandler();
        var sut = Build(handler, "Bearer admin-token");

        await sut.GetOverviewAsync();

        handler.AuthorizationHeaders.Should().HaveCount(4);
        handler.AuthorizationHeaders.Should().OnlyContain(h => h == "Bearer admin-token");
    }

    #endregion

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, int> _failures = new(StringComparer.OrdinalIgnoreCase);
        public List<string?> AuthorizationHeaders { get; } = new();
        public int CallCount { get; private set; }

        public void SetFailure(string service, int statusCode) => _failures[service] = statusCode;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            AuthorizationHeaders.Add(
                request.Headers.TryGetValues("Authorization", out var values)
                    ? values.SingleOrDefault()
                    : null);

            var path = request.RequestUri!.AbsolutePath;

            if (_failures.TryGetValue(ServiceKey(path), out var statusCode))
            {
                return Task.FromResult(new HttpResponseMessage((HttpStatusCode)statusCode));
            }

            var body = ServiceJson(path);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }

        private static string ServiceKey(string path)
        {
            if (path.Contains("/users/analytics")) return "auth";
            if (path.Contains("/submissions/analytics")) return "writing";
            if (path.Contains("/payments/analytics")) return "payment";
            return "subscription";
        }

        private static string ServiceJson(string path)
        {
            return ServiceKey(path) switch
            {
                "auth" => AuthJson,
                "writing" => WritingJson,
                "payment" => PaymentJson,
                _ => SubscriptionJson
            };
        }
    }
}
