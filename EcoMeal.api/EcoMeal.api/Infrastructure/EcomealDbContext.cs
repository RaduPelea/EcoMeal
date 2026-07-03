using EcoMeal.api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Infrastructure;

public class EcomealDbContext : DbContext
{
    public EcomealDbContext(DbContextOptions<EcomealDbContext> options) : base(options)
    { }

    
    public DbSet<User> Users { get; set; }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessType> BusinessTypes { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageType> PackageTypes { get; set; }
    public DbSet<Order> Orders { get; set; }
}
