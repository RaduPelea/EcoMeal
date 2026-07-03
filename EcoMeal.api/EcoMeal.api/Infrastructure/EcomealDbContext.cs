using EcoMeal.api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Infrastructure;

public class EcomealDbContext : DbContext
{
    public EcomealDbContext(DbContextOptions<EcomealDbContext> options) : base(options)
    {}
    public DbSet<User> User { get; set; }

}