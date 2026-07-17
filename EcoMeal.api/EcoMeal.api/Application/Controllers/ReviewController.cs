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
public class ReviewController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public ReviewController(EcomealDbContext context)
    {
        _context = context;
    }

    [HttpGet("business/{businessId:int}")]
    public async Task<ActionResult<List<ReviewDTO>>> GetForBusiness(int businessId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.BusinessId == businessId)
            .OrderByDescending(r => r.Date)
            .Select(r => new ReviewDTO
            {
                Id = r.Id,
                UserName = r.User!.Name,
                Stars = r.Stars,
                Comment = r.Comment,
                Date = r.Date
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<List<MyReviewDTO>>> GetMyReviews()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var reviews = await _context.Reviews
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Date)
            .Select(r => new MyReviewDTO
            {
                Id = r.Id,
                BusinessId = r.BusinessId,
                BusinessName = r.Business!.Name,
                Stars = r.Stars,
                Comment = r.Comment,
                Date = r.Date
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("eligible/{businessId:int}")]
    [Authorize]
    public async Task<ActionResult<bool>> CanReview(int businessId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        return Ok(await CanReviewBusiness(userId.Value, businessId));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var review = await _context.Reviews.FindAsync(id);
        if (review is null)
            return NotFound("This review no longer exists.");

        if (review.UserId != userId && !User.IsInRole(UserRoles.Admin))
            return BadRequest("You don't have permission to delete this review.");

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Submit([FromBody] ReviewAddDTO dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var businessExists = await _context.Businesses.AnyAsync(b => b.Id == dto.BusinessId);
        if (!businessExists)
            return NotFound("The business was not found");

        if (!await CanReviewBusiness(userId.Value, dto.BusinessId))
            return BadRequest("You can only review a business after your order has been picked up");

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.BusinessId == dto.BusinessId && r.UserId == userId);

        if (review is null)
        {
            _context.Reviews.Add(new Review
            {
                BusinessId = dto.BusinessId,
                UserId = userId.Value,
                Stars = dto.Stars,
                Comment = dto.Comment,
                Date = AppTime.Now
            });
        }
        else
        {
            review.Stars = dto.Stars;
            review.Comment = dto.Comment;
            review.Date = AppTime.Now;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("customer/pending")]
    [Authorize(Roles = UserRoles.Partner)]
    public async Task<ActionResult<List<PendingCustomerReviewDTO>>> GetPendingCustomerReviews()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var pending = await _context.Orders
            .Where(o => o.Package!.Business!.OwnerId == userId
                        && o.Status == OrderStatus.PickedUp
                        && !_context.CustomerReviews.Any(r => r.OrderId == o.Id))
            .OrderByDescending(o => o.Date)
            .Select(o => new PendingCustomerReviewDTO
            {
                OrderId = o.Id,
                CustomerName = o.User!.Name ?? "",
                PackageName = o.Package!.Name
            })
            .ToListAsync();

        return Ok(pending);
    }

    [HttpPost("customer")]
    [Authorize(Roles = UserRoles.Partner)]
    public async Task<IActionResult> SubmitCustomerReview([FromBody] CustomerReviewAddDTO dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.Package!)
            .ThenInclude(p => p.Business)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order is null)
            return NotFound("This order no longer exists.");

        if (order.Package!.Business!.OwnerId != userId)
            return BadRequest("You don't have permission to rate this customer.");

        if (order.Status != OrderStatus.PickedUp)
            return BadRequest("You can only rate a customer after the order has been picked up");

        var review = await _context.CustomerReviews
            .FirstOrDefaultAsync(r => r.OrderId == dto.OrderId);

        if (review is null)
        {
            _context.CustomerReviews.Add(new CustomerReview
            {
                OrderId = dto.OrderId,
                Stars = dto.Stars,
                Comment = dto.Comment,
                Date = AppTime.Now
            });
        }
        else
        {
            review.Stars = dto.Stars;
            review.Comment = dto.Comment;
            review.Date = AppTime.Now;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> CanReviewBusiness(int userId, int businessId)
    {
        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.BusinessId == businessId && r.UserId == userId);

        if (alreadyReviewed)
            return true;

        return await _context.Orders.AnyAsync(o =>
            o.UserId == userId &&
            o.Status == OrderStatus.PickedUp &&
            o.Package!.BusinessId == businessId);
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
