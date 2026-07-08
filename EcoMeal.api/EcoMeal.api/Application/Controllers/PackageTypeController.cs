using EcoMeal.api.Application.Models;
using EcoMeal.api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]   // ruta: api/packagetype
public class PackageTypeController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public PackageTypeController(EcomealDbContext context)
    {
        _context = context;
    }

    // GET api/packagetype -> lista tuturor tipurilor de pachet (pentru dropdown)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PackageTypeDTO>>> GetAll()
    {
        var types = await _context.PackageTypes
            .Select(t => new PackageTypeDTO { Id = t.Id, Name = t.Name })
            .ToListAsync();

        return Ok(types);
    }
}
