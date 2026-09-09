using Microsoft.EntityFrameworkCore;
using OrderApi.Data.Entities;

namespace OrderApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(o => o.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(o => o.Price).HasColumnType("decimal(18,2)");
            entity.Property(o => o.Status).IsRequired().HasMaxLength(50);
        });

        base.OnModelCreating(modelBuilder);
    }
}