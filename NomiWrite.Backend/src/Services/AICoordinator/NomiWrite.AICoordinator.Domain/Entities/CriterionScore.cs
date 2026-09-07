namespace NomiWrite.AICoordinator.Domain.Entities;

public class CriterionScore
{
    public string CriterionName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}
