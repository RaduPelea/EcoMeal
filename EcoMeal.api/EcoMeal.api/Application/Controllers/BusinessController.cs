using EcoMeal.api.Application.Models;
using EcoMeal.api.Infrastructure;
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
}
