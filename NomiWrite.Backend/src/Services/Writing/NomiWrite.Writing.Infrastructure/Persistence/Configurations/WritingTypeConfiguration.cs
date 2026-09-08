using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Writing.Domain.Entities;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Infrastructure.Persistence.Configurations;

public class WritingTypeConfiguration : IEntityTypeConfiguration<WritingType>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<WritingType> builder)
    {
        builder.ToTable("writing_types");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Category)
            .HasColumnName("category")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(t => t.Prompts)
            .WithOne(p => p.WritingType)
            .HasForeignKey(p => p.WritingTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new WritingType
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "IELTS Writing Task 1 Academic",
                Category = WritingTypeCategory.ExamFormat,
                Description = "Interpret and describe visual information (charts, graphs, tables, diagrams).",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "IELTS Writing Task 1 General Training",
                Category = WritingTypeCategory.ExamFormat,
                Description = "Write a letter in response to a given situation.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "IELTS Writing Task 2",
                Category = WritingTypeCategory.ExamFormat,
                Description = "Write an essay in response to a point of view, argument or problem.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Name = "TOEFL iBT Integrated Writing",
                Category = WritingTypeCategory.ExamFormat,
                Description = "Read a passage, listen to a lecture, then write a response that synthesizes both.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Name = "TOEFL iBT Independent Writing",
                Category = WritingTypeCategory.ExamFormat,
                Description = "Write an essay expressing an opinion on a familiar topic.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("66666666-6666-6666-6666-666666666666"),
                Name = "Cover Letter",
                Category = WritingTypeCategory.Professional,
                Description = "Write a professional cover letter applying for a job.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("77777777-7777-7777-7777-777777777777"),
                Name = "Business Email",
                Category = WritingTypeCategory.Professional,
                Description = "Write a clear and concise business email for a workplace scenario.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("88888888-8888-8888-8888-888888888888"),
                Name = "Meeting Minutes",
                Category = WritingTypeCategory.Professional,
                Description = "Record accurate and organized minutes for a meeting.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("99999999-9999-9999-9999-999999999999"),
                Name = "Paragraph Writing",
                Category = WritingTypeCategory.Academic,
                Description = "Write a well-structured paragraph on an academic topic.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new WritingType
            {
                Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Personal Statement / SOP",
                Category = WritingTypeCategory.Academic,
                Description = "Write a personal statement or statement of purpose for university applications.",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            });
    }
}