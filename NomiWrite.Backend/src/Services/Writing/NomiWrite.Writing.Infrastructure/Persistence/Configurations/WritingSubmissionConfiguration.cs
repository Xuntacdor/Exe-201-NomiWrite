using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Writing.Domain.Entities;

namespace NomiWrite.Writing.Infrastructure.Persistence.Configurations;

public class WritingSubmissionConfiguration : IEntityTypeConfiguration<WritingSubmission>
{
    public void Configure(EntityTypeBuilder<WritingSubmission> builder)
    {
        builder.ToTable("writing_submissions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("ix_writing_submissions_user_id");

        builder.Property(s => s.WritingPromptId)
            .HasColumnName("writing_prompt_id")
            .IsRequired();

        builder.HasIndex(s => s.WritingPromptId)
            .HasDatabaseName("ix_writing_submissions_writing_prompt_id");

        builder.Property(s => s.Content)
            .HasColumnName("content")
            .IsRequired()
            .HasMaxLength(20000);

        builder.Property(s => s.WordCount)
            .HasColumnName("word_count")
            .IsRequired();

        builder.Property(s => s.IsTimed)
            .HasColumnName("is_timed")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(s => s.SubmittedAt)
            .HasColumnName("submitted_at");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(s => s.WritingPrompt)
            .WithMany(p => p.Submissions)
            .HasForeignKey(s => s.WritingPromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}