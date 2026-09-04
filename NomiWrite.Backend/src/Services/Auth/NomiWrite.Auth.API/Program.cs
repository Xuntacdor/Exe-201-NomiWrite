using NomiWrite.Auth.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure (PostgreSQL, MassTransit/RabbitMQ, Services) ──
builder.Services.AddAuthInfrastructure(builder.Configuration);

// ── Controllers & Swagger ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NomiWrite Auth Service",
        Version = "v1"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Service = "AuthService", Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
