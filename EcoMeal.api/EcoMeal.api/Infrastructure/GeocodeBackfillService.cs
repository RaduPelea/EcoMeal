using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Infrastructure;

public class GeocodeBackfillService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GeocodeBackfillService> _logger;

    public GeocodeBackfillService(IServiceScopeFactory scopeFactory, ILogger<GeocodeBackfillService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EcomealDbContext>();
            var geocoder = scope.ServiceProvider.GetRequiredService<IGeocodingService>();

            var missing = await context.Businesses
                .Where(b => b.Latitude == null || b.Longitude == null)
                .ToListAsync(stoppingToken);

            if (missing.Count == 0)
                return;

            var done = 0;
            foreach (var business in missing)
            {
                var coords = await geocoder.GeocodeAsync(business.Address, business.City, business.Country);
                if (coords is not null)
                {
                    business.Latitude = coords.Value.Lat;
                    business.Longitude = coords.Value.Lon;
                    done++;
                }

                // Nominatim allows at most 1 request per second
                await Task.Delay(TimeSpan.FromSeconds(1.2), stoppingToken);
            }

            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Geocoded {Done} of {Total} businesses", done, missing.Count);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geocode backfill failed");
        }
    }
}
