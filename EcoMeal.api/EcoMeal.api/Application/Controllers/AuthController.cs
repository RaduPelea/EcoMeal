using EcoMeal.api.Application.Constants;
using EcoMeal.api.Application.Models.Auth;
using EcoMeal.api.Entities;
using EcoMeal.api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly EcomealDbContext _context;
    private readonly IEmailService _emailService;

    public AuthController(UserManager<User> userManager, EcomealDbContext context, IEmailService emailService)
    {
        _userManager = userManager;
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name,
            Contact = request.Contact,
            City = request.City,
            Country = "Romania"
        };

        user.EmailVerificationCode = Random.Shared.Next(100000, 1000000).ToString();
        user.EmailVerificationCodeExpiresAt = AppTime.Now.AddMinutes(15);

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, UserRoles.User);

        await SendVerificationEmailAsync(user);

        return Ok(new { Message = "User registered successfully. Check your email for the verification code." });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return BadRequest("We couldn't find an account with this email.");

        if (user.EmailConfirmed)
            return Ok(new { Message = "Your email is already verified. You can log in." });

        if (user.EmailVerificationCode is null || user.EmailVerificationCodeExpiresAt < AppTime.Now)
            return BadRequest("This code has expired. Request a new one and try again.");

        if (user.EmailVerificationCode != request.Code.Trim())
            return BadRequest("The code is not correct. Check your email and try again.");

        user.EmailConfirmed = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationCodeExpiresAt = null;
        await _userManager.UpdateAsync(user);

        return Ok(new { Message = "Email verified successfully. You can now log in." });
    }

    [HttpPost("resend-code")]
    public async Task<IActionResult> ResendVerificationCode([FromBody] ResendCodeRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return BadRequest("We couldn't find an account with this email.");

        if (user.EmailConfirmed)
            return BadRequest("Your email is already verified. You can log in.");

        user.EmailVerificationCode = Random.Shared.Next(100000, 1000000).ToString();
        user.EmailVerificationCodeExpiresAt = AppTime.Now.AddMinutes(15);
        await _userManager.UpdateAsync(user);

        await SendVerificationEmailAsync(user);

        return Ok(new { Message = "A new code was sent to your email." });
    }

    private Task SendVerificationEmailAsync(User user)
    {
        return _emailService.SendAsync(user.Email!, "Your EcoMeal verification code",
            $"<h2>Hi {user.Name},</h2>" +
            $"<p>Your EcoMeal verification code is:</p>" +
            $"<h1 style=\"letter-spacing: 6px;\">{user.EmailVerificationCode}</h1>" +
            $"<p>The code expires in 15 minutes.</p>");
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var businessName = await _context.Businesses
            .Where(b => b.OwnerId == user.Id)
            .Select(b => b.Name)
            .FirstOrDefaultAsync();

        var myRatings = await _context.CustomerReviews
            .Where(r => r.Order!.UserId == user.Id)
            .Select(r => r.Stars)
            .ToListAsync();

        return Ok(new UserMeResponse
        {
            Email = user.Email,
            Name = user.Name,
            Contact = user.Contact,
            City = user.City,
            Country = user.Country,
            BusinessName = businessName,
            Roles = roles,
            PreferredPackageTypes = user.PreferredPackageTypes,
            CustomerRating = myRatings.Count > 0 ? Math.Round((decimal)myRatings.Average(), 1) : null,
            CustomerRatingCount = myRatings.Count
        });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        user.Name = request.Name;
        user.Contact = request.Contact;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }

    [HttpPut("city")]
    [Authorize]
    public async Task<IActionResult> UpdateCity([FromBody] UpdateCityRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        user.City = request.City;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }

    [HttpGet("users")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<List<AdminUserDTO>>> GetUsers()
    {
        var owners = await _context.Businesses
            .Where(b => b.OwnerId != null)
            .GroupBy(b => b.OwnerId!.Value)
            .Select(g => new
            {
                OwnerId = g.Key,
                Name = g.Select(b => b.Name).FirstOrDefault(),
                Locations = g.Count()
            })
            .ToDictionaryAsync(o => o.OwnerId);

        var users = await _context.Users
            .OrderBy(u => u.Name)
            .Select(u => new AdminUserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                City = u.City,
                Country = u.Country,
                EmailConfirmed = u.EmailConfirmed
            })
            .ToListAsync();

        foreach (var user in users)
        {
            if (owners.TryGetValue(user.Id, out var owned))
            {
                user.IsBusiness = true;
                user.BusinessName = owned.Name;
                user.LocationCount = owned.Locations;
            }
        }

        return Ok(users.OrderByDescending(u => u.IsBusiness).ThenBy(u => u.Name).ToList());
    }

    [HttpPut("preferences")]
    [Authorize]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        user.PreferredPackageTypes = request.PreferredPackageTypes;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }
}
