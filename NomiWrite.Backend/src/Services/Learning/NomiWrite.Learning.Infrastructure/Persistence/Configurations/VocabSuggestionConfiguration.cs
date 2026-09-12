using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Persistence.Configurations;

public class VocabSuggestionConfiguration : IEntityTypeConfiguration<VocabSuggestion>
{
    public void Configure(EntityTypeBuilder<VocabSuggestion> builder)
    {
        builder.ToTable("vocab_suggestions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Topic).HasMaxLength(100).IsRequired();
        builder.Property(v => v.OriginalWord).HasMaxLength(100).IsRequired();
        builder.Property(v => v.SuggestedWord).HasMaxLength(100).IsRequired();
        builder.Property(v => v.ExampleSentence).HasMaxLength(2000).IsRequired();

        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.SubmissionId);
        builder.HasIndex(v => v.Topic);
    }
}