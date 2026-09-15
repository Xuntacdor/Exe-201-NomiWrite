using FluentAssertions;
using FluentValidation;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Exceptions;
using NomiWrite.Writing.Application.Services;
using NomiWrite.Writing.Application.UnitTests.Persistence;
using NomiWrite.Writing.Application.Validation;
using NomiWrite.Writing.Domain.Entities;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.UnitTests;

public class AdminPromptServiceTests
{
    private static AdminPromptService Build(TestWritingDbContext db)
        => new(db, new CreatePromptRequestValidator(), new UpdatePromptRequestValidator());

    private static WritingType SeedType(TestWritingDbContext db)
    {
        var type = new WritingType
        {
            Name = "IELTS Writing",
            Category = WritingTypeCategory.ExamFormat,
            Description = "exam",
            CreatedAt = DateTime.UtcNow
        };
        db.WritingTypes.Add(type);
        db.SaveChanges();
        return type;
    }

    private static CreatePromptRequestDto ValidCreate(Guid typeId) => new()
    {
        WritingTypeId = typeId,
        Title = "  Describe a chart  ",
        Instructions = "  Write about the chart.  ",
        Difficulty = DifficultyLevel.Intermediate,
        TimeLimitMinutes = 20,
        MinWords = 150,
        MaxWords = 250,
        ImageUrl = "  https://img.example.com/chart.png  ",
        SampleAnswer = "  A strong sample answer.  ",
        IsVipOnly = true
    };

    private static UpdatePromptRequestDto ValidUpdate(Guid typeId) => new()
    {
        WritingTypeId = typeId,
        Title = "  Describe a chart  ",
        Instructions = "  Write about the chart.  ",
        Difficulty = DifficultyLevel.Intermediate,
        TimeLimitMinutes = 20,
        MinWords = 150,
        MaxWords = 250,
        ImageUrl = "  https://img.example.com/chart.png  ",
        SampleAnswer = "  A strong sample answer.  ",
        IsVipOnly = true
    };

    #region U-W5 — Admin prompt CRUD validations

    [Fact]
    public async Task CreatePromptAsync_MinWordsGreaterThanMax_Throws()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var dto = ValidCreate(type.Id);
        dto.MinWords = 300;
        dto.MaxWords = 250;

        var sut = Build(db);
        var act = () => sut.CreatePromptAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*MinWords must be less than or equal to MaxWords*");
        db.WritingPrompts.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePromptAsync_Valid_TrimsAndPersistsVipFlagAndSample()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);

        var sut = Build(db);
        var result = await sut.CreatePromptAsync(ValidCreate(type.Id));

        result.Title.Should().Be("Describe a chart");
        result.IsVipOnly.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        result.MinWords.Should().Be(150);
        result.MaxWords.Should().Be(250);

        var stored = db.WritingPrompts.Single();
        stored.Title.Should().Be("Describe a chart");
        stored.SampleAnswer.Should().Be("A strong sample answer.");
        stored.IsVipOnly.Should().BeTrue();
        stored.TimeLimitMinutes.Should().Be(20);
    }

    [Fact]
    public async Task UpdatePromptAsync_UnknownPrompt_Throws()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);

        var sut = Build(db);
        var act = () => sut.UpdatePromptAsync(Guid.NewGuid(), ValidUpdate(type.Id));

        await act.Should().ThrowAsync<PromptNotFoundException>();
    }

    [Fact]
    public async Task UpdatePromptAsync_ChangesVipFlagAndSampleAnswer()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = new WritingPrompt
        {
            WritingTypeId = type.Id,
            Title = "Old title",
            Instructions = "Old instructions",
            Difficulty = DifficultyLevel.Beginner,
            IsActive = true,
            IsVipOnly = false,
            SampleAnswer = "Old sample",
            CreatedAt = DateTime.UtcNow
        };
        db.WritingPrompts.Add(prompt);
        db.SaveChanges();

        var dto = ValidUpdate(type.Id);
        dto.Title = "New title";
        dto.IsVipOnly = true;

        var sut = Build(db);
        var result = await sut.UpdatePromptAsync(prompt.Id, dto);

        result.Title.Should().Be("New title");
        result.IsVipOnly.Should().BeTrue();
        db.WritingPrompts.Single().IsVipOnly.Should().BeTrue();
        db.WritingPrompts.Single().Difficulty.Should().Be(DifficultyLevel.Intermediate);
    }

    [Fact]
    public async Task DeletePromptAsync_SoftDeletes()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        var prompt = new WritingPrompt
        {
            WritingTypeId = type.Id,
            Title = "Deletable",
            Instructions = "Instructions",
            Difficulty = DifficultyLevel.Beginner,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.WritingPrompts.Add(prompt);
        db.SaveChanges();

        var sut = Build(db);
        await sut.DeletePromptAsync(prompt.Id);

        db.WritingPrompts.Single(p => p.Id == prompt.Id).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeletePromptAsync_UnknownPrompt_Throws()
    {
        var db = TestWritingDbContext.Create();

        var sut = Build(db);
        var act = () => sut.DeletePromptAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<PromptNotFoundException>();
    }

    [Fact]
    public async Task GetPromptsAsync_FiltersAndPaginates()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        for (var i = 0; i < 5; i++)
        {
            db.WritingPrompts.Add(new WritingPrompt
            {
                WritingTypeId = type.Id,
                Title = $"Prompt {i}",
                Instructions = "Instructions",
                Difficulty = DifficultyLevel.Intermediate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        db.SaveChanges();

        var sut = Build(db);
        var page = await sut.GetPromptsAsync(type.Id, DifficultyLevel.Intermediate, true, 1, 2);

        page.TotalCount.Should().Be(5);
        page.TotalPages.Should().Be(3);
        page.Items.Should().HaveCount(2);
        page.Items.Should().OnlyContain(p => p.Difficulty == DifficultyLevel.Intermediate);
    }

    [Fact]
    public async Task GetPromptsAsync_DefaultsToActivePrompts()
    {
        var db = TestWritingDbContext.Create();
        var type = SeedType(db);
        db.WritingPrompts.Add(new WritingPrompt
        {
            WritingTypeId = type.Id, Title = "Active", Instructions = "I", Difficulty = DifficultyLevel.Beginner,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        db.WritingPrompts.Add(new WritingPrompt
        {
            WritingTypeId = type.Id, Title = "Inactive", Instructions = "I", Difficulty = DifficultyLevel.Beginner,
            IsActive = false, CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        var sut = Build(db);
        var page = await sut.GetPromptsAsync(null, null, null, 1, 10);

        page.Items.Should().ContainSingle(p => p.Title == "Active");
    }

    [Fact]
    public async Task GetPromptByIdAsync_UnknownPrompt_Throws()
    {
        var db = TestWritingDbContext.Create();

        var sut = Build(db);
        var act = () => sut.GetPromptByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<PromptNotFoundException>();
    }

    #endregion
}