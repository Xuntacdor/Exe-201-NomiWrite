using System;
using System.Text.Json.Serialization;

namespace NomiWrite.Learning.Domain.Entities;

public class VocabGroupItem
{
    public Guid VocabGroupId { get; set; }
    
    [JsonIgnore]
    public VocabGroup Group { get; set; } = null!;

    public Guid VocabSuggestionId { get; set; }
    
    [JsonIgnore]
    public VocabSuggestion Vocab { get; set; } = null!;
}
