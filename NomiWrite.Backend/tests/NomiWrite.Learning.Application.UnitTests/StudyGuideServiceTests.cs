using System.Net;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Exceptions;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Application.Services;
using NomiWrite.Learning.Application.UnitTests.Persistence;
using NomiWrite.Learning.Application.Validation;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.UnitTests;

public class StudyGuideServiceTests
{
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private static StudyGuideService Build(
        TestLearningDbContext db,
        IEssayHistoryClient? history = null,
        IStudyGuideAiProvider? ai = null,
        IFallbackStudyGuideProvider? fallback = null)
    {
        history ??= StubHistory();
        ai ??= StubAi();
        fallback ??= StubFallback();

        return new StudyGuideService(db, history, ai, fallback, new GenerateStudyGuideRequestValidator(),
            NullLogger<StudyGuideService>.Instance);
    }

    private static IEssayHistoryClient StubHistory()
    {
        var history = Substitute.For<IEssayHistoryClient>();
        history.GetGradingHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<GradedEssaySummaryDto>
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
            });
        history.GetWrittenTopicsAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<EssayTopicDto>
            {
                new() { SubmissionId = Guid.NewGuid(), Title = "Technology in education" }
            });
        return history;
    }

    private static IStudyGuideAiProvider StubAi()
    {
        var ai = Substitute.For<IStudyGuideAiProvider>();
        ai.GenerateGuideAsync(Arg.Any<StudyGuideGenerationRequest>())
            .Returns(new StudyGuideResult
            {
                Summary = "Solid foundations with clear room to grow.",
                EstimatedBand = 6.5m,
                Strengths = new List<StudyGuideInsight> { new() { Text = "Coherence and Cohesion is your strongest criterion." } },
                Weaknesses = new List<StudyGuideInsight> { new() { Text = "Lexical Resource holds back your band." } },
                NextSteps = new List<StudyGuideStep>
                {
                    new() { Title = "Raise Lexical Resource", Description = "Use 5 new collocations.", Focus = "lexical" },
                    new() { Title = "Fix tense control", Description = "Drill past tenses.", Focus = "grammar" },
                    new() { Title = "Outline before writing", Description = "Plan paragraphs first.", Focus = "task_response" }
                },
                RecommendedTopic = new StudyGuideTopic
                {
                    Title = "Renewable energy and jobs",
                    Reason = "Exercises the weakest lexical area.",
                    SuggestedPrompt = "Many people think that..."
                }
            });
        return ai;
    }

    private static IFallbackStudyGuideProvider StubFallback()
    {
        var fallback = Substitute.For<IFallbackStudyGuideProvider>();
        fallback.GenerateGuide(Arg.Any<StudyGuideGenerationRequest>())
            .Returns(new StudyGuideResult
            {
                Summary = "Fallback guide.",
                EstimatedBand = 6.5m,
                Strengths = new List<StudyGuideInsight> { new() { Text = "Consistent submission habit." } },
                Weaknesses = new List<StudyGuideInsight> { new() { Text = "Lexical Resource is the lowest criterion." } },
                NextSteps = new List<StudyGuideStep>
                {
                    new() { Title = "Raise Lexical Resource", Description = "Write daily.", Focus = "lexical" }
                },
                RecommendedTopic = new StudyGuideTopic { Title = "Opinion essay", Reason = "Stretch.", SuggestedPrompt = "..." }
            });
        return fallback;
    }

    private static GrammarError SeedGrammarError(TestLearningDbContext db, Guid userId,
        string category = "Tense", string sentence = "He go.", string suggestion = "He goes.")
    {
        var error = new GrammarError
        {
            UserId = userId,
            SubmissionId = Guid.NewGuid(),
            GrammarCategory = category,
            Sentence = sentence,
            ErrorPart = "go",
            Suggestion = suggestion,
            Explanation = "Fix verb form.",
            CreatedAt = DateTime.UtcNow
        };
        db.GrammarErrors.Add(error);
        db.SaveChanges();
        return error;
    }

    private static VocabSuggestion SeedVocab(TestLearningDbContext db, Guid userId,
        string word = "good", string suggested = "beneficial", bool isMastered = false)
    {
        var vocab = new VocabSuggestion
        {
            UserId = userId,
            SubmissionId = Guid.NewGuid(),
            Topic = "Vocabulary",
            OriginalWord = word,
            SuggestedWord = suggested,
            ExampleSentence = $"The {word} habit.",
            IsMastered = isMastered,
            CreatedAt = DateTime.UtcNow
        };
        db.VocabSuggestions.Add(vocab);
        db.SaveChanges();
        return vocab;
    }

    private static StudyGuide SeedGuide(TestLearningDbContext db, Guid userId)
    {
        var guide = new StudyGuide
        {
            UserId = userId,
            TargetExam = "IELTS Academic - Writing Task 2",
            TargetBand = 7m,
            Summary = "Existing cached summary.",
            EstimatedBand = 6.5m,
            Strengths = new List<StudyGuideInsight> { new() { Text = "Old strength." } },
            Weaknesses = new List<StudyGuideInsight> { new() { Text = "Old weakness." } },
            NextSteps = new List<StudyGuideStep>
            {
                new() { Title = "Old step", Description = "Old.", Focus = "grammar" }
            },
            RecommendedTopic = new StudyGuideTopic { Title = "Old topic", Reason = "Old.", SuggestedPrompt = "Old." },
            AnalyzedEssayCount = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        db.StudyGuides.Add(guide);
        db.SaveChanges();
        return guide;
    }

    #region U-L10 — study guide skeleton + insufficient data

    [Fact]
    public async Task GenerateGuideAsync_NoHistoryAndNoWeakPoints_ThrowsInsufficientData()
    {
        var db = TestLearningDbContext.Create();
        var history = Substitute.For<IEssayHistoryClient>();
        history.GetGradingHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<GradedEssaySummaryDto>());
        history.GetWrittenTopicsAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new List<EssayTopicDto>());

        var sut = Build(db, history: history);
        var act = () => sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto(), null);

        await act.Should().ThrowAsync<StudyGuideInsufficientDataException>();
        db.StudyGuides.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateGuideAsync_InvalidTargetBand_ThrowsValidation()
    {
        var db = TestLearningDbContext.Create();

        var sut = Build(db);
        var act = () => sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto { TargetBand = 12m }, null);

        await act.Should().ThrowAsync<ValidationException>();
        db.StudyGuides.Should().BeEmpty();
    }

    #endregion

    #region U-L11 — generation via AI provider + persistence

    [Fact]
    public async Task GenerateGuideAsync_WithHistory_PersistsLatestGuideAndReturnsDto()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA, category: "Preposition");
        SeedVocab(db, UserA, word: "good", suggested: "beneficial");

        StudyGuideGenerationRequest? captured = null;
        var ai = Substitute.For<IStudyGuideAiProvider>();
        ai.GenerateGuideAsync(Arg.Do<StudyGuideGenerationRequest>(r => captured = r))
            .Returns(new StudyGuideResult
            {
                Summary = "Solid foundations.",
                EstimatedBand = 6.5m,
                Strengths = new List<StudyGuideInsight> { new() { Text = "Coherence 7.0 is your strongest criterion." } },
                Weaknesses = new List<StudyGuideInsight> { new() { Text = "Lexical Resource is lowest at 6.0." } },
                NextSteps = new List<StudyGuideStep>
                {
                    new() { Title = "Raise Lexical Resource", Description = "Use collocations.", Focus = "lexical" },
                    new() { Title = "Fix Preposition", Description = "Drill prepositions.", Focus = "grammar" },
                    new() { Title = "Expand vocabulary", Description = "Master 10 words.", Focus = "vocabulary" }
                },
                RecommendedTopic = new StudyGuideTopic { Title = "Renewable energy", Reason = "Stretch.", SuggestedPrompt = "Discuss." }
            });

        var sut = Build(db, ai: ai);
        var dto = await sut.GenerateGuideAsync(UserA,
            new GenerateStudyGuideRequestDto { TargetExam = "IELTS Academic - Writing Task 2", TargetBand = 7m },
            "token");

        captured!.TargetExam.Should().Be("IELTS Academic - Writing Task 2");
        captured.TargetBand.Should().Be(7m);
        captured.EssaySummaries.Should().ContainSingle(e => e.OverallBand == 6.5m);
        captured.GrammarAggregates.Should().Contain(a => a.Category == "Preposition" && a.Count == 1);
        captured.Vocabulary.Should().ContainSingle(v => v.OriginalWord == "good");

        dto.Summary.Should().Be("Solid foundations.");
        dto.NextSteps.Should().HaveCount(3);
        dto.EstimatedBand.Should().Be(6.5m);
        dto.Strengths.Should().ContainSingle(s => s.Text == "Coherence 7.0 is your strongest criterion.");

        var stored = db.StudyGuides.Single();
        stored.UserId.Should().Be(UserA);
        stored.NextSteps.Should().HaveCount(3);
        stored.RecommendedTopic.Title.Should().Be("Renewable energy");
        stored.AnalyzedEssayCount.Should().Be(1);
    }

    [Fact]
    public async Task GenerateGuideAsync_Twice_KeepsOnlyLatestRow()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA);

        var sut = Build(db);
        await sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto(), null);
        await sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto(), null);

        db.StudyGuides.Should().HaveCount(1);
        db.StudyGuides.Single().Summary.Should().Be("Solid foundations with clear room to grow.");
    }

    [Fact]
    public async Task GenerateGuideAsync_AiUnavailable_UsesDeterministicFallback()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA, category: "Tense", sentence: "He go.", suggestion: "He goes.");

        var ai = Substitute.For<IStudyGuideAiProvider>();
        ai.GenerateGuideAsync(Arg.Any<StudyGuideGenerationRequest>())
            .Throws(new HttpRequestException("Gemini down", null, HttpStatusCode.ServiceUnavailable));

        var fallback = Substitute.For<IFallbackStudyGuideProvider>();
        fallback.GenerateGuide(Arg.Any<StudyGuideGenerationRequest>())
            .Returns(new StudyGuideResult
            {
                Summary = "Your recent essays average band 6.5.",
                EstimatedBand = 6.5m,
                Strengths = new List<StudyGuideInsight> { new() { Text = "Consistent submission habit." } },
                Weaknesses = new List<StudyGuideInsight> { new() { Text = "'Tense' is your most recurring grammar issue." } },
                NextSteps = new List<StudyGuideStep>
                {
                    new() { Title = "Fix 'Tense'", Description = "Correct every example.", Focus = "grammar" }
                },
                RecommendedTopic = new StudyGuideTopic { Title = "Opinion essay", Reason = "Stretch.", SuggestedPrompt = "Discuss." }
            });

        var sut = Build(db, ai: ai, fallback: fallback);
        var dto = await sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto(), null);

        fallback.Received(1).GenerateGuide(Arg.Any<StudyGuideGenerationRequest>());
        dto.Weaknesses.Should().Contain(w => w.Text.Contains("Tense"));
    }

    [Fact]
    public async Task GenerateGuideAsync_ExternalClientUnavailable_StillBuildsFromLocalWeakPoints()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA, category: "Preposition", sentence: "She good at.");
        SeedVocab(db, UserA, word: "bad", suggested: "adverse");

        var history = Substitute.For<IEssayHistoryClient>();
        history.GetGradingHistoryAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Throws(new HttpRequestException("grading down", null, HttpStatusCode.ServiceUnavailable));
        history.GetWrittenTopicsAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Throws(new TaskCanceledException());

        StudyGuideGenerationRequest? captured = null;
        var ai = Substitute.For<IStudyGuideAiProvider>();
        ai.GenerateGuideAsync(Arg.Do<StudyGuideGenerationRequest>(r => captured = r))
            .Returns(new StudyGuideResult
            {
                Summary = "Local-only guide.",
                EstimatedBand = 0,
                Strengths = new List<StudyGuideInsight> { new() { Text = "You build vocabulary." } },
                Weaknesses = new List<StudyGuideInsight> { new() { Text = "With local weak points." } },
                NextSteps = new List<StudyGuideStep> { new() { Title = "Practice", Description = ".", Focus = "grammar" } },
                RecommendedTopic = new StudyGuideTopic { Title = "Topic", Reason = ".", SuggestedPrompt = "." }
            });

        var sut = Build(db, history: history, ai: ai);
        var dto = await sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto(), null);

        captured!.EssaySummaries.Should().BeEmpty();
        captured.GrammarAggregates.Should().Contain(a => a.Category == "Preposition");
        dto.Summary.Should().Be("Local-only guide.");
    }

    [Fact]
    public async Task GenerateGuideAsync_EmptyNextStepsFromAi_DoesNotCrashAndPersistsDefaults()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA);

        var ai = Substitute.For<IStudyGuideAiProvider>();
        ai.GenerateGuideAsync(Arg.Any<StudyGuideGenerationRequest>())
            .Returns(new StudyGuideResult
            {
                Summary = "  ",
                EstimatedBand = 0,
                Strengths = new List<StudyGuideInsight>(),
                Weaknesses = new List<StudyGuideInsight>(),
                NextSteps = new List<StudyGuideStep>(),
                RecommendedTopic = new StudyGuideTopic()
            });

        var sut = Build(db, ai: ai);
        var dto = await sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto(), null);

        dto.Summary.Should().NotBeNullOrWhiteSpace();
        dto.EstimatedBand.Should().Be(6.5m);
        dto.Strengths.Should().BeEmpty();
        dto.RecommendedTopic.Title.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region U-L12 — cached read path + no cross-user leak

    [Fact]
    public async Task GetLatestGuideAsync_NoGuide_ReturnsNull()
    {
        var db = TestLearningDbContext.Create();

        var sut = Build(db);
        var result = await sut.GetLatestGuideAsync(UserA);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestGuideAsync_ReturnsOwnNewestGuideOnly()
    {
        var db = TestLearningDbContext.Create();
        var bGuide = SeedGuide(db, UserB);
        var ownOld = SeedGuide(db, UserA);
        ownOld.CreatedAt = DateTime.UtcNow.AddDays(-5);
        var ownNew = SeedGuide(db, UserA);
        ownNew.CreatedAt = DateTime.UtcNow.AddDays(-1);
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.GetLatestGuideAsync(UserA);

        result.Should().NotBeNull();
        result!.Id.Should().Be(ownNew.Id);
        result.Summary.Should().Be("Existing cached summary.");

        var other = await sut.GetLatestGuideAsync(UserB);
        other.Should().NotBeNull();
        other!.Id.Should().Be(bGuide.Id);
    }

    #endregion

    #region U-L13 — bilingual + actionable enrichment

    [Fact]
    public async Task GenerateGuideAsync_CarriesBilingualAndTopicScaffoldingIntoDto()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA);

        var ai = Substitute.For<IStudyGuideAiProvider>();
        ai.GenerateGuideAsync(Arg.Any<StudyGuideGenerationRequest>())
            .Returns(new StudyGuideResult
            {
                Summary = "Enriched.",
                EstimatedBand = 6.5m,
                Strengths = new List<StudyGuideInsight>
                {
                    new() { Text = "Coherence is strong at 7.0.", ExplanationVi = "Tính mạch lạc rất tốt." }
                },
                Weaknesses = new List<StudyGuideInsight>
                {
                    new() { Text = "Grammar range is limited.", ExplanationVi = "Cấu trúc ngữ pháp còn ít." }
                },
                NextSteps = new List<StudyGuideStep>
                {
                    new()
                    {
                        Title = "Rewrite intros",
                        Description = "Rewrite 3 intros with clear topic sentences.",
                        ExplanationVi = "Viết lại 3 phần mở bài theo câu chủ đề.",
                        Focus = "task_response",
                        ActionType = "write_essay",
                        ActionTarget = "/write?focus=task_response"
                    }
                },
                RecommendedTopic = new StudyGuideTopic
                {
                    Title = "Technology and stress",
                    Reason = "Exercises your weakest area.",
                    SuggestedPrompt = "Some people think technology makes life less stressful.",
                    IdeaHints = new List<string> { "Point 1: saves time", "Point 2: causes overload" },
                    KeyVocabulary = new List<string> { "information overload", "work-life balance" }
                }
            });

        var sut = Build(db, ai: ai);
        var dto = await sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto(), null);

        dto.Strengths.Single().Text.Should().Be("Coherence is strong at 7.0.");
        dto.Strengths.Single().ExplanationVi.Should().Be("Tính mạch lạc rất tốt.");
        dto.Weaknesses.Single().ExplanationVi.Should().NotBeNullOrWhiteSpace();

        var step = dto.NextSteps.Single();
        step.ExplanationVi.Should().Be("Viết lại 3 phần mở bài theo câu chủ đề.");
        step.ActionType.Should().Be("write_essay");
        step.ActionTarget.Should().Be("/write?focus=task_response");

        dto.RecommendedTopic.IdeaHints.Should().Contain("Point 1: saves time");
        dto.RecommendedTopic.KeyVocabulary.Should().Contain("information overload");
    }

    [Fact]
    public async Task GenerateGuideAsync_UnsafeStepActions_AreSanitizedOrDefaults()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA);

        var ai = Substitute.For<IStudyGuideAiProvider>();
        ai.GenerateGuideAsync(Arg.Any<StudyGuideGenerationRequest>())
            .Returns(new StudyGuideResult
            {
                Summary = "Actions.",
                EstimatedBand = 6.5m,
                Strengths = new List<StudyGuideInsight> { new() { Text = "S." } },
                Weaknesses = new List<StudyGuideInsight> { new() { Text = "W." } },
                NextSteps = new List<StudyGuideStep>
                {
                    new() { Title = "External link", Description = ".", Focus = "grammar", ActionType = "review_history", ActionTarget = "https://evil.example/x" },
                    new() { Title = "Unknown type", Description = ".", Focus = "grammar", ActionType = "hack", ActionTarget = "/write" },
                    new() { Title = "Direct vocab", Description = ".", Focus = "lexical", ActionType = "practice_vocabulary", ActionTarget = "/vocabulary" }
                },
                RecommendedTopic = new StudyGuideTopic { Title = "T", Reason = ".", SuggestedPrompt = "." }
            });

        var sut = Build(db, ai: ai);
        var dto = await sut.GenerateGuideAsync(UserA, new GenerateStudyGuideRequestDto(), null);

        var steps = dto.NextSteps;
        steps[0].ActionType.Should().Be("review_history");
        steps[0].ActionTarget.Should().Be("/history");
        steps[1].ActionType.Should().Be("none");
        steps[1].ActionTarget.Should().Be("/write");
        steps[2].ActionType.Should().Be("practice_vocabulary");
        steps[2].ActionTarget.Should().Be("/vocabulary");
    }

    #endregion
}
