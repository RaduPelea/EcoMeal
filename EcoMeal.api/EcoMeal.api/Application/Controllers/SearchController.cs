using EcoMeal.api.Application.Models;
using EcoMeal.api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public SearchController(EcomealDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<SearchResultDTO>>> Search([FromQuery] string? q)
    {
        var term = q?.Trim() ?? "";
        if (term.Length < 2)
            return Ok(new List<SearchResultDTO>());

        var businesses = await _context.Businesses
            .Where(b => b.Name.Contains(term) || b.City.Contains(term))
            .OrderBy(b => b.Name)
            .Take(5)
            .Select(b => new SearchResultDTO
            {
                Type = "Business",
                BusinessId = b.Id,
                Name = b.Name,
                Detail = b.City,
                ImageUrl = b.ImageUrl
            })
            .ToListAsync();

        var packages = await _context.Packages
            .Where(p => p.Quantity > 0 && p.Name.Contains(term))
            .OrderBy(p => p.Name)
            .Take(5)
            .Select(p => new SearchResultDTO
            {
                Type = "Package",
                BusinessId = p.BusinessId,
                Name = p.Name,
                Detail = p.Business!.Name,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync();

        return Ok(businesses.Concat(packages).ToList());
    }
}
