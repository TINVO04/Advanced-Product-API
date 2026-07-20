using ProductApi.Common.Responses;
using ProductApi.Dtos;

namespace ProductApi.Services;

public interface IProductService
{
    Task<PagedResult<ProductResponseDto>> GetAllAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken);

    Task<ProductResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<ProductWriteResult> CreateAsync(
        ProductCreateDto request,
        CancellationToken cancellationToken);

    Task<ProductWriteResult> UpdateAsync(
        int id,
        ProductUpdateDto request,
        CancellationToken cancellationToken);

    Task<ProductWriteResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken);
}
