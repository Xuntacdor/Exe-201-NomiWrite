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
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.UnitTests;

public class QuizServiceTests
{
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private static QuizService Build(TestLearningDbContext db, IAiQuizProvider? ai = null,
        IFallbackQuizProvider? fallback = null)
    {
        ai ??= StubAi();
        fallback ??= StubFallback();

        return new QuizService(db, ai, fallback, new Valid<SubmitQuizAttemptRequestDto>(),
            NullLogger<QuizService>.Instance);
    }

    private static IAiQuizProvider StubAi()
    {
        var ai = Substitute.For<IAiQuizProvider>();
        ai.GenerateQuestionsAsync(Arg.Any<QuizGenerationRequest>(), Arg.Any<int>())
            .Returns(new List<QuizQuestionItem>
            {
                new() { Id = "ai1", Category = "Grammar", Type = "multiple_choice", Question = "AI Q?", CorrectAnswer = "a" }
            });
        return ai;
    }

    private static IFallbackQuizProvider StubFallback()
    {
        var fallback = Substitute.For<IFallbackQuizProvider>();
        fallback.GenerateQuestions(Arg.Any<QuizGenerationRequest>(), Arg.Any<int>())
            .Returns(new List<QuizQuestionItem>
            {
                new() { Id = "fb1", Category = "Grammar", Type = "multiple_choice", Question = "FB Q?", CorrectAnswer = "b" }
            });
        return fallback;
    }

    private static Quiz SeedQuiz(TestLearningDbContext db, Guid userId,
        List<QuizQuestionItem>? questions = null)
    {
        var quiz = new Quiz
        {
            UserId = userId,
            Category = "Grammar",
            Questions = questions ?? new List<QuizQuestionItem>
            {
                new() { Id = "q1", Category = "Grammar", Type = "multiple_choice", Question = "Choose the best form.",
                    Options = new List<string> { "go", "goes", "went" }, CorrectAnswer = "goes", Explanation = "SVA." },
                new() { Id = "q2", Category = "Vocabulary", Type = "fill_blank",
                    Question = "Fill the blank.", Sentence = "The ____ is important.", CorrectAnswer = "benefit", Explanation = "." },
                new() { Id = "q3", Category = "Grammar", Type = "rewrite",
                    Question = "Rewrite the sentence.", Sentence = "He go to school.", CorrectAnswer = "He goes to school.", Explanation = "." }
            },
            CreatedAt = DateTime.UtcNow
        };
        db.Quizzes.Add(quiz);
        db.SaveChanges();
        return quiz;
    }

