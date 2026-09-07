using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Writing.Domain.Entities;

namespace NomiWrite.Writing.Infrastructure.Persistence.Configurations;

public class WritingPromptConfiguration : IEntityTypeConfiguration<WritingPrompt>
{
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
    }
}