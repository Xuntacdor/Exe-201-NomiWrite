using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.User.Domain.Entities;
using NomiWrite.User.Domain.Enums;

namespace NomiWrite.User.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("ix_user_profiles_user_id");

        builder.Property(p => p.DisplayName)
            .HasColumnName("display_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(p => p.Bio)
            .HasColumnName("bio")
            .HasMaxLength(500);

        builder.Property(p => p.TargetExam)
            .HasColumnName("target_exam")
            .HasMaxLength(50);

        builder.Property(p => p.TargetBand)
            .HasColumnName("target_band")
            .HasColumnType("numeric(3,1)");

        builder.Property(p => p.TargetExamDate)
            .HasColumnName("target_exam_date");

        builder.Property(p => p.EnglishLevel)
            .HasColumnName("english_level")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");
    }
}