    private static GrammarError SeedGrammarError(TestLearningDbContext db, Guid userId, Guid submissionId,
        string category = "Tense", string sentence = "He go.", string suggestion = "He goes.")
    {
        var error = new GrammarError
        {
            UserId = userId,
            SubmissionId = submissionId,
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

    private static VocabSuggestion SeedVocab(TestLearningDbContext db, Guid userId, Guid? submissionId,
        string word = "good", string suggested = "beneficial")
    {
        var vocab = new VocabSuggestion
        {
            UserId = userId,
            SubmissionId = submissionId,
            Topic = "Vocabulary",
            OriginalWord = word,
            SuggestedWord = suggested,
            ExampleSentence = $"The {word} habit.",
            CreatedAt = DateTime.UtcNow
        };
        db.VocabSuggestions.Add(vocab);
        db.SaveChanges();
        return vocab;
    }

    #region U-L2 / U-L7 — quiz get + answer-key leakage

    [Fact]
    public async Task ListQuizzesAsync_ReturnsOwnQuizzesWithLatestAttemptSummary()
    {
        var db = TestLearningDbContext.Create();
        var ownOld = SeedQuiz(db, UserA);
        ownOld.CreatedAt = DateTime.UtcNow.AddDays(-2);
        var ownNew = SeedQuiz(db, UserA);
        ownNew.CreatedAt = DateTime.UtcNow.AddDays(-1);
        SeedQuiz(db, UserB);

        db.QuizAttempts.Add(new QuizAttempt
        {
            QuizId = ownNew.Id,
            UserId = UserA,
            Score = 1,
            TotalQuestions = 3,
            AttemptedAt = DateTime.UtcNow.AddHours(-2)
        });
        db.QuizAttempts.Add(new QuizAttempt
        {
            QuizId = ownNew.Id,
            UserId = UserA,
            Score = 2,
            TotalQuestions = 3,
            AttemptedAt = DateTime.UtcNow.AddHours(-1)
        });
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.ListQuizzesAsync(UserA);

        result.Should().HaveCount(2);
        result.First().Id.Should().Be(ownNew.Id);
        result.First().AttemptCount.Should().Be(2);
        result.First().LatestScore.Should().Be(2);
        result.First().LatestTotalQuestions.Should().Be(3);
        result.Should().NotContain(q => q.UserId == UserB);
    }

    [Fact]
    public async Task GetQuizAsync_OtherUserQuiz_ThrowsForbidden()
    {
        var db = TestLearningDbContext.Create();
        var quiz = SeedQuiz(db, UserB);

        var sut = Build(db);
        var act = () => sut.GetQuizAsync(UserA, quiz.Id);

        await act.Should().ThrowAsync<ForbiddenLearningAccessException>();
    }

    [Fact]
    public async Task GetQuizAsync_UnknownQuiz_ThrowsNotFound()
    {
        var db = TestLearningDbContext.Create();

        var sut = Build(db);
        var act = () => sut.GetQuizAsync(UserA, Guid.NewGuid());

        await act.Should().ThrowAsync<QuizNotFoundException>();
    }

    [Fact]
    public async Task GetQuizAsync_BeforeAttempt_OmitsAnswersAndExplanations()
    {
        var db = TestLearningDbContext.Create();
        var quiz = SeedQuiz(db, UserA);

        var sut = Build(db);
        var detail = await sut.GetQuizAsync(UserA, quiz.Id);

        detail.IsCompleted.Should().BeFalse();
        detail.CorrectAnswers.Should().BeNull();
        detail.Explanations.Should().BeNull();
        detail.Questions.Should().OnlyContain(q =>
            string.IsNullOrWhiteSpace(q.Question) == false);
    }

    [Fact]
    public async Task GetQuizAsync_AfterAttempt_ReturnsAnswersAndExplanations()
    {
        var db = TestLearningDbContext.Create();
        var quiz = SeedQuiz(db, UserA);
        db.QuizAttempts.Add(new QuizAttempt { QuizId = quiz.Id, UserId = UserA, Score = 1, TotalQuestions = 3, AttemptedAt = DateTime.UtcNow });
        db.SaveChanges();

        var sut = Build(db);
        var detail = await sut.GetQuizAsync(UserA, quiz.Id);

        detail.IsCompleted.Should().BeTrue();
        detail.CorrectAnswers.Should().ContainKey("q1").WhoseValue.Should().Be("goes");
        detail.CorrectAnswers.Should().ContainKey("q3");
        detail.Explanations.Should().ContainKey("q1").WhoseValue.Should().Be("SVA.");
    }

    #endregion

    #region U-L3 / U-L6 — attempt IDOR + grading

    [Fact]
    public async Task SubmitAttemptAsync_OtherUserQuiz_ThrowsForbidden()
    {
        var db = TestLearningDbContext.Create();
        var quiz = SeedQuiz(db, UserB);

        var sut = Build(db);
        var act = () => sut.SubmitAttemptAsync(UserA, new SubmitQuizAttemptRequestDto
        {
            QuizId = quiz.Id,
            Answers = new Dictionary<string, string> { ["q1"] = "goes" }
        });

        await act.Should().ThrowAsync<ForbiddenLearningAccessException>();
        db.QuizAttempts.Should().BeEmpty();
    }

    [Fact]
    public async Task SubmitAttemptAsync_UnknownQuiz_ThrowsNotFound()
    {
        var db = TestLearningDbContext.Create();

        var sut = Build(db);
        var act = () => sut.SubmitAttemptAsync(UserA, new SubmitQuizAttemptRequestDto
        {
            QuizId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<QuizNotFoundException>();
    }

    [Fact]
    public async Task SubmitAttemptAsync_GradesCaseInsensitiveAndPersistsNormalizedAnswers()
    {
        var db = TestLearningDbContext.Create();
        var quiz = SeedQuiz(db, UserA);

        var sut = Build(db);
        var result = await sut.SubmitAttemptAsync(UserA, new SubmitQuizAttemptRequestDto
        {
            QuizId = quiz.Id,
            Answers = new Dictionary<string, string>
            {
                ["q1"] = "  Goes  ",
                ["q2"] = "",
                ["q3"] = "He goes to school."
            }
        });

        result.Score.Should().Be(2);
        result.TotalQuestions.Should().Be(3);

        var breakdown = result.QuestionBreakdown.ToDictionary(b => b.QuestionId);
        breakdown["q1"].IsCorrect.Should().BeTrue();
        breakdown["q1"].UserAnswer.Should().Be("Goes");
        breakdown["q2"].IsCorrect.Should().BeFalse();
        breakdown["q2"].UserAnswer.Should().BeEmpty();
        breakdown["q3"].IsCorrect.Should().BeTrue();
        breakdown["q3"].UserAnswer.Should().Be("He goes to school.");

        var persisted = db.QuizAttempts.Single();
        persisted.Score.Should().Be(2);
        persisted.TotalQuestions.Should().Be(3);
        persisted.Answers["q1"].Should().Be("Goes");
    }

    [Fact]
    public async Task SubmitAttemptAsync_MissingAndCaseMismatchedAnswers_AreIncorrect()
    {
        var db = TestLearningDbContext.Create();
        var quiz = SeedQuiz(db, UserA);

        var sut = Build(db);
        var result = await sut.SubmitAttemptAsync(UserA, new SubmitQuizAttemptRequestDto
        {
            QuizId = quiz.Id,
            Answers = new Dictionary<string, string>
            {
                ["q2"] = "BENEFIT"
            }
        });

        result.Score.Should().Be(1);
        result.QuestionBreakdown.Single(b => b.QuestionId == "q1").IsCorrect.Should().BeFalse();
        result.QuestionBreakdown.Single(b => b.QuestionId == "q2").IsCorrect.Should().BeTrue();
    }

    #endregion

    #region U-L4 / U-L5 — quiz generation sources + no data leak

    [Fact]
    public async Task GenerateQuizAsync_SubmissionScopedMissingSources_Throws()
    {
        var db = TestLearningDbContext.Create();
        var submissionId = Guid.NewGuid();

        var sut = Build(db);
        var act = () => sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto { SubmissionId = submissionId });

        await act.Should().ThrowAsync<QuizGenerationSourceNotFoundException>();
    }

    [Fact]
    public async Task GenerateQuizAsync_GeneralMissingSources_Throws()
    {
        var db = TestLearningDbContext.Create();

        var sut = Build(db);
        var act = () => sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto());

        await act.Should().ThrowAsync<QuizGenerationSourceNotFoundException>();
    }

    [Fact]
    public async Task GenerateQuizAsync_SubmissionScopedMissingSources_FallsBackToRecentOwnWeakPoints()
    {
        var db = TestLearningDbContext.Create();
        var emptySubmission = Guid.NewGuid();
        var recentSubmission = Guid.NewGuid();
        SeedGrammarError(db, UserA, recentSubmission, category: "Tense", sentence: "He go.");
        SeedGrammarError(db, UserB, Guid.NewGuid(), category: "Preposition", sentence: "She good at.");

        QuizGenerationRequest? captured = null;
        var ai = Substitute.For<IAiQuizProvider>();
        ai.GenerateQuestionsAsync(Arg.Do<QuizGenerationRequest>(r => captured = r), Arg.Any<int>())
            .Returns(new List<QuizQuestionItem>
            {
                new() { Id = "x", Category = "Grammar", Type = "multiple_choice", Question = "Q?", CorrectAnswer = "a" }
            });

        var sut = Build(db, ai: ai);
        var quiz = await sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto { SubmissionId = emptySubmission });

        quiz.SourceSubmissionId.Should().Be(emptySubmission);
        captured!.GrammarErrors.Should().ContainSingle(e => e.SubmissionId == recentSubmission);
        captured.GrammarErrors.Should().NotContain(e => e.UserId == UserB);
    }

    [Fact]
    public async Task GenerateQuizAsync_OnlyAnotherUsersVocabIds_ThrowsNoLeak()
    {
        var db = TestLearningDbContext.Create();
        var bVocab = SeedVocab(db, UserB, submissionId: null);

        var sut = Build(db);
        var act = () => sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto
        {
            VocabularyIds = new List<Guid> { bVocab.Id }
        });

        await act.Should().ThrowAsync<QuizGenerationSourceNotFoundException>();
    }

