using System.Text.Json;
using System.Text.Json.Serialization;

namespace NomiWrite.Learning.Infrastructure.Persistence.Configurations;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}