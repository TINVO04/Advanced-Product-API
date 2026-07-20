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

    public DbSet<Category> Categories => Set<Category>();

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");

        var categoryEntity = modelBuilder.Entity<Category>();

        categoryEntity.ToTable("Categories");

        categoryEntity.HasKey(category => category.Id);

        categoryEntity
            .Property(category => category.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 4);

        categoryEntity
            .Property(category => category.Name)
            .HasColumnType("citext")
            .HasMaxLength(100)
            .IsRequired();

        categoryEntity
            .HasIndex(category => category.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_Categories_Name");

        categoryEntity
            .Property(category => category.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        categoryEntity
            .Property(category => category.IsDeleted)
            .HasDefaultValue(false);

        categoryEntity.HasData(
            new Category
            {
                Id = 1,
                Name = "Electronics"
            },
            new Category
            {
                Id = 2,
                Name = "Audio"
            },
            new Category
            {
                Id = 3,
                Name = "Accessories"
            });

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
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_Products_Name_CategoryId");

        productEntity
            .HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        productEntity
            .Property(product => product.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        productEntity
            .Property(product => product.IsDeleted)
            .HasDefaultValue(false);

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

    private void ApplyAuditFields()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = null;
                entry.Entity.DeletedAt = null;
                entry.Entity.IsDeleted = false;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
