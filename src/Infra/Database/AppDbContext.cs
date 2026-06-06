using ECommerce.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infra.Database;

internal class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasKey(p => p.ProductId);
    }
}
