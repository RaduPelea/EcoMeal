using EcoMeal.api.Application.Constants;
using Microsoft.AspNetCore.DataProtection;
using EcoMeal.api.Infrastructure;
using EcoMeal.api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dp-keys")));

builder.Services.AddOpenApi();

// conectare la sql  prin DbContext
builder.Services.AddDbContext<EcomealDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentityApiEndpoints<User>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        // enforced only when a real SMTP server is configured, otherwise
        // nobody could receive the code and new accounts would be locked out
        options.SignIn.RequireConfirmedEmail = builder.Environment.IsDevelopment()
                                               || !string.IsNullOrEmpty(builder.Configuration["Email:Host"]);
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<EcomealDbContext>();
builder.Services.AddControllers();
builder.Services.AddHostedService<EndingSoonDiscountService>();
builder.Services.AddHttpClient<IGeocodingService, GeocodingService>();
builder.Services.AddHostedService<GeocodeBackfillService>();
if (string.IsNullOrEmpty(builder.Configuration["Email:Host"]))
    builder.Services.AddScoped<IEmailService, DevEmailService>();
else
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
var allowedOrigins = builder.Configuration["AllowedOrigins"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:5044", "https://localhost:7032"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorSite", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //pagina swagger, citeste json generat de mapopenapi
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "EcoMeal API");
    });
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                       | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
});
app.UseHttpsRedirection();
app.UseStaticFiles();

// leaga http de controlere
app.UseCors("AllowBlazorSite");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapIdentityApi<User>();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EcomealDbContext>();
    db.Database.Migrate();

    if (!db.BusinessTypes.Any())
    {
        db.BusinessTypes.AddRange(
            new BusinessType { Name = "Fast Food" },
            new BusinessType { Name = "Bakery" },
            new BusinessType { Name = "Fine Dining" });
    }

    if (!db.PackageTypes.Any())
    {
        db.PackageTypes.AddRange(
            new PackageType { Name = "Meal" },
            new PackageType { Name = "Bakery" },
            new PackageType { Name = "Groceries" });
    }

    await db.SaveChangesAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var roles = new[] { UserRoles.Admin, UserRoles.User, UserRoles.Partner };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
        }
    }
}
app.Run();

