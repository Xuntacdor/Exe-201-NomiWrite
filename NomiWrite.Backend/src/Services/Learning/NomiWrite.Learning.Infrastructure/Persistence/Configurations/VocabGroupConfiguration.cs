using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Infrastructure.Persistence.Configurations;

public class VocabGroupConfiguration : IEntityTypeConfiguration<VocabGroup>
{
    public void Configure(EntityTypeBuilder<VocabGroup> builder)
    {
        builder.ToTable("VocabGroups");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        
        builder.HasIndex(x => x.UserId);
    }
}
