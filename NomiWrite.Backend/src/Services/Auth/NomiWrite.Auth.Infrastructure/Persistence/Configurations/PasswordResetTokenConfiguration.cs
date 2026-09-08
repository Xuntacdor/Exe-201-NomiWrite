using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Auth.Domain.Entities;

namespace NomiWrite.Auth.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(t => t.Token).HasColumnName("token").IsRequired().HasMaxLength(256);
        builder.HasIndex(t => t.Token).IsUnique().HasDatabaseName("ix_password_reset_tokens_token");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(t => t.UserId).HasDatabaseName("ix_password_reset_tokens_user_id");
        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(t => t.IsUsed).HasColumnName("is_used").IsRequired().HasDefaultValue(false);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        builder.Property(t => t.IsDeleted).HasColumnName("is_deleted").IsRequired().HasDefaultValue(false);
        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasOne(t => t.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}