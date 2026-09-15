using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Services;

public class DeterministicQuizProvider : IFallbackQuizProvider
{
    public List<QuizQuestionItem> GenerateQuestions(QuizGenerationRequest request, int targetCount)
    {
        var grammarErrors = (request.GrammarErrors ?? new List<GrammarErrorDto>())
            .Where(g => !string.IsNullOrWhiteSpace(g.Sentence) || !string.IsNullOrWhiteSpace(g.Suggestion))
            .ToList();

        var vocab = (request.Vocabulary ?? new List<VocabDto>())
            .Where(v => !string.IsNullOrWhiteSpace(v.OriginalWord) &&
                        !string.IsNullOrWhiteSpace(v.SuggestedWord))
            .ToList();

        if (grammarErrors.Count == 0 && vocab.Count == 0)
            return new List<QuizQuestionItem>();

        var questions = new List<QuizQuestionItem>();
        var answerBank = BuildAnswerBank(grammarErrors, vocab);
        var idCounter = 0;
        var useGrammarVariant = true;

        for (var i = 0; i < targetCount && (grammarErrors.Count > 0 || vocab.Count > 0); i++)
        {
            if (vocab.Count > 0)
            {
                var item = vocab[0];
                vocab.RemoveAt(0);

                if (useGrammarVariant)
                    questions.Add(BuildVocabMultipleChoice(item, answerBank, ++idCounter));
                else
                    questions.Add(BuildVocabFillBlank(item, ++idCounter));

                useGrammarVariant = !useGrammarVariant;
            }

            if (grammarErrors.Count > 0 && questions.Count < targetCount)
            {
                var error = grammarErrors[0];
                grammarErrors.RemoveAt(0);

                if (useGrammarVariant)
                    questions.Add(BuildGrammarFillBlank(error, ++idCounter));
                else
                    questions.Add(BuildGrammarRewrite(error, ++idCounter));

                useGrammarVariant = !useGrammarVariant;
            }
        }

        return questions.Take(targetCount).ToList();
    }

    private static QuizQuestionItem BuildVocabMultipleChoice(VocabDto item, List<string> answerBank, int id)
    {
        var correctAnswer = item.SuggestedWord.Split('/')[0].Trim();

        var distractors = answerBank
            .Where(w => !string.Equals(w, correctAnswer, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .ToList();

        var options = distractors
            .Concat(new[] { correctAnswer })
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        return new QuizQuestionItem
        {
            Id = $"q{id}",
            Category = "Vocabulary",
            Type = "multiple_choice",
            Question = $"Which word is the best replacement for underlined '{item.OriginalWord}'?",
            Sentence = item.ExampleSentence,
            Options = options,
            CorrectAnswer = correctAnswer,
            Explanation = $"Use '{correctAnswer}' instead of '{item.OriginalWord}'."
        };
    }

    private static QuizQuestionItem BuildVocabFillBlank(VocabDto item, int id)
    {
        var correctAnswer = item.SuggestedWord.Split('/')[0].Trim();

        var sentence = string.IsNullOrWhiteSpace(item.ExampleSentence)
            ? item.OriginalWord
            : BlankOut(item.ExampleSentence, item.OriginalWord);

        return new QuizQuestionItem
        {
            Id = $"q{id}",
            Category = "Vocabulary",
            Type = "fill_blank",
            Question = "Fill in the blank with the best word.",
            Sentence = sentence,
            Options = new List<string>(),
            CorrectAnswer = correctAnswer,
            Explanation = $"Prefer '{correctAnswer}' over '{item.OriginalWord}'."
        };
    }

    private static QuizQuestionItem BuildGrammarRewrite(GrammarErrorDto error, int id)
    {
        return new QuizQuestionItem
        {
            Id = $"q{id}",
            Category = error.GrammarCategory,
            Type = "rewrite",
            Question = "Rewrite the sentence fixing the grammar mistake.",
            Sentence = error.Sentence,
            Options = new List<string>(),
            CorrectAnswer = error.Suggestion,
            Explanation = error.Explanation
        };
    }

    private static QuizQuestionItem BuildGrammarFillBlank(GrammarErrorDto error, int id)
    {
        return new QuizQuestionItem
        {
            Id = $"q{id}",
            Category = error.GrammarCategory,
            Type = "fill_blank",
            Question = "Fill in the blank to correct the mistake.",
            Sentence = error.Sentence,
            Options = new List<string>(),
            CorrectAnswer = error.Suggestion,
            Explanation = error.Explanation
        };
    }

    private static List<string> BuildAnswerBank(List<GrammarErrorDto> grammarErrors, List<VocabDto> vocab)
    {
        var bank = new List<string>();

        foreach (var item in vocab)
        {
            foreach (var alternative in item.SuggestedWord.Split('/'))
            {
                var trimmed = alternative.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    bank.Add(trimmed);
            }
        }

        foreach (var error in grammarErrors)
        {
            if (!string.IsNullOrWhiteSpace(error.Suggestion))
                bank.Add(error.Suggestion);
        }

        return bank.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string BlankOut(string sentence, string word)
    {
        var markers = new[] { '\'', '"', '(', ')', '[', ']', '{', '}', '\u201C', '\u201D' };
        var candidate = word.Trim(markers);

        var index = sentence.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);

        if (index < 0)
            return $"{sentence} (blank: use the best word for '{word}')";

        var replacement = new string('_', Math.Max(candidate.Length, 4));

        return string.Concat(
            sentence.AsSpan(0, index),
            replacement,
            sentence.AsSpan(index + candidate.Length)).ToString();
    }
}