    [Fact]
    public async Task GenerateQuizAsync_SubmissionScoped_UsesOnlyThatSubmissionsErrors()
    {
        var db = TestLearningDbContext.Create();
        var sub1 = Guid.NewGuid();
        var sub2 = Guid.NewGuid();
        SeedGrammarError(db, UserA, sub1, category: "Tense", sentence: "He go.");
        SeedGrammarError(db, UserA, sub2, category: "Preposition", sentence: "She good at.");

        QuizGenerationRequest? captured = null;
        var ai = Substitute.For<IAiQuizProvider>();
        ai.GenerateQuestionsAsync(Arg.Do<QuizGenerationRequest>(r => captured = r), Arg.Any<int>())
            .Returns(new List<QuizQuestionItem>
            {
                new() { Id = "x", Category = "Grammar", Type = "multiple_choice", Question = "Q?", CorrectAnswer = "a" }
            });

        var sut = Build(db, ai: ai);
        await sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto { SubmissionId = sub1 });

        captured!.GrammarErrors.Should().ContainSingle();
        captured.GrammarErrors.Single().SubmissionId.Should().Be(sub1);
        captured.GrammarErrors.Single().GrammarCategory.Should().Be("Tense");
    }

    [Fact]
    public async Task GenerateQuizAsync_VocabIdScoped_UsesOnlyRequestedIds()
    {
        var db = TestLearningDbContext.Create();
        var wanted = SeedVocab(db, UserA, submissionId: null, word: "good", suggested: "beneficial");
        SeedVocab(db, UserA, submissionId: null, word: "bad", suggested: "adverse");

        QuizGenerationRequest? captured = null;
        var ai = Substitute.For<IAiQuizProvider>();
        ai.GenerateQuestionsAsync(Arg.Do<QuizGenerationRequest>(r => captured = r), Arg.Any<int>())
            .Returns(new List<QuizQuestionItem>
            {
                new() { Id = "x", Category = "Grammar", Type = "multiple_choice", Question = "Q?", CorrectAnswer = "a" }
            });

        var sut = Build(db, ai: ai);
        await sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto
        {
            VocabularyIds = new List<Guid> { wanted.Id }
        });

        captured!.Vocabulary.Should().ContainSingle(v => v.Id == wanted.Id);
    }

    [Fact]
    public async Task GenerateQuizAsync_SubmissionScopedCategories_FiltersGrammarSources()
    {
        var db = TestLearningDbContext.Create();
        var submissionId = Guid.NewGuid();
        SeedGrammarError(db, UserA, submissionId, category: "Tense", sentence: "He go.");
        SeedGrammarError(db, UserA, submissionId, category: "Preposition", sentence: "She good at.");

        QuizGenerationRequest? captured = null;
        var ai = Substitute.For<IAiQuizProvider>();
        ai.GenerateQuestionsAsync(Arg.Do<QuizGenerationRequest>(r => captured = r), Arg.Any<int>())
            .Returns(new List<QuizQuestionItem>
            {
                new() { Id = "x", Category = "Grammar", Type = "multiple_choice", Question = "Q?", CorrectAnswer = "a" }
            });

        var sut = Build(db, ai: ai);
        await sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto
        {
            SubmissionId = submissionId,
            Categories = new List<string> { "Tense" }
        });

        captured!.GrammarErrors.Should().ContainSingle();
        captured.GrammarErrors.Single().GrammarCategory.Should().Be("Tense");
    }

    [Fact]
    public async Task GenerateQuizAsync_SubmissionScopedMissingSources_UsesRequestedVocabularyIds()
    {
        var db = TestLearningDbContext.Create();
        var emptySubmission = Guid.NewGuid();
        var wanted = SeedVocab(db, UserA, submissionId: null, word: "good", suggested: "beneficial");
        SeedVocab(db, UserA, submissionId: null, word: "bad", suggested: "adverse");

        QuizGenerationRequest? captured = null;
        var ai = Substitute.For<IAiQuizProvider>();
        ai.GenerateQuestionsAsync(Arg.Do<QuizGenerationRequest>(r => captured = r), Arg.Any<int>())
            .Returns(new List<QuizQuestionItem>
            {
                new() { Id = "x", Category = "Vocabulary", Type = "multiple_choice", Question = "Q?", CorrectAnswer = "a" }
            });

        var sut = Build(db, ai: ai);
        await sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto
        {
            SubmissionId = emptySubmission,
            VocabularyIds = new List<Guid> { wanted.Id }
        });

        captured!.Vocabulary.Should().ContainSingle(v => v.Id == wanted.Id);
    }

    #endregion

    #region U-L9 — normalization caps at 10, dedupe types

    [Fact]
    public async Task GenerateQuizAsync_NormalizesAiOutput_CapsAtTenAndFiltersEmpty()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA, Guid.NewGuid(), sentence: "He go.", suggestion: "He goes.");

        var aiQuestions = new List<QuizQuestionItem>();
        for (var i = 0; i < 12; i++)
        {
            aiQuestions.Add(new QuizQuestionItem
            {
                Id = string.Empty,
                Category = string.Empty,
                Type = i == 0 ? "essay" : "multiple_choice",
                Question = $"Question {i}?",
                CorrectAnswer = "ans",
                Explanation = "e"
            });
        }
        aiQuestions.Add(new QuizQuestionItem { Id = "blank", Question = "  " });

        var ai = Substitute.For<IAiQuizProvider>();
        ai.GenerateQuestionsAsync(Arg.Any<QuizGenerationRequest>(), Arg.Any<int>()).Returns(aiQuestions);

        var sut = Build(db, ai: ai);
        var quizDto = await sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto());

        var stored = db.Quizzes.Single();
        stored.Questions.Should().HaveCount(10);
        stored.Questions.Should().OnlyContain(q => !string.IsNullOrWhiteSpace(q.Question));
        stored.Questions[0].Type.Should().Be("multiple_choice");
        stored.Questions[0].Category.Should().Be("General");
        stored.Questions.Should().OnlyContain(q => !string.IsNullOrWhiteSpace(q.Id));
        quizDto.Questions.Should().HaveCount(10);
    }

    [Fact]
    public async Task GenerateQuizAsync_AiUnavailable_UsesDeterministicFallback()
    {
        var db = TestLearningDbContext.Create();
        SeedGrammarError(db, UserA, Guid.NewGuid(), sentence: "He go.", suggestion: "He goes.");

        var ai = Substitute.For<IAiQuizProvider>();
        ai.GenerateQuestionsAsync(Arg.Any<QuizGenerationRequest>(), Arg.Any<int>())
            .Throws(new HttpRequestException("Gemini down", null, HttpStatusCode.ServiceUnavailable));

        var fallback = Substitute.For<IFallbackQuizProvider>();
        fallback.GenerateQuestions(Arg.Any<QuizGenerationRequest>(), Arg.Any<int>())
            .Returns(new List<QuizQuestionItem>
            {
                new() { Id = "fb", Category = "Grammar", Type = "rewrite",
                    Question = "Rewrite.", Sentence = "He go.", CorrectAnswer = "He goes.", Explanation = "." }
            });

        var sut = Build(db, ai: ai, fallback: fallback);
        var quizDto = await sut.GenerateQuizAsync(UserA, new GenerateQuizRequestDto());

        fallback.Received(1).GenerateQuestions(Arg.Any<QuizGenerationRequest>(), Arg.Any<int>());
        db.Quizzes.Single().Questions.Single().Id.Should().Be("fb");
        quizDto.Questions.Should().ContainSingle();
    }

    #endregion

    private sealed class Valid<T> : AbstractValidator<T>
    {
    }
}
