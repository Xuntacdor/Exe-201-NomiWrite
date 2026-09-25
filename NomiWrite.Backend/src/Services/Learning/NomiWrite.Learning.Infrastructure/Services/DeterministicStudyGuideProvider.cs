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
    private static readonly IReadOnlyDictionary<string, string> CriterionVi = new Dictionary<string, string>
    {
        ["Task Achievement"] = "Trả lời đúng và đủ trọng tâm câu hỏi.",
        ["Coherence and Cohesion"] = "Độ mạch lạc giữa các đoạn và cách liên kết ý.",
        ["Lexical Resource"] = "Vốn từ vựng học thuật và cách dùng từ chính xác.",
        ["Grammatical Range and Accuracy"] = "Sự đa dạng và độ chính xác của cấu trúc ngữ pháp.",
        ["Vocabulary"] = "Vốn từ vựng học thuật.",
        ["Structure"] = "Bố cục bài viết."
    };

    public StudyGuideResult GenerateGuide(StudyGuideGenerationRequest request)
    {
        var strengths = BuildStrengths(request);
        var weaknesses = BuildWeaknesses(request);
        var steps = BuildSteps(request);
        var topic = BuildTopic(request);

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
            return "No graded essays yet. Submit your next essay for feedback so the roadmap can be scored against real criterion data. Viết một bài để kế hoạch học được cá nhân hóa theo đúng trình độ của bạn.";

        var band = ComputeBand(request);
        var weakest = AggregateCriteria(request).OrderBy(c => c.Value).FirstOrDefault().Key;
        var strongest = AggregateCriteria(request).OrderByDescending(c => c.Value).FirstOrDefault().Key;

        return $"Your recent essays average band {band:0.0}. {strongest} is your strongest area; focus on raising {weakest} to reach your target faster. Điểm trung bình hiện tại của bạn là {band:0.0} — ưu tiên cải thiện {Vi(weakest)}.";
    }

    private static List<StudyGuideInsight> BuildStrengths(StudyGuideGenerationRequest request)
    {
        var strengths = new List<StudyGuideInsight>();

        foreach (var (criterion, average) in AggregateCriteria(request)
                     .OrderByDescending(c => c.Value)
                     .Where(c => c.Value >= 6.5m)
                     .Take(2))
        {
            strengths.Add(new StudyGuideInsight
            {
                Text = $"{criterion} is consistently strong at {average:0.0} across your recent essays.",
                ExplanationVi = $"{Vi(criterion)} điểm trung bình {average:0.0} — đây là thế mạnh cần giữ vững."
            });
        }

        if (request.Vocabulary.Any(v => v.IsMastered))
        {
            var masteredCount = request.Vocabulary.Count(v => v.IsMastered);
            strengths.Add(new StudyGuideInsight
            {
                Text = $"You have already mastered {masteredCount} suggested vocabulary word{(masteredCount == 1 ? "" : "s")}.",
                ExplanationVi = $"Bạn đã ghi nhớ {masteredCount} từ vựng được gợi ý — tiếp tục vận dụng chúng vào bài viết."
            });
        }

        if (strengths.Count == 0 && request.EssaySummaries.Count > 0)
        {
            strengths.Add(new StudyGuideInsight
            {
                Text = "You submit essays regularly and receive feedback — the strongest habit for improving your band.",
                ExplanationVi = "Bạn nộp bài đều đặn và đọc kỹ phản hồi — đây là thói quen giúp tăng điểm nhanh nhất."
            });
        }

        return strengths;
    }

    private static List<StudyGuideInsight> BuildWeaknesses(StudyGuideGenerationRequest request)
    {
        var weaknesses = new List<StudyGuideInsight>();

        foreach (var (criterion, average) in AggregateCriteria(request)
                     .OrderBy(c => c.Value)
                     .Where(c => c.Value < 6.5m)
                     .Take(2))
        {
            weaknesses.Add(new StudyGuideInsight
            {
                Text = $"{criterion} is your lowest criterion at {average:0.0}.",
                ExplanationVi = $"{Vi(criterion)} đang ở mức {average:0.0} — dưới ngưỡng mục tiêu, cần ưu tiên luyện phần này."
            });
        }

        var topGrammar = request.GrammarAggregates
            .OrderByDescending(g => g.Count)
            .FirstOrDefault();

        if (topGrammar is { Count: > 0 } && !string.IsNullOrWhiteSpace(topGrammar.Category))
        {
            weaknesses.Add(new StudyGuideInsight
            {
                Text = $"'{topGrammar.Category}' is your most recurring grammar issue ({topGrammar.Count} occurrences).",
                ExplanationVi = $"Lỗi ngữ pháp '{topGrammar.Category}' lặp lại {topGrammar.Count} lần — cần luyện chủ đề này trước tiên."
            });
        }

        if (weaknesses.Count == 0 && request.EssaySummaries.Count > 0)
        {
            weaknesses.Add(new StudyGuideInsight
            {
                Text = "Your criterion scores are evenly balanced; pushing every criterion above 6.5 will move the overall band.",
                ExplanationVi = "Các tiêu chí đang khá đồng đều; nâng từng tiêu chí lên trên 6.5 sẽ kéo điểm tổng lên."
            });
        }

        return weaknesses;
    }

    private static List<StudyGuideStep> BuildSteps(StudyGuideGenerationRequest request)
    {
        var steps = new List<StudyGuideStep>();
        var criterionScores = AggregateCriteria(request);
        var weakestCriterion = criterionScores.OrderBy(c => c.Value).FirstOrDefault().Key;
        var weakCriterionAverage = !string.IsNullOrWhiteSpace(weakestCriterion)
            ? criterionScores[weakestCriterion]
            : (decimal?)null;
        var topGrammar = request.GrammarAggregates
            .OrderByDescending(g => g.Count)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(weakestCriterion))
        {
            var focus = MapCriterionToFocus(weakestCriterion);
            steps.Add(new StudyGuideStep
            {
                Title = $"Raise {weakestCriterion}",
                Description = $"Write one short essay per day that deliberately targets {weakestCriterion}, then review the criterion comment in your feedback.",
                ExplanationVi = $"Viết mỗi ngày một bài ngắn tập trung vào {Vi(weakestCriterion)}, rồi đọc lại nhận xét tiêu chí này trong phản hồi.",
                Focus = focus,
                ActionType = "write_essay",
                ActionTarget = $"/write?focus={focus}"
            });
        }

        if (topGrammar is { Count: > 0 })
        {
            steps.Add(new StudyGuideStep
            {
                Title = $"Fix '{topGrammar.Category}'",
                Description = $"Do a grammar drill on '{topGrammar.Category}': correct every example from your history and write 3 new sentences avoiding the same mistake.",
                ExplanationVi = $"Sửa lỗi '{topGrammar.Category}': chữa từng ví dụ trong lịch sử và viết 3 câu mới tránh lặp lại lỗi.",
                Focus = "grammar",
                ActionType = "review_history",
                ActionTarget = "/history"
            });
        }

        var unmastered = request.Vocabulary.Where(v => !v.IsMastered).ToList();
        if (unmastered.Count > 0)
        {
            steps.Add(new StudyGuideStep
            {
                Title = "Activate your vocabulary",
                Description = $"Use {unmastered.Count} suggested words in your next essay and mark them mastered only after you use them correctly once.",
                ExplanationVi = $"Dùng {unmastered.Count} từ đã gợi ý trong bài kế tiếp; chỉ đánh dấu đã thuộc sau khi dùng đúng một lần.",
                Focus = "lexical",
                ActionType = "practice_vocabulary",
                ActionTarget = "/vocabulary"
            });
        }

        if (steps.Count == 0)
        {
            steps.Add(new StudyGuideStep
            {
                Title = "Keep a consistent writing habit",
                Description = "Submit at least two essays a week and review each feedback comment before starting the next one.",
                ExplanationVi = "Nộp ít nhất hai bài mỗi tuần và đọc kỹ từng góp ý trước khi viết bài tiếp theo.",
                Focus = "task_response",
                ActionType = "write_essay",
                ActionTarget = "/write"
            });
        }

        return steps.Take(3).ToList();
    }

    private static StudyGuideTopic BuildTopic(StudyGuideGenerationRequest request)
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
                Reason = "You have written about this topic least often, so it will stretch your idea development without repeated phrasing. Bạn viết chủ đề này ít nhất nên sẽ luyện được kỹ năng triển khai ý.",
                SuggestedPrompt = "Some people believe <new angle of the topic>; to what extent do you agree or disagree? Write a full essay of at least 250 words.",
                IdeaHints = new List<string>
                {
                    "Point 1: State your position clearly and give one concrete reason.",
                    "Point 2: Present the opposite view fairly, then rebut it.",
                    "Point 3: Support with a real-world example or statistic idea."
                },
                KeyVocabulary = new List<string> { "a growing concern", "increasingly common", "balanced view", "on balance" }
            };
        }

        return new StudyGuideTopic
        {
            Title = "IELTS Writing Task 2 - Opinion essay",
            Reason = "An opinion essay directly exercises position-taking and task response, your most common feedback area. Kiểu bài luận này rèn khả năng nêu quan điểm — khâu hay bị trừ điểm của bạn.",
            SuggestedPrompt = "Some people think that technology makes life less stressful, while others disagree. Discuss both views and give your own opinion.",
            IdeaHints = new List<string>
            {
                "Point 1: Technology saves time (automation, instant communication).",
                "Point 2: It can raise stress (information overload, always connected).",
                "Point 3: Your opinion — how to balance both sides."
            },
            KeyVocabulary = new List<string> { "streamline daily tasks", "information overload", "work-life balance", "at the expense of" }
        };
    }

    private static string Vi(string criterion) =>
        CriterionVi.TryGetValue(criterion, out var vi)
            ? vi
            : $"Kỹ năng '{criterion}'";

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