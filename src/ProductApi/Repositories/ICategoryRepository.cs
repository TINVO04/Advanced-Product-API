using ProductApi.Models;

namespace ProductApi.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Category?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        string name,
        int? excludedCategoryId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<bool> HasProductsAsync(
        int id,
        CancellationToken cancellationToken);

    Task AddAsync(
        Category category,
        CancellationToken cancellationToken);

    void Remove(Category category);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
