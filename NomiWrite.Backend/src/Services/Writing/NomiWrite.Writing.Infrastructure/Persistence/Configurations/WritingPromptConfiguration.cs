using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Writing.Domain.Entities;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Infrastructure.Persistence.Configurations;

public class WritingPromptConfiguration : IEntityTypeConfiguration<WritingPrompt>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 9, 13, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<WritingPrompt> builder)
    {
        builder.ToTable("writing_prompts");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(p => p.WritingTypeId)
            .HasColumnName("writing_type_id")
            .IsRequired();

        builder.HasIndex(p => p.WritingTypeId)
            .HasDatabaseName("ix_writing_prompts_writing_type_id");

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Instructions)
            .HasColumnName("instructions")
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(p => p.Difficulty)
            .HasColumnName("difficulty")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.TimeLimitMinutes)
            .HasColumnName("time_limit_minutes");

        builder.Property(p => p.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(2048);

        builder.Property(p => p.SampleAnswer)
            .HasColumnName("sample_answer")
            .HasMaxLength(20000);

        builder.Property(p => p.MinWords)
            .HasColumnName("min_words");

        builder.Property(p => p.MaxWords)
            .HasColumnName("max_words");

        builder.Property(p => p.IsVipOnly)
            .HasColumnName("is_vip_only")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(p => p.WritingType)
            .WithMany(t => t.Prompts)
            .HasForeignKey(p => p.WritingTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Submissions)
            .WithOne(s => s.WritingPrompt)
            .HasForeignKey(s => s.WritingPromptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000001"),
                WritingTypeId = new Guid("11111111-1111-1111-1111-111111111111"),
                Title = "Urban Transport Modes",
                Instructions = "The chart compares how commuters travel in three cities. Summarize the main features and make comparisons where relevant.",
                Difficulty = DifficultyLevel.Intermediate,
                IsActive = true,
                TimeLimitMinutes = 20,
                SampleAnswer = "The chart compares commuter transport choices across three cities. Overall, public transport is the most common option in the largest city, while private cars dominate in the suburban city. Cycling remains the least used mode in all three locations.",
                MinWords = 150,
                MaxWords = 220,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000002"),
                WritingTypeId = new Guid("22222222-2222-2222-2222-222222222222"),
                Title = "Request A Course Refund",
                Instructions = "You recently enrolled in an online course but cannot continue. Write a letter to the course provider explaining the situation and requesting a refund.",
                Difficulty = DifficultyLevel.Beginner,
                IsActive = true,
                TimeLimitMinutes = 20,
                SampleAnswer = "Dear Sir or Madam, I am writing to request a refund for the online course I purchased last week. Unfortunately, my work schedule has changed and I can no longer attend the live sessions.",
                MinWords = 150,
                MaxWords = 220,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000003"),
                WritingTypeId = new Guid("33333333-3333-3333-3333-333333333333"),
                Title = "Remote Work And Productivity",
                Instructions = "Some people believe remote work improves productivity, while others think employees work better in offices. Discuss both views and give your own opinion.",
                Difficulty = DifficultyLevel.Intermediate,
                IsActive = true,
                TimeLimitMinutes = 40,
                SampleAnswer = "Remote work can improve productivity by reducing commuting time and allowing employees to focus in a comfortable environment. However, offices still provide faster collaboration and clearer team routines.",
                MinWords = 250,
                MaxWords = 380,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000004"),
                WritingTypeId = new Guid("44444444-4444-4444-4444-444444444444"),
                Title = "Campus Library Policy",
                Instructions = "Summarize the relationship between a reading passage about extended library hours and a lecture that challenges the policy.",
                Difficulty = DifficultyLevel.Advanced,
                IsActive = true,
                TimeLimitMinutes = 20,
                SampleAnswer = "The reading supports extending library hours because students need quiet study space at night. The lecture disagrees, arguing that staffing costs are too high and existing evening usage is limited.",
                MinWords = 150,
                MaxWords = 225,
                IsVipOnly = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000005"),
                WritingTypeId = new Guid("55555555-5555-5555-5555-555555555555"),
                Title = "Learning Through Mistakes",
                Instructions = "Do you agree or disagree that people learn more from mistakes than from success? Use specific reasons and examples.",
                Difficulty = DifficultyLevel.Intermediate,
                IsActive = true,
                TimeLimitMinutes = 30,
                SampleAnswer = "I agree that mistakes often teach people more than success because failure forces reflection. When a project succeeds easily, people may not understand which choices mattered.",
                MinWords = 300,
                MaxWords = 450,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000006"),
                WritingTypeId = new Guid("66666666-6666-6666-6666-666666666666"),
                Title = "Junior Marketing Associate Cover Letter",
                Instructions = "Write a cover letter for a junior marketing associate role, emphasizing communication, campaign support, and willingness to learn.",
                Difficulty = DifficultyLevel.Beginner,
                IsActive = true,
                TimeLimitMinutes = 30,
                SampleAnswer = "Dear Hiring Manager, I am excited to apply for the Junior Marketing Associate position. My academic projects and internship experience have helped me build strong communication and campaign coordination skills.",
                MinWords = 180,
                MaxWords = 320,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000007"),
                WritingTypeId = new Guid("77777777-7777-7777-7777-777777777777"),
                Title = "Project Deadline Update",
                Instructions = "Write a professional email informing a client that a project milestone will be delayed by three days and proposing a revised timeline.",
                Difficulty = DifficultyLevel.Beginner,
                IsActive = true,
                TimeLimitMinutes = 15,
                SampleAnswer = "Dear Client, I am writing to update you on the current project milestone. We need three additional days to complete final quality checks, and I propose delivering the revised milestone on Friday.",
                MinWords = 120,
                MaxWords = 220,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000008"),
                WritingTypeId = new Guid("88888888-8888-8888-8888-888888888888"),
                Title = "Product Launch Meeting Minutes",
                Instructions = "Write concise meeting minutes from notes about a product launch meeting, including attendees, decisions, action items, and deadlines.",
                Difficulty = DifficultyLevel.Intermediate,
                IsActive = true,
                TimeLimitMinutes = 25,
                SampleAnswer = "Meeting minutes should identify the meeting purpose, attendees, key decisions, and assigned action items. The product launch date was confirmed, while marketing assets and QA checks were assigned to separate owners.",
                MinWords = 180,
                MaxWords = 320,
                IsVipOnly = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000009"),
                WritingTypeId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Title = "Digital Textbooks In Universities",
                Instructions = "Summarize a passage about the shift from printed textbooks to digital textbooks in one sentence.",
                Difficulty = DifficultyLevel.Intermediate,
                IsActive = true,
                TimeLimitMinutes = 10,
                SampleAnswer = "Universities are increasingly adopting digital textbooks because they reduce costs, improve accessibility, and allow faster updates, although some students still prefer printed materials for focused reading.",
                MinWords = 5,
                MaxWords = 75,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000010"),
                WritingTypeId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Title = "School Trips And Learning",
                Instructions = "Write an essay discussing whether schools should organize more educational trips for students.",
                Difficulty = DifficultyLevel.Intermediate,
                IsActive = true,
                TimeLimitMinutes = 40,
                SampleAnswer = "Educational trips should be used more often because they connect classroom knowledge with real experiences. However, schools must plan them carefully so they remain affordable and relevant.",
                MinWords = 140,
                MaxWords = 220,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000011"),
                WritingTypeId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Title = "Public Parks In Cities",
                Instructions = "Some people think cities should spend more money on public parks. To what extent do you agree or disagree?",
                Difficulty = DifficultyLevel.Intermediate,
                IsActive = true,
                TimeLimitMinutes = 40,
                SampleAnswer = "I largely agree that cities should invest more in public parks because they improve public health, provide social spaces, and make dense urban areas more livable.",
                MinWords = 250,
                MaxWords = 380,
                IsVipOnly = false,
                CreatedAt = SeedCreatedAt
            },
            new WritingPrompt
            {
                Id = new Guid("10000000-0000-0000-0000-000000000012"),
                WritingTypeId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Title = "Abstract For A Study On Mobile Learning",
                Instructions = "Write a research abstract for a small study investigating how mobile learning apps affect vocabulary retention among university students.",
                Difficulty = DifficultyLevel.Advanced,
                IsActive = true,
                TimeLimitMinutes = 30,
                SampleAnswer = "This study investigates the effect of mobile learning applications on vocabulary retention among university students. Using pre- and post-tests, it compares app-supported practice with conventional review.",
                MinWords = 150,
                MaxWords = 250,
                IsVipOnly = true,
                CreatedAt = SeedCreatedAt
            });
    }
}
