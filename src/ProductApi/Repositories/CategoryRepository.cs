using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Category?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Categories
            .FirstOrDefaultAsync(
                category => category.Id == id,
                cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        int? excludedCategoryId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Categories.AnyAsync(
            category => category.Name == name
                && (!excludedCategoryId.HasValue
                    || category.Id != excludedCategoryId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Categories.AnyAsync(
            category => category.Id == id,
            cancellationToken);
    }

    public Task<bool> HasProductsAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products.AnyAsync(
            product => product.CategoryId == id,
            cancellationToken);
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken)
    {
        await _dbContext.Categories.AddAsync(
            category,
            cancellationToken);
    }

    public void Remove(Category category)
    {
        _dbContext.Categories.Remove(category);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
