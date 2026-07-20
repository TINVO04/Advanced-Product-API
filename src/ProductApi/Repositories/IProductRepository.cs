using ProductApi.Models;

namespace ProductApi.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        int? categoryId,
        string sortBy,
        string sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? search,
        int? categoryId,
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<Product?> GetDeletedByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAndCategoryAsync(
        string name,
        int categoryId,
        int? excludedProductId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken);

    void SoftDelete(Product product);

    void Restore(Product product);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
