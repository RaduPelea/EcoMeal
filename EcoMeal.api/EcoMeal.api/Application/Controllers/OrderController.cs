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
[Authorize]
public class OrderController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public OrderController(EcomealDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<OrderGetDTO>> PlaceOrder([FromBody] OrderCreateDTO request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var package = await _context.Packages
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.Id == request.PackageId);

        if (package is null)
            return NotFound("The package was not found");

        if (AppTime.Now > package.EndPickup)
            return BadRequest("The pickup window for this package has ended");

        var user = await _context.Users.FindAsync(userId);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var reserved = await _context.Packages
            .Where(p => p.Id == package.Id && p.Quantity > 0)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, p => p.Quantity - 1));

        if (reserved == 0)
            return BadRequest("The package is no longer available");

        var order = new Order
        {
            UserId = userId.Value,
            PackageId = package.Id,
            Status = OrderStatus.New,
            Date = AppTime.Now,
            Price = package.Price
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new OrderGetDTO
        {
            Id = order.Id,
            PackageName = package.Name,
            Status = order.Status.ToString(),
            Price = package.Price,
            BusinessId = package.BusinessId,
            BusinessName = package.Business!.Name,
            Date = order.Date,
            UserName = user?.Name,
            UserContact = user?.Contact,
            EndPickup = package.EndPickup
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderGetDTO>>> GetMyOrders()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Date)
            .Select(o => new OrderGetDTO
            {
                Id = o.Id,
                PackageName = o.Package!.Name,
                Status = o.Status.ToString(),
                Price = o.Price,
                LoyaltyDiscountPercent = o.LoyaltyDiscountPercent,
                BusinessId = o.Package.BusinessId,
                BusinessName = o.Package.Business!.Name,
                Date = o.Date,
                UserName = o.User!.Name,
                UserContact = o.User.Contact,
                EndPickup = o.Package.EndPickup
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost("batch")]
    public async Task<IActionResult> PlaceBatchOrder([FromBody] OrderCreateBatchDTO request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var packageIds = request.PackageIds.Distinct().ToList();
        if (packageIds.Count == 0)
            return BadRequest("The basket is empty");

        var packages = await _context.Packages
            .Where(p => packageIds.Contains(p.Id))
            .ToListAsync();

        if (packages.Count != packageIds.Count)
            return BadRequest("Some packages no longer exist");

        if (packages.Select(p => p.BusinessId).Distinct().Count() > 1)
            return BadRequest("All packages must be from the same place");

        var expired = packages.FirstOrDefault(p => AppTime.Now > p.EndPickup);
        if (expired is not null)
            return BadRequest($"The pickup window for '{expired.Name}' has ended");

        var user = await _context.Users.FindAsync(userId);
        if (request.UseLoyaltyDiscount && (user is null || !user.HasActiveLoyaltyDiscount))
            return BadRequest("You don't have a loyalty discount to use right now.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        foreach (var package in packages)
        {
            var reserved = await _context.Packages
                .Where(p => p.Id == package.Id && p.Quantity > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, p => p.Quantity - 1));

            if (reserved == 0)
            {
                await transaction.RollbackAsync();
                return BadRequest($"'{package.Name}' is no longer available");
            }

            _context.Orders.Add(new Order
            {
                UserId = userId.Value,
                PackageId = package.Id,
                Status = OrderStatus.New,
                Date = AppTime.Now,
                LoyaltyDiscountPercent = request.UseLoyaltyDiscount ? 20 : null,
                Price = request.UseLoyaltyDiscount
                    ? Math.Round(package.Price * 80 / 100m, 2)
                    : package.Price
            });
        }

        if (request.UseLoyaltyDiscount)
            user!.HasActiveLoyaltyDiscount = false;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new { Count = packages.Count });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderGetDTO>> GetOneById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var order = await _context.Orders
            .Where(o => o.Id == id && o.UserId == userId)
            .Select(o => new OrderGetDTO
            {
                Id = o.Id,
                PackageName = o.Package!.Name,
                Status = o.Status.ToString(),
                Price = o.Price,
                LoyaltyDiscountPercent = o.LoyaltyDiscountPercent,
                BusinessId = o.Package.BusinessId,
                BusinessName = o.Package.Business!.Name,
                Date = o.Date,
                UserName = o.User!.Name,
                UserContact = o.User.Contact,
                EndPickup = o.Package.EndPickup
            })
            .FirstOrDefaultAsync();

        if (order is null)
            return NotFound();

        return Ok(order);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDTO dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
            return BadRequest("Unknown order status");

        var order = await _context.Orders
            .Include(o => o.Package!)
            .ThenInclude(p => p.Business)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound("This order no longer exists.");

        var isCustomer = order.UserId == userId;
        var isPartner = order.Package!.Business!.OwnerId == userId || User.IsInRole(UserRoles.Admin);

        var pickupExpired = AppTime.Now > order.Package.EndPickup;

        var allowed = newStatus switch
        {
            OrderStatus.Cancelled => (isCustomer || isPartner) && order.Status is OrderStatus.New or OrderStatus.Confirmed
                                     && !(isCustomer && !isPartner && pickupExpired),
            OrderStatus.Confirmed => isPartner && order.Status == OrderStatus.New,
            OrderStatus.PickedUp => isPartner && order.Status is OrderStatus.New or OrderStatus.Confirmed,
            _ => false
        };

        if (!allowed)
        {
            var reason = !isCustomer && !isPartner
                ? "You don't have permission to change this order."
                : order.Status is OrderStatus.PickedUp or OrderStatus.Cancelled
                    ? $"This order is already {order.Status.ToString().ToLower()} and can no longer be changed."
                    : newStatus == OrderStatus.Cancelled && pickupExpired
                        ? "The pickup window has passed, so this order can no longer be cancelled."
                        : $"An order that is '{order.Status}' cannot be changed to '{newStatus}'.";
            return BadRequest(reason);
        }

        if (newStatus == OrderStatus.Cancelled)
            order.Package.Quantity += 1;

        order.Status = newStatus;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("loyalty")]
    public async Task<ActionResult<LoyaltyStatusDTO>> GetLoyaltyStatus()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            return Unauthorized();

        var pickedUp = await _context.Orders
            .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.PickedUp);
        var earned = pickedUp / 3;

        return Ok(new LoyaltyStatusDTO
        {
            PickedUpOrders = pickedUp,
            EarnedRewards = earned,
            ClaimedRewards = user.LoyaltyClaimedRewards,
            HasActiveDiscount = user.HasActiveLoyaltyDiscount,
            CanClaim = earned > user.LoyaltyClaimedRewards && !user.HasActiveLoyaltyDiscount,
            OrdersUntilNextReward = pickedUp % 3 == 0 ? 3 : 3 - pickedUp % 3
        });
    }

    [HttpPost("loyalty/claim")]
    public async Task<IActionResult> ClaimLoyaltyReward()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            return Unauthorized();

        var pickedUp = await _context.Orders
            .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.PickedUp);

        if (pickedUp / 3 <= user.LoyaltyClaimedRewards)
            return BadRequest("You don't have a reward to claim yet — complete more orders!");

        if (user.HasActiveLoyaltyDiscount)
            return BadRequest("You already have an unused 20% discount. Use it in your basket first!");

        user.LoyaltyClaimedRewards += 1;
        user.HasActiveLoyaltyDiscount = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("pending-reviews")]
    public async Task<ActionResult<List<PendingReviewDTO>>> GetPendingReviews()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var pending = await _context.Orders
            .Where(o => o.UserId == userId
                        && o.Status == OrderStatus.PickedUp
                        && !_context.Reviews.Any(r => r.UserId == userId
                                                      && r.BusinessId == o.Package!.BusinessId
                                                      && r.Date >= o.Date))
            .OrderByDescending(o => o.Date)
            .Select(o => new PendingReviewDTO
            {
                OrderId = o.Id,
                BusinessId = o.Package!.BusinessId,
                BusinessName = o.Package.Business!.Name,
                PackageName = o.Package.Name
            })
            .ToListAsync();

        var distinctPending = pending
            .GroupBy(p => p.BusinessId)
            .Select(g => g.First())
            .ToList();

        return Ok(distinctPending);
    }

    [HttpGet("business")]
    [Authorize(Roles = UserRoles.Partner)]
    public async Task<ActionResult<List<OrderGetDTO>>> GetBusinessOrders()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var orders = await _context.Orders
            .Where(o => o.Package!.Business!.OwnerId == userId)
            .OrderByDescending(o => o.Date)
            .Select(o => new OrderGetDTO
            {
                Id = o.Id,
                PackageName = o.Package!.Name,
                Status = o.Status.ToString(),
                Price = o.Price,
                LoyaltyDiscountPercent = o.LoyaltyDiscountPercent,
                BusinessId = o.Package.BusinessId,
                BusinessName = o.Package.Business!.Name,
                Date = o.Date,
                UserName = o.User!.Name,
                UserContact = o.User.Contact,
                EndPickup = o.Package.EndPickup,
                CustomerRating = _context.CustomerReviews
                    .Where(r => r.Order!.UserId == o.UserId)
                    .Average(r => (decimal?)r.Stars),
                CustomerRatingCount = _context.CustomerReviews
                    .Count(r => r.Order!.UserId == o.UserId)
            })
            .ToListAsync();

        return Ok(orders);
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
