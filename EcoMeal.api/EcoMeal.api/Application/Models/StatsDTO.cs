namespace EcoMeal.api.Application.Models;

public class StatsDTO
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
    public List<DailyOrdersDTO> OrdersPerDay { get; set; } = new();
    public List<TopBusinessDTO> TopBusinesses { get; set; } = new();
    public List<CityStatsDTO> TopCities { get; set; } = new();
}

public class DailyOrdersDTO
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class TopBusinessDTO
{
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string BusinessTypeName { get; set; } = "";
    public int Orders { get; set; }
}

public class CityStatsDTO
{
    public string City { get; set; } = "";
    public int Businesses { get; set; }
    public int Orders { get; set; }
}
