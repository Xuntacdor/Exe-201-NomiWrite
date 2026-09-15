using FluentAssertions;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Domain.Entities;
using NomiWrite.Learning.Infrastructure.Services;

namespace NomiWrite.Learning.Application.UnitTests;

public class DeterministicQuizProviderTests
{
    #region U-L9 — deterministic fallback provider

    [Fact]
    public void GenerateQuestions_EmptySources_ReturnsEmptyList()
    {
        var sut = new DeterministicQuizProvider();

        var result = sut.GenerateQuestions(new QuizGenerationRequest(), 5);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateQuestions_MultipleChoiceOptions_AreDistinctAndIncludeCorrect()
    {
        var sut = new DeterministicQuizProvider();

        var result = sut.GenerateQuestions(new QuizGenerationRequest
        {
            Vocabulary = new List<VocabDto>
            {
                NewVocab("good", "beneficial / advantageous", "The good habit pays off."),
                NewVocab("bad", "harmful / detrimental", "The bad outcome."),
                NewVocab("big", "substantial / considerable", "The big change."),
                NewVocab("many", "numerous / plenty", "The many options.")
            }
        }, 4);

        var mcq = result.First(q => q.Type == "multiple_choice");
        mcq.Options.Should().HaveCountGreaterThanOrEqualTo(2);
        mcq.Options.Should().OnlyHaveUniqueItems();
        mcq.Options.Should().Contain(mcq.CorrectAnswer);
        mcq.Options.Should().NotContain("beneficial / advantageous");
    }

    [Fact]
    public void GenerateQuestions_FillBlank_BlanksOutOriginalWord()
    {
        var sut = new DeterministicQuizProvider();

        // Two vocab items: the first becomes a multiple-choice, the second a fill_blank.
        var result = sut.GenerateQuestions(new QuizGenerationRequest
        {
            Vocabulary = new List<VocabDto>
            {
                NewVocab("good", "beneficial", "The good habit pays off."),
                NewVocab("bad", "harmful", "The bad outcome.")
            }
        }, 2);

        var fillBlank = result.First(q => q.Type == "fill_blank");
        fillBlank.Sentence.Should().NotContain("bad");
        fillBlank.Sentence.Should().Contain("____");
        fillBlank.CorrectAnswer.Should().Be("harmful");
    }

    [Fact]
    public void GenerateQuestions_GrammarRewrite_PreservesCategoryAndSuggestion()
    {
        var sut = new DeterministicQuizProvider();

        // Two grammar errors: the first becomes a fill_blank, the second a rewrite.
        var result = sut.GenerateQuestions(new QuizGenerationRequest
        {
            GrammarErrors = new List<GrammarErrorDto>
            {
                new()
                {
                    GrammarCategory = "Chia thì động từ",
                    Sentence = "He go to school.",
                    Suggestion = "He goes to school.",
                    Explanation = "Fix the verb form."
                },
                new()
                {
                    GrammarCategory = "Giới từ",
                    Sentence = "She is good at.",
                    Suggestion = "She is good at English.",
                    Explanation = "Add the object."
                }
            }
        }, 2);

        var rewrite = result.Last(q => q.Type == "rewrite");
        rewrite.Type.Should().Be("rewrite");
        rewrite.Category.Should().Be("Giới từ");
        rewrite.CorrectAnswer.Should().Be("She is good at English.");
        rewrite.Sentence.Should().Be("She is good at.");
    }

    [Fact]
    public void GenerateQuestions_CorrectAnswers_NeverContainSlashDelimiter()
    {
        var sut = new DeterministicQuizProvider();

        var result = sut.GenerateQuestions(new QuizGenerationRequest
        {
            Vocabulary = new List<VocabDto>
            {
                NewVocab("good", "beneficial / advantageous", "The good habit pays off."),
                NewVocab("bad", "harmful / detrimental", "The bad outcome.")
            }
        }, 4);

        result.Should().OnlyContain(q => !q.CorrectAnswer.Contains('/'));
    }

    private static VocabDto NewVocab(string original, string suggested, string example) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        SubmissionId = null,
        Topic = "Vocabulary",
        OriginalWord = original,
        SuggestedWord = suggested,
        ExampleSentence = example
    };

    #endregion
}