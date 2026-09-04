var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// YARP Reverse Proxy — routes all client traffic to microservices
// ═══════════════════════════════════════════════════════════════
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

app.UseCors("GatewayCors");
app.MapReverseProxy();
app.MapGet("/health", () => Results.Ok(new { Status = "Gateway Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
