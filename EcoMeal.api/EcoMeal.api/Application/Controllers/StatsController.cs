using System.Security.Claims;
using EcoMeal.api.Application.Constants;
using EcoMeal.api.Application.Models;
using EcoMeal.api.Entities;
using EcoMeal.api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public StatsController(EcomealDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<StatsDTO>> Get()
    {
        var since = AppTime.Now.Date.AddDays(-6);

        var pickedUp = await _context.Orders.CountAsync(o => o.Status == OrderStatus.PickedUp);
        var foodSaved = await _context.Orders
            .Where(o => o.Status == OrderStatus.PickedUp)
            .SumAsync(o => (decimal?)o.Price) ?? 0;

        var stats = new StatsDTO
        {
            TotalOrders = await _context.Orders.CountAsync(),
            PickedUpOrders = pickedUp,
            CancelledOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled),
            TotalUsers = await _context.Users.CountAsync(),
            TotalPartners = await _context.Businesses
                .Where(b => b.OwnerId != null)
                .Select(b => b.OwnerId)
                .Distinct()
                .CountAsync(),
            TotalBusinesses = await _context.Businesses.CountAsync(),
            AvailablePackages = await _context.Packages.CountAsync(p => p.Quantity > 0 && p.EndPickup > AppTime.Now),
            FoodSavedValue = foodSaved,
            AverageOrderValue = pickedUp > 0 ? Math.Round(foodSaved / pickedUp, 2) : 0,
            TotalReviews = await _context.Reviews.CountAsync(),
            AverageRating = await _context.Reviews.AnyAsync()
                ? Math.Round(await _context.Reviews.AverageAsync(r => (decimal)r.Stars), 1)
                : 0,
            OrdersPerDay = await _context.Orders
                .Where(o => o.Date >= since)
                .GroupBy(o => o.Date.Date)
                .Select(g => new DailyOrdersDTO { Date = g.Key, Count = g.Count() })
                .OrderBy(d => d.Date)
                .ToListAsync(),
            TopBusinesses = await _context.Orders
                .GroupBy(o => new { o.Package!.Business!.Name, o.Package.Business.City, TypeName = o.Package.Business.BusinessType!.Name })
                .Select(g => new TopBusinessDTO
                {
                    Name = g.Key.Name,
                    City = g.Key.City,
                    BusinessTypeName = g.Key.TypeName,
                    Orders = g.Count()
                })
                .OrderByDescending(t => t.Orders)
                .Take(5)
                .ToListAsync(),
            TopCities = await _context.Businesses
                .GroupBy(b => b.City)
                .Select(g => new CityStatsDTO
                {
                    City = g.Key,
                    Businesses = g.Count(),
                    Orders = _context.Orders.Count(o => o.Package!.Business!.City == g.Key)
                })
                .OrderByDescending(c => c.Orders)
                .ThenByDescending(c => c.Businesses)
                .Take(6)
                .ToListAsync()
        };

        return Ok(stats);
    }

    [HttpGet("business")]
    [Authorize(Roles = UserRoles.Partner)]
    public async Task<ActionResult<BusinessStatsDTO>> GetBusinessStats()
    {
        var userIdValue = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var businessNames = await _context.Businesses
            .Where(b => b.OwnerId == userId)
            .OrderBy(b => b.Id)
            .Select(b => b.Name)
            .ToListAsync();

        if (businessNames.Count == 0)
            return NotFound("You don't own a business.");

        var orders = _context.Orders.Where(o => o.Package!.Business!.OwnerId == userId);
        var since = AppTime.Now.AddMonths(-5);
        var sinceMonth = new DateTime(since.Year, since.Month, 1);

        var stats = new BusinessStatsDTO
        {
            BusinessName = businessNames.Distinct().Count() == 1
                ? $"{businessNames[0]}{(businessNames.Count > 1 ? $" ({businessNames.Count} locations)" : "")}"
                : string.Join(" + ", businessNames.Distinct()),
            TotalOrders = await orders.CountAsync(),
            PickedUpOrders = await orders.CountAsync(o => o.Status == OrderStatus.PickedUp),
            CancelledOrders = await orders.CountAsync(o => o.Status == OrderStatus.Cancelled),
            Revenue = await orders
                .Where(o => o.Status == OrderStatus.PickedUp)
                .SumAsync(o => (decimal?)o.Price) ?? 0,
            RevenueByMonth = await orders
                .Where(o => o.Status == OrderStatus.PickedUp && o.Date >= sinceMonth)
                .GroupBy(o => new { o.Date.Year, o.Date.Month })
                .Select(g => new MonthlyRevenueDTO
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(o => o.Price),
                    Orders = g.Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToListAsync(),
            TopPackages = await orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.Package!.Name)
                .Select(g => new TopPackageDTO { Name = g.Key, Orders = g.Count() })
                .OrderByDescending(t => t.Orders)
                .Take(5)
                .ToListAsync()
        };

        return Ok(stats);
    }
}
