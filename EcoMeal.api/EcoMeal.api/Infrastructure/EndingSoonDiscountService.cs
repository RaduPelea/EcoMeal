using EcoMeal.api.Entities;
using Microsoft.EntityFrameworkCore;

using EcoMeal.api.Infrastructure;

namespace EcoMeal.api.Infrastructure;

// Applies each package's own scheduled discount plan when its pickup window
// gets close enough to the end (DiscountHoursBeforeEnd hours before EndPickup).
public class EndingSoonDiscountService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EndingSoonDiscountService> _logger;

    public EndingSoonDiscountService(IServiceScopeFactory scopeFactory, ILogger<EndingSoonDiscountService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ApplyDiscountsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply ending-soon discounts");
            }

            await Task.Delay(TimeSpan.FromMinutes(12), stoppingToken);
        }
    }

    private async Task ApplyDiscountsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EcomealDbContext>();

        var now = AppTime.Now;

        var packages = await context.Packages
            .Where(p => p.OriginalPrice == null
                        && p.DiscountPercent != null
                        && p.DiscountHoursBeforeEnd != null
                        && p.Quantity > 0
                        && p.EndPickup > now
                        && EF.Functions.DateDiffMinute(now, p.EndPickup) <= p.DiscountHoursBeforeEnd * 60)
            .ToListAsync(ct);

        if (packages.Count == 0)
            return;

        foreach (var package in packages)
        {
            package.OriginalPrice = package.Price;
            package.Price = Math.Round(package.Price * (100 - package.DiscountPercent!.Value) / 100m, 2);
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Applied ending-soon discount to {Count} package(s)", packages.Count);
    }
}
