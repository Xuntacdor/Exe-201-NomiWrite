using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Infrastructure.Persistence.Configurations;

public class TutorReviewRequestConfiguration : IEntityTypeConfiguration<TutorReviewRequest>
{
    public void Configure(EntityTypeBuilder<TutorReviewRequest> builder)
    {
        builder.ToTable("tutor_review_requests");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(t => t.SubmissionId)
            .HasColumnName("submission_id")
            .IsRequired();

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("ix_tutor_review_requests_user_id");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.RequestedAt)
            .HasColumnName("requested_at")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");
    }
}