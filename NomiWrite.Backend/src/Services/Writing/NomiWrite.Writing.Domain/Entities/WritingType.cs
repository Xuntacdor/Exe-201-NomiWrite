using NomiWrite.Writing.Domain.Common;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Domain.Entities;

public class WritingType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public WritingTypeCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<WritingPrompt> Prompts { get; set; } = new List<WritingPrompt>();
}