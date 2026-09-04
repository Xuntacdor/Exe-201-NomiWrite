using NomiWrite.Auth.Domain.Common;

namespace NomiWrite.Auth.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
