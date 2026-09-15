using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Persistence.Configurations;

public class GrammarErrorConfiguration : IEntityTypeConfiguration<GrammarError>
{
    public void Configure(EntityTypeBuilder<GrammarError> builder)
    {
        builder.ToTable("grammar_errors");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GrammarCategory).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Sentence).HasMaxLength(5000).IsRequired();
        builder.Property(e => e.ErrorPart).HasMaxLength(2000);
        builder.Property(e => e.Suggestion).HasMaxLength(2000);
        builder.Property(e => e.Explanation).HasMaxLength(2000);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.SubmissionId);
    }
}