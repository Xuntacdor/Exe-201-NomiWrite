using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NomiWrite.Learning.Domain.Entities;

public class VocabGroup : Common.BaseEntity
{
    public Guid UserId { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<VocabGroupItem> Items { get; set; } = new List<VocabGroupItem>();
}
