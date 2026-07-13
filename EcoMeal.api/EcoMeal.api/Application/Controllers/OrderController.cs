using System.Security.Claims;
using EcoMeal.api.Application.Models;
using EcoMeal.api.Infrastructure;
using EcoMeal.api.Models;
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

        var package = await _context.Packages
            .Include(p => p.Business)
            .Include(p => p.Orders)
            .FirstOrDefaultAsync(p => p.Id == request.PackageId);

        if (package is null)
            return NotFound("The package was not found");

        if (package.Orders.Any())
            return BadRequest("The package is no longer available");

        var user = await _context.Users.FindAsync(userId);

        var order = new Order
        {
            UserId = userId,
            PackageId = package.Id,
            Package = package,
            Status = OrderStatus.New,
            Date = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(new OrderGetDTO
        {
            Id = order.Id,
            PackageName = package.Name,
            Status = order.Status.ToString(),
            Price = package.Price,
            BusinessId = package.BusinessId,
            BusinessName = package.Business.Name,
            Date = order.Date,
            UserName = user?.Name,
            UserContact = user?.Contact
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderGetDTO>>> GetMyOrders()
    {
        var userId = GetCurrentUserId();

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Date)
            .Select(o => new OrderGetDTO
            {
                Id = o.Id,
                PackageName = o.Package.Name,
                Status = o.Status.ToString(),
                Price = o.Package.Price,
                BusinessId = o.Package.BusinessId,
                BusinessName = o.Package.Business.Name,
                Date = o.Date,
                UserName = o.User.Name,
                UserContact = o.User.Contact
            })
            .ToListAsync();

        return Ok(orders);
    }

    private int GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }
}
