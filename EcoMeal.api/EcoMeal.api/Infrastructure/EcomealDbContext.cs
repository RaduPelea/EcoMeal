using EcoMeal.api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Infrastructure;

public class EcomealDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public EcomealDbContext(DbContextOptions<EcomealDbContext> options) : base(options)
    {
    }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessType> BusinessTypes { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageType> PackageTypes { get; set; }
    public DbSet<Order> Orders { get; set; }
}