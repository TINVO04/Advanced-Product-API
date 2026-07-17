using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductApi.Dtos;
using ProductApi.Models;
using ProductApi.Repositories;

namespace ProductApi.Services;

public class ProductService : IProductService
{
    private const string ProductNameCategoryConstraint =
        "IX_Products_Name_CategoryId";

    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductListResponseDto> GetAllAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(
            search,
            page,
            pageSize,
            cancellationToken);

        var totalItems = await _productRepository.CountAsync(
            search,
            cancellationToken);

        return new ProductListResponseDto
        {
            Items = products
                .Select(ToResponseDto)
                .ToList(),
            Pagination = new PaginationResponseDto
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(
                    totalItems / (double)pageSize)
            }
        };
    }

    public async Task<ProductResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        return product is null
            ? null
            : ToResponseDto(product);
    }

    public async Task<ProductWriteResult> CreateAsync(
        ProductCreateDto request,
        CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeName(request.Name);

        var isDuplicate =
            await _productRepository.ExistsByNameAndCategoryAsync(
                normalizedName,
                request.CategoryId,
                excludedProductId: null,
                cancellationToken);

        if (isDuplicate)
        {
            return DuplicateNameResult();
        }

        var product = new Product
        {
            Name = normalizedName,
            CategoryId = request.CategoryId,
            Price = request.Price,
            Quantity = request.Quantity
        };

        await _productRepository.AddAsync(
            product,
            cancellationToken);

        try
        {
            await _productRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateNameException(exception))
        {
            return DuplicateNameResult();
        }

        return SuccessResult(product);
    }

    public async Task<ProductWriteResult> UpdateAsync(
        int id,
        ProductUpdateDto request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return NotFoundResult();
        }

        var normalizedName = NormalizeName(request.Name);

        var isDuplicate =
            await _productRepository.ExistsByNameAndCategoryAsync(
                normalizedName,
                request.CategoryId,
                excludedProductId: id,
                cancellationToken);

        if (isDuplicate)
        {
            return DuplicateNameResult();
        }

        product.Name = normalizedName;
        product.CategoryId = request.CategoryId;
        product.Price = request.Price;
        product.Quantity = request.Quantity;

        try
        {
            await _productRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateNameException(exception))
        {
            return DuplicateNameResult();
        }

        return SuccessResult(product);
    }

    public async Task<ProductWriteResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return NotFoundResult();
        }

        _productRepository.Remove(product);

        await _productRepository.SaveChangesAsync(cancellationToken);

        return SuccessResult(product);
    }

    private static ProductResponseDto ToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            Price = product.Price,
            Quantity = product.Quantity
        };
    }

    private static string NormalizeName(string name)
    {
        return string.Join(
            ' ',
            name.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool IsDuplicateNameException(
        DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState
                == PostgresErrorCodes.UniqueViolation
            && postgresException.ConstraintName
                == ProductNameCategoryConstraint;
    }

    private static ProductWriteResult SuccessResult(Product product)
    {
        return new ProductWriteResult
        {
            Status = ProductWriteStatus.Success,
            Product = ToResponseDto(product)
        };
    }

    private static ProductWriteResult NotFoundResult()
    {
        return new ProductWriteResult
        {
            Status = ProductWriteStatus.NotFound
        };
    }

    private static ProductWriteResult DuplicateNameResult()
    {
        return new ProductWriteResult
        {
            Status = ProductWriteStatus.DuplicateName
        };
    }
}
