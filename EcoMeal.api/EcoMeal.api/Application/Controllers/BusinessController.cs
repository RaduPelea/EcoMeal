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
                Description = b.Description,
                Contact = b.Contact,
                Rating = b.Rating,
                BusinessTypeName = b.BusinessType!.Name
            })
            .ToListAsync();

        return Ok(businesses);   // 200 OK + lista json
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BusinessDetailsDTO>> GetOneById(int id)
    {
        var business = await _context.Businesses
            .Select(b => new BusinessDetailsDTO
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                Description = b.Description,
                Contact = b.Contact,
                Rating = b.Rating,
                BusinessTypeName = b.BusinessType!.Name
            })
            .FirstOrDefaultAsync(b => b.Id == id);   // ia prima afacere cu id-ul cerut (sau null)

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
            Price = package.Price,
            StartPickup = package.StartPickup,
            EndPickup = package.EndPickup,
            PackageTypeId = package.PackageTypeId,
            BusinessId = id,
        });
        
        await _context.SaveChangesAsync();
        return Created();
    }

}
