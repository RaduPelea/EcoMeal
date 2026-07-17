using System.Security.Claims;
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
public class FavoriteController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public FavoriteController(EcomealDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<BusinessDTO>>> GetMyFavorites()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => new BusinessDTO
            {
                Id = f.Business!.Id,
                Name = f.Business.Name,
                Address = f.Business.Address,
                City = f.Business.City,
                ImageUrl = f.Business.ImageUrl,
                Description = f.Business.Description,
                Contact = f.Business.Contact,
                Rating = f.Business.Reviews.Count != 0 ? Math.Round(f.Business.Reviews.Average(r => (decimal)r.Stars), 1) : 0,
                BusinessTypeName = f.Business.BusinessType!.Name
            })
            .ToListAsync();

        return Ok(favorites);
    }

    [HttpGet("ids")]
    public async Task<ActionResult<List<int>>> GetMyFavoriteIds()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var ids = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.BusinessId)
            .ToListAsync();

        return Ok(ids);
    }

    [HttpPost("{businessId:int}")]
    public async Task<IActionResult> Toggle(int businessId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var businessExists = await _context.Businesses.AnyAsync(b => b.Id == businessId);
        if (!businessExists)
            return NotFound("This business no longer exists.");

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.BusinessId == businessId);

        bool isFavorite;
        if (favorite is null)
        {
            _context.Favorites.Add(new Favorite { UserId = userId.Value, BusinessId = businessId });
            isFavorite = true;
        }
        else
        {
            _context.Favorites.Remove(favorite);
            isFavorite = false;
        }

        await _context.SaveChangesAsync();
        return Ok(new { IsFavorite = isFavorite });
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
