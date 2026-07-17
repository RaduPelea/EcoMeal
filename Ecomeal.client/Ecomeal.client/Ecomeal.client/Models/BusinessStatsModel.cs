namespace Ecomeal.client.Models;

public class BusinessStatsModel
{
    public string BusinessName { get; set; } = "";
    public int TotalOrders { get; set; }
    public int PickedUpOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal Revenue { get; set; }
    public List<MonthlyRevenueModel> RevenueByMonth { get; set; } = new();
    public List<TopPackageModel> TopPackages { get; set; } = new();
}

public class MonthlyRevenueModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class TopPackageModel
{
    public string Name { get; set; } = "";
    public int Orders { get; set; }
}
