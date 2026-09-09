using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Infrastructure.Persistence.Configurations;

public class GradingResultConfiguration : IEntityTypeConfiguration<GradingResult>
{
    public void Configure(EntityTypeBuilder<GradingResult> builder)
    {
        builder.ToTable("grading_results");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(g => g.SubmissionId)
            .HasColumnName("submission_id")
            .IsRequired();

        builder.HasIndex(g => g.SubmissionId)
            .HasDatabaseName("ix_grading_results_submission_id");

        builder.Property(g => g.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(g => g.UserId)
            .HasDatabaseName("ix_grading_results_user_id");

        builder.Property(g => g.OverallBand)
            .HasColumnName("overall_band")
            .IsRequired()
            .HasColumnType("numeric(3,1)");

        builder.Property(g => g.CriterionScores)
            .HasColumnName("criterion_scores")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<CriterionScore>>(v, (JsonSerializerOptions?)null)
                    ?? new List<CriterionScore>(),
                new ValueComparer<List<CriterionScore>>(
                    (left, right) =>
                        JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) ==
                        JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
                    value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
                    value => JsonSerializer.Deserialize<List<CriterionScore>>(
                                 JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                                 (JsonSerializerOptions?)null)
                             ?? new List<CriterionScore>()));

        builder.Property(g => g.OverallFeedback)
            .HasColumnName("overall_feedback")
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(g => g.GrammarErrorsJson)
            .HasColumnName("grammar_errors_json")
            .HasColumnType("jsonb");

        builder.Property(g => g.VocabularySuggestionsJson)
            .HasColumnName("vocabulary_suggestions_json")
            .HasColumnType("jsonb");

        builder.Property(g => g.RestructuringSuggestionsJson)
            .HasColumnName("restructuring_suggestions_json")
            .HasColumnType("jsonb");

        builder.Property(g => g.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(g => g.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(g => g.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(g => g.CompletedAt)
            .HasColumnName("completed_at");
    }
}
