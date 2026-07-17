using System.Security.Claims;
using EcoMeal.api.Application.Constants;
using EcoMeal.api.Application.Models;
using EcoMeal.api.Entities;
using EcoMeal.api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessController : ControllerBase
{
    private readonly EcomealDbContext _context;
    private readonly UserManager<User> _userManager;

    private readonly IGeocodingService _geocoder;

    public BusinessController(EcomealDbContext context, UserManager<User> userManager, IGeocodingService geocoder)
    {
        _context = context;
        _userManager = userManager;
        _geocoder = geocoder;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessDTO>>> GetAll()
    {
        var businesses = await _context.Businesses
            .Select(b => new BusinessDTO
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                ImageUrl = b.ImageUrl,
                Description = b.Description,
                Contact = b.Contact,
                Rating = b.Reviews.Count != 0 ? Math.Round(b.Reviews.Average(r => (decimal)r.Stars), 1) : 0,
                BusinessTypeName = b.BusinessType!.Name
            })
            .ToListAsync();

        return Ok(businesses);
    }

    [HttpGet("browse")]
    public async Task<ActionResult<BusinessPageDTO>> Browse(
        [FromQuery] string? type,
        [FromQuery] string? city,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _context.Businesses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(b => b.BusinessType!.Name == type);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(b => b.City == city);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BusinessDTO
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                ImageUrl = b.ImageUrl,
                Description = b.Description,
                Contact = b.Contact,
                Rating = b.Reviews.Count != 0 ? Math.Round(b.Reviews.Average(r => (decimal)r.Stars), 1) : 0,
                BusinessTypeName = b.BusinessType!.Name
            })
            .ToListAsync();

        return Ok(new BusinessPageDTO { Items = items, TotalCount = totalCount });
    }

    [HttpGet("cities")]
    public async Task<ActionResult<IEnumerable<string>>> GetCities()
    {
        var cities = await _context.Businesses
            .Select(b => b.City)
            .Where(c => c != null && c != "")
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(cities);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BusinessDetailsDTO>> GetOneById(int id)
    {
        var business = await _context.Businesses
            .Where(b => b.Id == id)
            .Select(b => new BusinessDetailsDTO
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                ImageUrl = b.ImageUrl,
                Description = b.Description,
                Contact = b.Contact,
                Rating = b.Reviews.Count != 0 ? Math.Round(b.Reviews.Average(r => (decimal)r.Stars), 1) : 0,
                BusinessTypeName = b.BusinessType!.Name,
                Packages = b.Packages.Where(p => p.Quantity > 0 && p.EndPickup > AppTime.Now).Select(p => new PackageDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Images = p.Images.Select(i => i.Url).ToList(),
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    Quantity = p.Quantity,
                    StartPickup = p.StartPickup,
                    EndPickup = p.EndPickup,
                    PackageTypeId = p.PackageTypeId,
                    PackageTypeName = p.PackageType!.Name
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (business is null)
            return NotFound();

        return Ok(business);
    }

    [HttpGet("mine")]
    [Authorize(Roles = UserRoles.Partner)]
    public async Task<ActionResult<List<BusinessDetailsDTO>>> GetMine()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var businesses = await _context.Businesses
            .Where(b => b.OwnerId == userId)
            .OrderBy(b => b.City)
            .Select(b => new BusinessDetailsDTO
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                ImageUrl = b.ImageUrl,
                Description = b.Description,
                Contact = b.Contact,
                Rating = b.Reviews.Count != 0 ? Math.Round(b.Reviews.Average(r => (decimal)r.Stars), 1) : 0,
                BusinessTypeName = b.BusinessType!.Name,
                Packages = b.Packages.Select(p => new PackageDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Images = p.Images.Select(i => i.Url).ToList(),
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    Quantity = p.Quantity,
                    StartPickup = p.StartPickup,
                    EndPickup = p.EndPickup,
                    PackageTypeId = p.PackageTypeId,
                    PackageTypeName = p.PackageType!.Name
                }).ToList()
            })
            .ToListAsync();

        return Ok(businesses);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult> Delete(int id)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business is null)
            return NotFound("This business no longer exists.");

        var hasActiveOrders = await _context.Orders.AnyAsync(o =>
            o.Package!.BusinessId == id &&
            (o.Status == OrderStatus.New || o.Status == OrderStatus.Confirmed));

        if (hasActiveOrders)
            return BadRequest("This business has active orders and can't be deleted right now.");

        _context.Businesses.Remove(business);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost]
    [Route("{id}/AddPackage")]
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Partner)]
    public async Task<IActionResult> AddPackageToBusiness(int id, [FromBody] PackageAddDto package)
    {
        var businessExists = await _context.Businesses.AnyAsync(b => b.Id == id);
        if (!businessExists)
            return NotFound("This business no longer exists.");

        if (!await CanManageBusinessAsync(id))
            return BadRequest("You don't have permission to add packages to this business.");

        if (package.DiscountPercent is < 5 or > 90)
            return BadRequest("The discount must be between 5% and 90%");

        if (package.EndPickup <= package.StartPickup)
            return BadRequest("The pickup window must end after it starts");

        if (package.EndPickup <= AppTime.Now.AddMinutes(15))
            return BadRequest("The pickup window must end at least 15 minutes from now — otherwise the package expires instantly and customers never see it");

        var entity = new Package
        {
            Name = package.Name,
            Description = package.Description,
            ImageUrl = package.ImageUrl,
            Price = package.Price,
            Quantity = package.Quantity,
            StartPickup = package.StartPickup,
            EndPickup = package.EndPickup,
            PackageTypeId = package.PackageTypeId,
            BusinessId = id,
            Images = package.ImageUrls.Distinct().Select(url => new PackageImage { Url = url }).ToList(),
            DiscountPercent = package.DiscountPercent,
            DiscountHoursBeforeEnd = package.DiscountPercent is null ? null : package.DiscountHoursBeforeEnd,
        };

        if (entity.DiscountPercent is not null && entity.DiscountHoursBeforeEnd is null)
        {
            entity.OriginalPrice = entity.Price;
            entity.Price = Math.Round(entity.Price * (100 - entity.DiscountPercent.Value) / 100m, 2);
        }

        _context.Packages.Add(entity);

        await _context.SaveChangesAsync();
        return Created();
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] BusinessAddDTO dto)
    {
        var (ownerId, error) = await ResolveOwnerAsync(dto.OwnerEmail);
        if (error is not null)
            return BadRequest(error);

        var coords = await _geocoder.GeocodeAsync(dto.Address, dto.City, "Romania");

        _context.Businesses.Add(new Business
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            ImageUrl = dto.ImageUrl,
            Description = dto.Description,
            Contact = dto.Contact,
            BusinessTypeId = dto.BusinessTypeId,
            OwnerId = ownerId,
            Latitude = coords?.Lat,
            Longitude = coords?.Lon
        });

        await _context.SaveChangesAsync();
        return Created();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] BusinessAddDTO dto)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business is null)
            return NotFound();

        var (ownerId, error) = await ResolveOwnerAsync(dto.OwnerEmail);
        if (error is not null)
            return BadRequest(error);

        if (business.Address != dto.Address || business.City != dto.City || business.Latitude is null)
        {
            var newCoords = await _geocoder.GeocodeAsync(dto.Address, dto.City, business.Country);
            business.Latitude = newCoords?.Lat;
            business.Longitude = newCoords?.Lon;
        }

        business.Name = dto.Name;
        business.Address = dto.Address;
        business.City = dto.City;
        business.ImageUrl = dto.ImageUrl;
        business.Description = dto.Description;
        business.Contact = dto.Contact;
        business.BusinessTypeId = dto.BusinessTypeId;
        business.OwnerId = ownerId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/edit")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<BusinessAddDTO>> GetForEdit(int id)
    {
        var business = await _context.Businesses
            .Where(b => b.Id == id)
            .Select(b => new BusinessAddDTO
            {
                Name = b.Name,
                Address = b.Address,
                City = b.City,
                ImageUrl = b.ImageUrl,
                Description = b.Description,
                Contact = b.Contact,
                BusinessTypeId = b.BusinessTypeId,
                OwnerEmail = b.Owner != null ? b.Owner.Email : null
            })
            .FirstOrDefaultAsync();

        if (business is null)
            return NotFound();

        return Ok(business);
    }

    private async Task<(int? OwnerId, string? Error)> ResolveOwnerAsync(string? ownerEmail)
    {
        if (string.IsNullOrWhiteSpace(ownerEmail))
            return (null, null);

        var owner = await _userManager.FindByEmailAsync(ownerEmail);
        if (owner is null)
            return (null, "No user found with this email");

        if (!await _userManager.IsInRoleAsync(owner, UserRoles.Partner))
            await _userManager.AddToRoleAsync(owner, UserRoles.Partner);

        return (owner.Id, null);
    }

    private async Task<bool> CanManageBusinessAsync(int businessId)
    {
        if (User.IsInRole(UserRoles.Admin))
            return true;

        var userId = GetCurrentUserId();
        if (userId is null)
            return false;

        return await _context.Businesses.AnyAsync(b => b.Id == businessId && b.OwnerId == userId);
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
