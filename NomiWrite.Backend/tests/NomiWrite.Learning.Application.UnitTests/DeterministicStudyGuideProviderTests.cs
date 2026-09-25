using FluentAssertions;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Infrastructure.Services;

namespace NomiWrite.Learning.Application.UnitTests;

public class DeterministicStudyGuideProviderTests
{
    private static StudyGuideGenerationRequest BuildRequest()
    {
        return new StudyGuideGenerationRequest
        {
            TargetExam = "IELTS Academic - Writing Task 2",
            TargetBand = 7m,
            EssaySummaries = new List<GradedEssaySummaryDto>
            {
                new()
                {
                    SubmissionId = Guid.NewGuid(),
                    OverallBand = 6.5m,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    CriteriaScores = new Dictionary<string, decimal>
                    {
                        ["Task Achievement"] = 6.5m,
                        ["Coherence and Cohesion"] = 7.0m,
                        ["Lexical Resource"] = 6.0m,
                        ["Grammatical Range and Accuracy"] = 6.0m
                    }
                }
            },
            Topics = new List<EssayTopicDto>
            {
                new() { SubmissionId = Guid.NewGuid(), Title = "Technology in education" }
            },
            GrammarAggregates = new List<GrammarAggregateDto>
            {
                new() { Category = "Tense", Count = 3, Sentences = new List<string> { "He go." } }
            },
            Vocabulary = new List<VocabSnapshotDto>
            {
                new() { OriginalWord = "good", SuggestedWord = "beneficial", IsMastered = false },
                new() { OriginalWord = "bad", SuggestedWord = "adverse", IsMastered = true }
            },
            QuizStats = new QuizStatsDto { AttemptCount = 2, AverageAccuracy = 70 }
        };
    }

    [Fact]
    public void GenerateGuide_ProducesBilingualInsightsAndActionableSteps()
    {
        var provider = new DeterministicStudyGuideProvider();

        var result = provider.GenerateGuide(BuildRequest());

        result.Strengths.Should().NotBeEmpty();
        result.Strengths.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.Text));
        result.Strengths.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.ExplanationVi));

        result.Weaknesses.Should().NotBeEmpty();
        result.Weaknesses.Should().OnlyContain(w => !string.IsNullOrWhiteSpace(w.ExplanationVi));

        result.NextSteps.Should().OnlyContain(s => ActionTypeIsValid(s.ActionType));
        result.NextSteps.Should().OnlyContain(s => ActionTargetIsSafe(s.ActionTarget));
        result.NextSteps.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.ExplanationVi));
    }

    [Fact]
    public void GenerateGuide_RecommendedTopic_IncludesIdeaHintsAndKeyVocabulary()
    {
        var provider = new DeterministicStudyGuideProvider();

        var result = provider.GenerateGuide(BuildRequest());

        result.RecommendedTopic.Title.Should().NotBeNullOrWhiteSpace();
        result.RecommendedTopic.SuggestedPrompt.Should().NotBeNullOrWhiteSpace();
        result.RecommendedTopic.IdeaHints.Should().NotBeEmpty();
        result.RecommendedTopic.IdeaHints.Should().OnlyContain(h => h.StartsWith("Point", StringComparison.OrdinalIgnoreCase));
        result.RecommendedTopic.KeyVocabulary.Should().NotBeEmpty();
    }

    private static bool ActionTypeIsValid(string actionType) =>
        actionType is "write_essay" or "review_history" or "practice_vocabulary" or "practice_quiz" or "none";

    private static bool ActionTargetIsSafe(string target) =>
        string.IsNullOrEmpty(target) ||
        (target.StartsWith("/", StringComparison.Ordinal) &&
         !target.StartsWith("//", StringComparison.Ordinal) &&
         !target.StartsWith("http", StringComparison.OrdinalIgnoreCase));
}