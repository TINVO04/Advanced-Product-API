using ProductApi.Models;

namespace ProductApi.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? search,
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(
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

    void Remove(Product product);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
