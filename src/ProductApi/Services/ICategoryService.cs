using ProductApi.Dtos;

namespace ProductApi.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponseDto>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<CategoryResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<CategoryResponseDto> CreateAsync(
        CategoryCreateDto request,
        CancellationToken cancellationToken);

    Task<CategoryResponseDto?> UpdateAsync(
        int id,
        CategoryUpdateDto request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken);
}
