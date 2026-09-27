using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Persistence.Configurations;

public class VocabGroupItemConfiguration : IEntityTypeConfiguration<VocabGroupItem>
{
    public void Configure(EntityTypeBuilder<VocabGroupItem> builder)
    {
        builder.ToTable("VocabGroupItems");
        builder.HasKey(x => new { x.VocabGroupId, x.VocabSuggestionId });
        
        builder.HasOne(x => x.Group)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.VocabGroupId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.Vocab)
            .WithMany()
            .HasForeignKey(x => x.VocabSuggestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
