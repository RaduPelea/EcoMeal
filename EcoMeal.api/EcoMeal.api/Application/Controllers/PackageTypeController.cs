using EcoMeal.api.Application.Models;
using EcoMeal.api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackageTypeController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public PackageTypeController(EcomealDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PackageTypeDTO>>> GetAll()
    {
        var types = await _context.PackageTypes
            .Select(t => new PackageTypeDTO { Id = t.Id, Name = t.Name })
            .ToListAsync();

        return Ok(types);
    }
}
