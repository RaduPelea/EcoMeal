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
                OriginalPrice = p.OriginalPrice,
                DiscountPercent = p.DiscountPercent,
                DiscountHoursBeforeEnd = p.DiscountHoursBeforeEnd,
                Quantity = p.Quantity,
                StartPickup = p.StartPickup,
                EndPickup = p.EndPickup,
                PackageTypeId = p.PackageTypeId,
                PackageTypeName = p.PackageType!.Name,
                Images = p.Images.Select(i => i.Url).ToList()
            })
            .FirstOrDefaultAsync();

        if (package is null)
            return NotFound();

        return Ok(package);
    }

    [HttpGet("ending-soon")]
    public async Task<ActionResult<List<EndingSoonPackageDTO>>> GetEndingSoon()
    {
        var now = AppTime.Now;
        var cutoff = now.AddHours(1);

        var packages = await _context.Packages
            .Where(p => p.Quantity > 0 && p.EndPickup > now && p.EndPickup <= cutoff)
            .OrderBy(p => p.EndPickup)
            .Select(p => new EndingSoonPackageDTO
            {
                PackageId = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                Quantity = p.Quantity,
                EndPickup = p.EndPickup,
                BusinessId = p.BusinessId,
                BusinessName = p.Business!.Name,
                City = p.Business.City
            })
            .ToListAsync();

        return Ok(packages);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Partner)]
    public async Task<IActionResult> Update(int id, [FromBody] PackageAddDto dto)
    {
        var package = await _context.Packages
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (package is null)
            return NotFound("This package no longer exists.");

        if (!await CanManagePackageAsync(package))
            return BadRequest("You don't have permission to edit this package.");

        if (dto.DiscountPercent is < 5 or > 90)
            return BadRequest("The discount must be between 5% and 90%");

        if (dto.EndPickup <= dto.StartPickup)
            return BadRequest("The pickup window must end after it starts");

        if (dto.EndPickup <= AppTime.Now.AddMinutes(15))
            return BadRequest("The pickup window must end at least 15 minutes from now — otherwise the package expires instantly and customers never see it");

        package.Images.Clear();
        foreach (var url in dto.ImageUrls.Distinct())
            package.Images.Add(new PackageImage { Url = url, PackageId = package.Id });

        package.Name = dto.Name;
        package.Description = dto.Description;
        package.ImageUrl = dto.ImageUrl;
        package.OriginalPrice = null;
        package.Price = dto.Price;
        package.DiscountPercent = dto.DiscountPercent;
        package.DiscountHoursBeforeEnd = dto.DiscountPercent is null ? null : dto.DiscountHoursBeforeEnd;

        if (package.DiscountPercent is not null && package.DiscountHoursBeforeEnd is null)
        {
            package.OriginalPrice = package.Price;
            package.Price = Math.Round(package.Price * (100 - package.DiscountPercent.Value) / 100m, 2);
        }
        package.Quantity = dto.Quantity;
        package.StartPickup = dto.StartPickup;
        package.EndPickup = dto.EndPickup;
        package.PackageTypeId = dto.PackageTypeId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Partner)]
    public async Task<IActionResult> Delete(int id)
    {
        var package = await _context.Packages.FindAsync(id);
        if (package is null)
            return NotFound("This package no longer exists.");

        if (!await CanManagePackageAsync(package))
            return BadRequest("You don't have permission to delete this package.");

        var hasActiveOrders = await _context.Orders.AnyAsync(o =>
            o.PackageId == id &&
            (o.Status == OrderStatus.New || o.Status == OrderStatus.Confirmed));

        if (hasActiveOrders)
            return BadRequest("This package has active orders and can't be deleted right now.");

        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> CanManagePackageAsync(Package package)
    {
        if (User.IsInRole(UserRoles.Admin))
            return true;

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
            return false;

        return await _context.Businesses.AnyAsync(b => b.Id == package.BusinessId && b.OwnerId == userId);
    }
}
