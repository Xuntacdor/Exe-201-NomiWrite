namespace NomiWrite.Admin.Application.DTOs;

public class AnalyticsOverviewDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int BannedUsers { get; set; }
    public int TotalSubmissions { get; set; }
    public int GradedSubmissions { get; set; }
    public int PendingSubmissions { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedTransactions { get; set; }
    public int ActiveVipMembers { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<string> Warnings { get; set; } = new();
}
