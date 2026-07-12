using EcoMeal.api.Application.Models;
using EcoMeal.api.Infrastructure;
using EcoMeal.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]   // ruta devine: api/business  ([controller] = "Business" fără sufixul Controller)
public class BusinessController : ControllerBase
{
    private readonly EcomealDbContext _context;

    // DbContextvine automat
    public BusinessController(EcomealDbContext context)
    {
        _context = context;
    }

    // read -> get api/business
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessDTO>>> GetAll()
    {
        var businesses = await _context.Businesses
            .Include(b => b.BusinessType)          // aduce și tipul afacerii 
            .Select(b => new BusinessDTO           // transforma entitatea în DTO
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                City = b.City,
                ImageUrl = b.ImageUrl,
                Description = b.Description,
                Contact = b.Contact,
                Rating = b.Rating,
                BusinessTypeName = b.BusinessType!.Name
            })
            .ToListAsync();

        return Ok(businesses);   // 200 OK + lista json
    }

    // distinct cities that have at least one business (for the city search)
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
                ImageUrl = b.ImageUrl,
                Description = b.Description,
                Contact = b.Contact,
                Rating = b.Rating,
                BusinessTypeName = b.BusinessType!.Name,
                // include the packages
                Packages = b.Packages.Select(p => new PackageDTO
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
                }).ToList()
            })
            .FirstOrDefaultAsync();   // business with the given id (or null)

        if (business is null)
            return NotFound();

        return Ok(business);
    }

    // delete  api/business/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business is null)
            return NotFound();          // nu exista

        _context.Businesses.Remove(business);
        await _context.SaveChangesAsync();
        return NoContent();             // nimc de returnat
    }

    [HttpPost]
    [Route("{id}/AddPackage")]
    public async Task<IActionResult> AddPackageToBusiness(int id, [FromBody] packageAddDTO package)
    {
        _context.Packages.Add(new Package
        {
            Name = package.Name,
            Description = package.Description,
            ImageUrl = package.ImageUrl,
            Price = package.Price,
            StartPickup = package.StartPickup,
            EndPickup = package.EndPickup,
            PackageTypeId = package.PackageTypeId,
            BusinessId = id,
        });
        
        await _context.SaveChangesAsync();
        return Created();
    }

    // create a business
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BusinessAddDTO dto)
    {
        _context.Businesses.Add(new Business
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            ImageUrl = dto.ImageUrl,
            Description = dto.Description,
            Contact = dto.Contact,
            Rating = dto.Rating,
            BusinessTypeId = dto.BusinessTypeId
        });

        await _context.SaveChangesAsync();
        return Created();
    }

    // update a business
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BusinessAddDTO dto)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business is null)
            return NotFound();

        business.Name = dto.Name;
        business.Address = dto.Address;
        business.City = dto.City;
        business.ImageUrl = dto.ImageUrl;
        business.Description = dto.Description;
        business.Contact = dto.Contact;
        business.Rating = dto.Rating;
        business.BusinessTypeId = dto.BusinessTypeId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // business data for the edit form
    [HttpGet("{id}/edit")]
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
                Rating = b.Rating,
                BusinessTypeId = b.BusinessTypeId
            })
            .FirstOrDefaultAsync();

        if (business is null)
            return NotFound();

        return Ok(business);
    }

}
