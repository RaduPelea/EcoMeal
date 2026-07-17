using System.Globalization;
using System.Text.Json;

namespace EcoMeal.api.Infrastructure;

public interface IGeocodingService
{
    Task<(double Lat, double Lon)?> GeocodeAsync(string address, string city, string country);
}

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _http;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(HttpClient http, ILogger<GeocodingService> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
        _http.DefaultRequestHeaders.Add("User-Agent", "EcoMeal-StudentProject");
        _http.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<(double Lat, double Lon)?> GeocodeAsync(string address, string city, string country)
    {
        return await SearchAsync($"{address}, {city}, {country}")
               ?? await SearchAsync($"{city}, {country}");
    }

    private async Task<(double Lat, double Lon)?> SearchAsync(string query)
    {
        try
        {
            var url = $"search?format=json&limit=1&q={Uri.EscapeDataString(query)}";
            using var doc = JsonDocument.Parse(await _http.GetStringAsync(url));

            if (doc.RootElement.GetArrayLength() == 0)
                return null;

            var lat = double.Parse(doc.RootElement[0].GetProperty("lat").GetString()!, CultureInfo.InvariantCulture);
            var lon = double.Parse(doc.RootElement[0].GetProperty("lon").GetString()!, CultureInfo.InvariantCulture);
            return (lat, lon);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Geocoding failed for {Query}", query);
            return null;
        }
    }
}
