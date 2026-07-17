using EcoMeal.api.Entities;
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
    public DbSet<Review> Reviews { get; set; }
    public DbSet<CustomerReview> CustomerReviews { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<PackageImage> PackageImages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Review>()
            .HasIndex(r => new { r.BusinessId, r.UserId })
            .IsUnique();

        builder.Entity<CustomerReview>()
            .HasIndex(r => r.OrderId)
            .IsUnique();

        builder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.BusinessId })
            .IsUnique();
    }
}