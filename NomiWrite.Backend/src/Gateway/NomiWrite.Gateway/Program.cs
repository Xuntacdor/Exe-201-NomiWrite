using Yarp.ReverseProxy.Model;

// ═══════════════════════════════════════════════════════════════
// NomiWrite API Gateway — single entry point for Web/Mobile clients.
//
// Uses YARP (Yarp.ReverseProxy) to reverse-proxy /api/* traffic to the
// downstream microservices. Routing/cluster-destination configuration
// lives in appsettings.json (section "ReverseProxy") so destination
// addresses can be changed (e.g. to docker service names in production)
// without code changes.
//
// Swagger/OpenAPI is intentionally NOT enabled on the gateway — it is a
// pure proxy. Each downstream service keeps its own /swagger endpoint.
// ═══════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// Register YARP and load route/cluster configuration from appsettings.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Permissive CORS for development — the gateway is the single entry point
// used by Web/Mobile clients, so any origin/method/header is allowed.
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Basic request logging: log method, path, target cluster (from YARP),
// response status and elapsed duration. The cluster id reveals which
// downstream service each request was routed to — useful for debugging.
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    var method = context.Request.Method;
    var path = context.Request.Path.ToString();

    await next();

    stopwatch.Stop();
    var clusterId = context.Features.Get<IReverseProxyFeature>()?.Cluster?.Config?.ClusterId ?? "-";

    logger.LogInformation(
        "{Method} {Path} -> cluster {ClusterId} responded {StatusCode} in {Elapsed}ms",
        method,
        path,
        clusterId,
        context.Response.StatusCode,
        stopwatch.ElapsedMilliseconds);
});

app.UseCors("GatewayCors");

app.MapReverseProxy();

// Gateway health check — independent of downstream service availability.
app.MapGet("/health", () => Results.Ok(new { Status = "Gateway Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
