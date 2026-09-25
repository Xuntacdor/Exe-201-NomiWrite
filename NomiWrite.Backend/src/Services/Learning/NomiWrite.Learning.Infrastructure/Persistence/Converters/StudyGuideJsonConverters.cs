using System.Text.Json;
using NomiWrite.Learning.Domain.Entities;
using NomiWrite.Learning.Infrastructure.Persistence.Configurations;

namespace NomiWrite.Learning.Infrastructure.Persistence.Converters;

/// <summary>
/// JSON (de)serializers for the study-guide jsonb blobs. Strengths/weaknesses
/// used to be stored as a plain string array; they are now objects with an
/// English statement and a Vietnamese explanation. <see cref="FromJson"/> stays
/// tolerant of the legacy shape so cached guides written by older versions keep
/// loading without a data migration.
/// </summary>
public static class StudyGuideJsonConverters
{
    public static string ToJson(List<StudyGuideInsight> insights) =>
        JsonSerializer.Serialize(insights, JsonOptions.JsonSerializerOptions);

    public static List<StudyGuideInsight> FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<StudyGuideInsight>>(
                       json, JsonOptions.JsonSerializerOptions)
                   ?? new List<StudyGuideInsight>();
        }
        catch (JsonException)
        {
            var legacy = JsonSerializer.Deserialize<List<string>>(
                             json, JsonOptions.JsonSerializerOptions)
                         ?? new List<string>();

            return legacy
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => new StudyGuideInsight { Text = s })
                .ToList();
        }
    }
}