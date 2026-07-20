using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        int? categoryId,
        string sortBy,
        string sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = BuildQuery(search, categoryId)
            .Include(product => product.Category)
            .AsNoTracking();

        var sortedQuery = ApplySorting(
            query,
            sortBy,
            sortOrder);

        return await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        string? search,
        int? categoryId,
        CancellationToken cancellationToken)
    {
        return BuildQuery(search, categoryId).CountAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .Include(product => product.Category)
            .FirstOrDefaultAsync(
                product => product.Id == id && !product.IsDeleted,
                cancellationToken);
    }

    public Task<Product?> GetDeletedByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .Include(product => product.Category)
            .FirstOrDefaultAsync(
                product => product.Id == id && product.IsDeleted,
                cancellationToken);
    }

    public Task<bool> ExistsByNameAndCategoryAsync(
        string name,
        int categoryId,
        int? excludedProductId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products.AnyAsync(
            product =>
                !product.IsDeleted
                && product.Name == name
                && product.CategoryId == categoryId
                && (!excludedProductId.HasValue
                    || product.Id != excludedProductId.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    public void SoftDelete(Product product)
    {
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
    }

    public void Restore(Product product)
    {
        product.IsDeleted = false;
        product.DeletedAt = null;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Product> BuildQuery(string? search, int? categoryId)
    {
        IQueryable<Product> query = _dbContext.Products
            .Where(product => !product.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();

            query = query.Where(product =>
                EF.Functions.ILike(
                    product.Name,
                    $"%{trimmedSearch}%"));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product =>
                product.CategoryId == categoryId.Value);
        }

        return query;
    }

    private static IOrderedQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder == "desc";

        return (sortBy, isDescending) switch
        {
            ("name", false) => query
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id),

            ("name", true) => query
                .OrderByDescending(product => product.Name)
                .ThenBy(product => product.Id),

            ("price", false) => query
                .OrderBy(product => product.Price)
                .ThenBy(product => product.Id),

            ("price", true) => query
                .OrderByDescending(product => product.Price)
                .ThenBy(product => product.Id),

            ("quantity", false) => query
                .OrderBy(product => product.Quantity)
                .ThenBy(product => product.Id),

            ("quantity", true) => query
                .OrderByDescending(product => product.Quantity)
                .ThenBy(product => product.Id),

            ("id", true) => query.OrderByDescending(product => product.Id),

            _ => query.OrderBy(product => product.Id)
        };
    }
}
