using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Services;

/// <summary>
/// Deterministic, offline study guide built purely from the aggregated writing
/// history. Produces the same JSON contract as the AI provider so the feature
/// stays usable when the LLM is unavailable.
/// </summary>
public class DeterministicStudyGuideProvider : IFallbackStudyGuideProvider
{
    public StudyGuideResult GenerateGuide(StudyGuideGenerationRequest request)
    {
        var strengths = BuildStrengths(request);
        var weaknesses = BuildWeaknesses(request);
        var steps = BuildSteps(request, strengths, weaknesses);
        var topic = BuildTopic(request, weaknesses);

        return new StudyGuideResult
        {
            Summary = BuildSummary(request),
            EstimatedBand = ComputeBand(request),
            Strengths = strengths,
            Weaknesses = weaknesses,
            NextSteps = steps,
            RecommendedTopic = topic
        };
    }

    private static string BuildSummary(StudyGuideGenerationRequest request)
    {
        if (request.EssaySummaries.Count == 0)
            return "No graded essays yet. Submit your next essay for feedback so the roadmap can be scored against real criterion data.";

        var band = ComputeBand(request);
        var weakest = AggregateCriteria(request).OrderBy(c => c.Value).FirstOrDefault().Key;
        var strongest = AggregateCriteria(request).OrderByDescending(c => c.Value).FirstOrDefault().Key;

        return $"Your recent essays average band {band:0.0}. {strongest} is your strongest area; focus on raising {weakest} to reach your target faster.";
    }

    private static List<string> BuildStrengths(StudyGuideGenerationRequest request)
    {
        var strengths = new List<string>();

        foreach (var (criterion, average) in AggregateCriteria(request)
                     .OrderByDescending(c => c.Value)
                     .Where(c => c.Value >= 6.5m)
                     .Take(2))
        {
            strengths.Add($"{criterion} is consistently strong at {average:0.0} across your recent essays.");
        }

        if (request.Vocabulary.Any(v => v.IsMastered))
            strengths.Add($"You have already mastered {request.Vocabulary.Count(v => v.IsMastered)} suggested vocabulary words.");

        if (strengths.Count == 0 && request.EssaySummaries.Count > 0)
            strengths.Add("You submit essays regularly and receive feedback — the strongest habit for improving your band.");

        return strengths;
    }

    private static List<string> BuildWeaknesses(StudyGuideGenerationRequest request)
    {
        var weaknesses = new List<string>();

        foreach (var (criterion, average) in AggregateCriteria(request)
                     .OrderBy(c => c.Value)
                     .Where(c => c.Value < 6.5m)
                     .Take(2))
        {
            weaknesses.Add($"{criterion} is your lowest criterion at {average:0.0}.");
        }

        var topGrammar = request.GrammarAggregates
            .OrderByDescending(g => g.Count)
            .FirstOrDefault();

        if (topGrammar is { Count: > 0 } && !string.IsNullOrWhiteSpace(topGrammar.Category))
            weaknesses.Add($"'{topGrammar.Category}' is your most recurring grammar issue ({topGrammar.Count} occurrences).");

        if (weaknesses.Count == 0 && request.EssaySummaries.Count > 0)
            weaknesses.Add("Your criterion scores are evenly balanced; pushing every criterion above 6.5 will move the overall band.");

        return weaknesses;
    }

    private static List<StudyGuideStep> BuildSteps(
        StudyGuideGenerationRequest request,
        List<string> strengths,
        List<string> weaknesses)
    {
        var steps = new List<StudyGuideStep>();
        var criterionScores = AggregateCriteria(request);
        var weakestCriterion = criterionScores.OrderBy(c => c.Value).FirstOrDefault().Key;
        var topGrammar = request.GrammarAggregates
            .OrderByDescending(g => g.Count)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(weakestCriterion))
        {
            steps.Add(new StudyGuideStep
            {
                Title = $"Raise {weakestCriterion}",
                Description = $"Write one short essay per day that deliberately targets {weakestCriterion}, then review the criterion comment in your feedback.",
                Focus = MapCriterionToFocus(weakestCriterion)
            });
        }

        if (topGrammar is { Count: > 0 })
        {
            steps.Add(new StudyGuideStep
            {
                Title = $"Fix '{topGrammar.Category}'",
                Description = $"Do a grammar drill on '{topGrammar.Category}': correct every example from your history and write 3 new sentences avoiding the same mistake.",
                Focus = "grammar"
            });
        }

        var unmastered = request.Vocabulary.Where(v => !v.IsMastered).ToList();
        if (unmastered.Count > 0)
        {
            steps.Add(new StudyGuideStep
            {
                Title = "Activate your vocabulary",
                Description = $"Use {unmastered.Count} suggested words in your next essay and mark them mastered only after you use them correctly once.",
                Focus = "lexical"
            });
        }

        if (steps.Count == 0)
        {
            steps.Add(new StudyGuideStep
            {
                Title = "Keep a consistent writing habit",
                Description = "Submit at least two essays a week and review each feedback comment before starting the next one.",
                Focus = "task_response"
            });
        }

        return steps.Take(3).ToList();
    }

    private static StudyGuideTopic BuildTopic(StudyGuideGenerationRequest request, List<string> weaknesses)
    {
        var leastWritten = request.Topics
            .GroupBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Count())
            .FirstOrDefault()
            ?.Key;

        if (!string.IsNullOrWhiteSpace(leastWritten))
        {
            return new StudyGuideTopic
            {
                Title = $"A fresh take on \"{leastWritten}\"",
                Reason = "You have written about this topic least often, so it will stretch your idea development without repeated phrasing.",
                SuggestedPrompt = "Some people believe <new angle of the topic>; to what extent do you agree or disagree? Write a full essay of at least 250 words."
            };
        }

        return new StudyGuideTopic
        {
            Title = "IELTS Writing Task 2 - Opinion essay",
            Reason = "An opinion essay directly exercises position-taking and task response, your most common feedback area.",
            SuggestedPrompt = "Some people think that technology makes life less stressful, while others disagree. Discuss both views and give your own opinion."
        };
    }

    private static string MapCriterionToFocus(string criterion)
    {
        if (criterion.Contains("Coherence", StringComparison.OrdinalIgnoreCase))
            return "coherence";

        if (criterion.Contains("Lexical", StringComparison.OrdinalIgnoreCase))
            return "lexical";

        if (criterion.Contains("Task", StringComparison.OrdinalIgnoreCase))
            return "task_response";

        if (criterion.Contains("Grammatical", StringComparison.OrdinalIgnoreCase))
            return "grammar";

        return "task_response";
    }

    private static Dictionary<string, decimal> AggregateCriteria(StudyGuideGenerationRequest request)
    {
        return request.EssaySummaries
            .SelectMany(e => e.CriteriaScores)
            .GroupBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => Math.Round(g.Average(kvp => kvp.Value), 1));
    }

    private static decimal ComputeBand(StudyGuideGenerationRequest request)
    {
        if (request.EssaySummaries.Count == 0)
            return 0;

        return Math.Round(
            request.EssaySummaries.Average(e => e.OverallBand) * 2,
            MidpointRounding.AwayFromZero) / 2;
    }
}