using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Admin.Domain.Entities;

namespace NomiWrite.Admin.Infrastructure.Persistence.Configurations;

public class ContentReportConfiguration : IEntityTypeConfiguration<ContentReport>
{
    public void Configure(EntityTypeBuilder<ContentReport> builder)
    {
        builder.ToTable("content_reports");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(r => r.ReporterUserId)
            .HasColumnName("reporter_user_id")
            .IsRequired();

        builder.HasIndex(r => r.ReporterUserId)
            .HasDatabaseName("ix_content_reports_reporter_user_id");

        builder.Property(r => r.ContentType)
            .HasColumnName("content_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.TargetId)
            .HasColumnName("target_id")
            .IsRequired();

        builder.HasIndex(r => r.TargetId)
            .HasDatabaseName("ix_content_reports_target_id");

        builder.HasIndex(r => new { r.ContentType, r.TargetId })
            .HasDatabaseName("ix_content_reports_content_type_target_id");

        builder.Property(r => r.Reason)
            .HasColumnName("reason")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("ix_content_reports_status");

        builder.Property(r => r.ModeratorNotes)
            .HasColumnName("moderator_notes")
            .HasMaxLength(2000);

        builder.Property(r => r.ResolvedByUserId)
            .HasColumnName("resolved_by_user_id");

        builder.Property(r => r.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
