using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("quizzes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Category).HasMaxLength(500).IsRequired();

        builder.Property(q => q.Questions)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<List<QuizQuestionItem>>(v, JsonOptions.JsonSerializerOptions) ??
                     new List<QuizQuestionItem>(),
                new ValueComparer<List<QuizQuestionItem>>(
                    (left, right) =>
                        JsonSerializer.Serialize(left, JsonOptions.JsonSerializerOptions) ==
                        JsonSerializer.Serialize(right, JsonOptions.JsonSerializerOptions),
                    value => JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions).GetHashCode(),
                    value => JsonSerializer.Deserialize<List<QuizQuestionItem>>(
                                 JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions),
                                 JsonOptions.JsonSerializerOptions)
                             ?? new List<QuizQuestionItem>()));

        builder.HasIndex(q => q.UserId);
        builder.HasIndex(q => q.SourceSubmissionId);
    }
}