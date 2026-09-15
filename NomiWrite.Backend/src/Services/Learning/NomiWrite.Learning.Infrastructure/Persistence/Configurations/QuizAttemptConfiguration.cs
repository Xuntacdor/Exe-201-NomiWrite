using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Persistence.Configurations;

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("quiz_attempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Answers)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions.JsonSerializerOptions) ??
                     new Dictionary<string, string>(),
                new ValueComparer<Dictionary<string, string>>(
                    (left, right) =>
                        JsonSerializer.Serialize(left, JsonOptions.JsonSerializerOptions) ==
                        JsonSerializer.Serialize(right, JsonOptions.JsonSerializerOptions),
                    value => JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions).GetHashCode(),
                    value => JsonSerializer.Deserialize<Dictionary<string, string>>(
                                 JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions),
                                 JsonOptions.JsonSerializerOptions)
                             ?? new Dictionary<string, string>()));

        builder.Property(a => a.Score).IsRequired();
        builder.Property(a => a.TotalQuestions).IsRequired();
        builder.Property(a => a.AttemptedAt).IsRequired();

        builder.HasIndex(a => a.QuizId);
        builder.HasIndex(a => a.UserId);

        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}