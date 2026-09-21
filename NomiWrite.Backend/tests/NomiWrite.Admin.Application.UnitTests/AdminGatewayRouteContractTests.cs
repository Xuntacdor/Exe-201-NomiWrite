using System.Text.Json;
using FluentAssertions;

namespace NomiWrite.Admin.Application.UnitTests;

public class AdminGatewayRouteContractTests
{
    public static TheoryData<string, string, string> AdminRoutes => new()
    {
        { "admin-users-route", "/api/admin/users/{**catch-all}", "auth-cluster" },
        { "admin-writing-prompts-route", "/api/admin/prompts/{**catch-all}", "writing-cluster" },
        { "admin-writing-submissions-route", "/api/admin/submissions/{**catch-all}", "writing-cluster" },
        { "admin-payments-route", "/api/admin/payments/{**catch-all}", "payment-cluster" },
        { "admin-plans-route", "/api/admin/plans/{**catch-all}", "subscription-cluster" },
        { "admin-subscriptions-route", "/api/admin/subscriptions/{**catch-all}", "subscription-cluster" },
        { "admin-ai-config-route", "/api/admin/ai-config/{**catch-all}", "grading-cluster" },
        { "admin-analytics-route", "/api/admin/analytics/{**catch-all}", "admin-cluster" },
        { "admin-announcements-route", "/api/admin/announcements/{**catch-all}", "admin-cluster" },
        { "admin-logs-route", "/api/admin/logs/{**catch-all}", "logging-cluster" },
        { "moderation-reports-route", "/api/moderation/reports/{**catch-all}", "admin-cluster" }
    };

    [Theory]
    [MemberData(nameof(AdminRoutes))]
    public void Gateway_AdminRoute_HasExplicitOwningCluster(
        string routeName,
        string expectedPath,
        string expectedCluster)
    {
        using var config = JsonDocument.Parse(File.ReadAllText(GatewayAppSettingsPath()));
        var routes = config.RootElement.GetProperty("ReverseProxy").GetProperty("Routes");

        routes.TryGetProperty(routeName, out var route).Should().BeTrue();
        route.GetProperty("ClusterId").GetString().Should().Be(expectedCluster);
        route.GetProperty("Match").GetProperty("Path").GetString().Should().Be(expectedPath);
    }

    [Fact]
    public void Gateway_DoesNotUseBroadAdminCatchAllRoute()
    {
        using var config = JsonDocument.Parse(File.ReadAllText(GatewayAppSettingsPath()));
        var routes = config.RootElement.GetProperty("ReverseProxy").GetProperty("Routes");

        routes.EnumerateObject()
            .Select(route => route.Value.GetProperty("Match").GetProperty("Path").GetString())
            .Should()
            .NotContain("/api/admin/{**catch-all}");
    }

    private static string GatewayAppSettingsPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "Gateway",
                "NomiWrite.Gateway",
                "appsettings.json");

            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate Gateway appsettings.json from test output directory.");
    }
}
