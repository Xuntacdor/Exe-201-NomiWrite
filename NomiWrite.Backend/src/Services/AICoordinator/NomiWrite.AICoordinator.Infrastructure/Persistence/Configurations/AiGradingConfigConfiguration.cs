using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Infrastructure.Persistence.Configurations;

public class AiGradingConfigConfiguration : IEntityTypeConfiguration<AiGradingConfig>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<AiGradingConfig> builder)
    {
        builder.ToTable("ai_grading_configs");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(c => c.ProviderName)
            .HasColumnName("provider_name")
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Gemini");

        builder.Property(c => c.ModelName)
            .HasColumnName("model_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Temperature)
            .HasColumnName("temperature");

        builder.Property(c => c.SystemPromptTemplate)
            .HasColumnName("system_prompt_template");

        builder.Property(c => c.MaxOutputTokens)
            .HasColumnName("max_output_tokens");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Seed one row matching the current hardcoded defaults so existing behaviour is unchanged
        // until an admin explicitly edits the config.
        builder.HasData(
            new AiGradingConfig
            {
                Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01"),
                ProviderName = "Gemini",
                ModelName = "gemini-2.5-flash",
                Temperature = null,
                SystemPromptTemplate = null,
                MaxOutputTokens = null,
                IsActive = true,
                CreatedAt = SeedCreatedAt
            });
    }
}