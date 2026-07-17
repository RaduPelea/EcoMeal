namespace Ecomeal.client.Models;

public class StatsModel
{
    public int TotalOrders { get; set; }
    public int PickedUpOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int TotalUsers { get; set; }
    public int TotalPartners { get; set; }
    public int TotalBusinesses { get; set; }
    public int AvailablePackages { get; set; }
    public decimal FoodSavedValue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public List<DailyOrdersModel> OrdersPerDay { get; set; } = new();
    public List<TopBusinessModel> TopBusinesses { get; set; } = new();
    public List<CityStatsModel> TopCities { get; set; } = new();
}

public class DailyOrdersModel
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class TopBusinessModel
{
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string BusinessTypeName { get; set; } = "";
    public int Orders { get; set; }
}

public class CityStatsModel
{
    public string City { get; set; } = "";
    public int Businesses { get; set; }
    public int Orders { get; set; }
}
