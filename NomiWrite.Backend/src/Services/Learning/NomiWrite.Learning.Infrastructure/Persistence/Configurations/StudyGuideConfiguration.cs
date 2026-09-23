using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Persistence.Configurations;

public class StudyGuideConfiguration : IEntityTypeConfiguration<StudyGuide>
{
    public void Configure(EntityTypeBuilder<StudyGuide> builder)
    {
        builder.ToTable("study_guides");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.TargetExam).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Summary).HasMaxLength(4000).IsRequired();
        builder.Property(g => g.EstimatedBand).HasPrecision(4, 1);
        builder.Property(g => g.TargetBand).HasPrecision(4, 1);

        builder.Property(g => g.Strengths)
            .HasColumnType("jsonb")
            .HasConversion(ForList<string>(), ListComparer<string>());

        builder.Property(g => g.Weaknesses)
            .HasColumnType("jsonb")
            .HasConversion(ForList<string>(), ListComparer<string>());

        builder.Property(g => g.NextSteps)
            .HasColumnType("jsonb")
            .HasConversion(ForList<StudyGuideStep>(), ListComparer<StudyGuideStep>());

        builder.Property(g => g.RecommendedTopic)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<StudyGuideTopic>(v, JsonOptions.JsonSerializerOptions) ?? new StudyGuideTopic(),
                new ValueComparer<StudyGuideTopic>(
                    (left, right) =>
                        JsonSerializer.Serialize(left, JsonOptions.JsonSerializerOptions) ==
                        JsonSerializer.Serialize(right, JsonOptions.JsonSerializerOptions),
                    value => JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions).GetHashCode(),
                    value => JsonSerializer.Deserialize<StudyGuideTopic>(
                                 JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions),
                                 JsonOptions.JsonSerializerOptions)
                             ?? new StudyGuideTopic()));

        builder.HasIndex(g => g.UserId);
    }

    private static ValueConverter<List<T>, string> ForList<T>() => new(
        value => JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions),
        value => JsonSerializer.Deserialize<List<T>>(value, JsonOptions.JsonSerializerOptions) ?? new List<T>());

    private static ValueComparer<List<T>> ListComparer<T>() => new(
        (left, right) =>
            JsonSerializer.Serialize(left, JsonOptions.JsonSerializerOptions) ==
            JsonSerializer.Serialize(right, JsonOptions.JsonSerializerOptions),
        value => JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions).GetHashCode(),
        value => JsonSerializer.Deserialize<List<T>>(
                     JsonSerializer.Serialize(value, JsonOptions.JsonSerializerOptions),
                     JsonOptions.JsonSerializerOptions)
                 ?? new List<T>());
}