using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");

        var productEntity = modelBuilder.Entity<Product>();

        productEntity.ToTable("Products");

        productEntity.HasKey(product => product.Id);

        productEntity
            .Property(product => product.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 4);

        productEntity
            .Property(product => product.Name)
            .HasColumnType("citext")
            .HasMaxLength(100)
            .IsRequired();

        productEntity
            .Property(product => product.Price)
            .HasPrecision(18, 2);

        productEntity
            .HasIndex(product => new
            {
                product.Name,
                product.CategoryId
            })
            .IsUnique()
            .HasDatabaseName("IX_Products_Name_CategoryId");

        productEntity.HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop",
                CategoryId = 1,
                Price = 1500m,
                Quantity = 10
            },
            new Product
            {
                Id = 2,
                Name = "Smartphone",
                CategoryId = 1,
                Price = 800m,
                Quantity = 20
            },
            new Product
            {
                Id = 3,
                Name = "Headphones",
                CategoryId = 2,
                Price = 120m,
                Quantity = 30
            });
    }
}
