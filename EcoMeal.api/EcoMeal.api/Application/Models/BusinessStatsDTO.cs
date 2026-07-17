namespace EcoMeal.api.Application.Models;

public class BusinessStatsDTO
{
    public string BusinessName { get; set; } = "";
    public int TotalOrders { get; set; }
    public int PickedUpOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal Revenue { get; set; }
    public List<MonthlyRevenueDTO> RevenueByMonth { get; set; } = new();
    public List<TopPackageDTO> TopPackages { get; set; } = new();
}

public class MonthlyRevenueDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class TopPackageDTO
{
    public string Name { get; set; } = "";
    public int Orders { get; set; }
}
