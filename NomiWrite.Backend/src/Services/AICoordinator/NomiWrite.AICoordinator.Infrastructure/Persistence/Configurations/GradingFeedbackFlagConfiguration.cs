using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Infrastructure.Persistence.Configurations;

public class GradingFeedbackFlagConfiguration : IEntityTypeConfiguration<GradingFeedbackFlag>
{
    public void Configure(EntityTypeBuilder<GradingFeedbackFlag> builder)
    {
        builder.ToTable("grading_feedback_flags");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(f => f.GradingResultId)
            .HasColumnName("grading_result_id")
            .IsRequired();

        builder.HasIndex(f => f.GradingResultId)
            .HasDatabaseName("ix_grading_feedback_flags_grading_result_id");

        builder.Property(f => f.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(f => f.UserId)
            .HasDatabaseName("ix_grading_feedback_flags_user_id");

        builder.Property(f => f.Reason)
            .HasColumnName("reason")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at");
    }
}