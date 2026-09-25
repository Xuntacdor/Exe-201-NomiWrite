using FluentAssertions;
using NomiWrite.Learning.Domain.Entities;
using NomiWrite.Learning.Infrastructure.Persistence.Converters;

namespace NomiWrite.Learning.Application.UnitTests;

public class StudyGuideJsonConvertersTests
{
    [Fact]
    public void FromJson_LegacyStringArray_WrapsItemsIntoTolerantInsights()
    {
        const string legacyJson = "[\"Coherence is strong\",\"Grammar is limited\"]";

        var result = StudyGuideJsonConverters.FromJson(legacyJson);

        result.Should().HaveCount(2);
        result[0].Text.Should().Be("Coherence is strong");
        result[0].ExplanationVi.Should().BeEmpty();
    }

    [Fact]
    public void FromJson_ObjectArray_PreservesExplanationVi()
    {
        const string json = "[{\"text\":\"Grammar range is limited\",\"explanationVi\":\"Cấu trúc câu còn ít.\"}]";

        var result = StudyGuideJsonConverters.FromJson(json);

        result.Single().Text.Should().Be("Grammar range is limited");
        result.Single().ExplanationVi.Should().Be("Cấu trúc câu còn ít.");
    }

    [Fact]
    public void ToJson_ThenFromJson_RoundTripsInsights()
    {
        var insights = new List<StudyGuideInsight>
        {
            new() { Text = "Lexical range is strong.", ExplanationVi = "Vốn từ vựng rất tốt." }
        };

        var json = StudyGuideJsonConverters.ToJson(insights);
        var roundTrip = StudyGuideJsonConverters.FromJson(json);

        roundTrip.Should().BeEquivalentTo(insights);
    }
}