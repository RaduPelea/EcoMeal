using EcoMeal.api.Application.Constants;
using EcoMeal.api.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _environment;

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Partner)]
    public async Task<ActionResult<UploadResultDTO>> UploadImage(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file was uploaded");

        if (file.Length > MaxFileSize)
            return BadRequest("The file is larger than 5 MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest("Only jpg, jpeg, png and webp images are allowed");

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        await using (var stream = System.IO.File.Create(Path.Combine(uploadsFolder, fileName)))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new UploadResultDTO
        {
            Url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}"
        });
    }
}
