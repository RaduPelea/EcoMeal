using EcoMeal.api.Application.Models;
using EcoMeal.api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessTypeController : ControllerBase
{
    private readonly EcomealDbContext _context;

    public BusinessTypeController(EcomealDbContext context)
    {
        _context = context;
    }

    // GET all business types (for the dropdown)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessTypeDTO>>> GetAll()
    {
        var types = await _context.BusinessTypes
            .Select(t => new BusinessTypeDTO { Id = t.Id, Name = t.Name })
            .ToListAsync();

        return Ok(types);
    }
}
