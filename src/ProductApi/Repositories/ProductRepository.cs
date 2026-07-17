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
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await BuildQuery(search)
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        return BuildQuery(search).CountAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products.FirstOrDefaultAsync(
            product => product.Id == id,
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
                product.Name == name
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

    public void Remove(Product product)
    {
        _dbContext.Products.Remove(product);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Product> BuildQuery(string? search)
    {
        IQueryable<Product> query = _dbContext.Products;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();

            query = query.Where(product =>
                EF.Functions.ILike(
                    product.Name,
                    $"%{trimmedSearch}%"));
        }

        return query;
    }
}
