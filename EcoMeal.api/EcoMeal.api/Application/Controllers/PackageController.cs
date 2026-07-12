using EcoMeal.api.Application.Models;
using EcoMeal.api.Infrastructure;
using EcoMeal.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackageController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public PackageController(EcomealDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PackageDTO>> GetOneById(int id)
    {
        var package = await _context.Packages
            .Where(p => p.Id == id)
            .Select(p => new PackageDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                StartPickup = p.StartPickup,
                EndPickup = p.EndPickup,
                PackageTypeId = p.PackageTypeId,
                PackageTypeName = p.PackageType!.Name
            })
            .FirstOrDefaultAsync();

        if (package is null)
            return NotFound();

        return Ok(package);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] packageAddDTO dto)
    {
        var package = await _context.Packages.FindAsync(id);
        if (package is null)
            return NotFound();

        package.Name = dto.Name;
        package.Description = dto.Description;
        package.ImageUrl = dto.ImageUrl;
        package.Price = dto.Price;
        package.StartPickup = dto.StartPickup;
        package.EndPickup = dto.EndPickup;
        package.PackageTypeId = dto.PackageTypeId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var package = await _context.Packages.FindAsync(id);
        if (package is null)
            return NotFound();

        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
