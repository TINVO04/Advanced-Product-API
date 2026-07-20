using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductApi.Common.Responses;
using ProductApi.Dtos;
using ProductApi.Models;
using ProductApi.Repositories;

namespace ProductApi.Services;

public class ProductService : IProductService
{
    private const string ProductNameCategoryConstraint =
        "IX_Products_Name_CategoryId";

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResult<ProductResponseDto>> GetAllAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(
            query.Search,
            query.CategoryId,
            query.SortBy,
            query.SortOrder,
            query.Page,
            query.PageSize,
            cancellationToken);

        var totalItems = await _productRepository.CountAsync(
            query.Search,
            query.CategoryId,
            cancellationToken);

        return new PagedResult<ProductResponseDto>
        {
            Items = products
                .Select(ToResponseDto)
                .ToList(),
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(
                totalItems / (double)query.PageSize)
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
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category is null)
        {
            return CategoryNotFoundResult();
        }

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
            Category = category,
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

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category is null)
        {
            return CategoryNotFoundResult();
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
        product.Category = category;
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

        _productRepository.SoftDelete(product);

        await _productRepository.SaveChangesAsync(cancellationToken);

        return SuccessResult(product);
    }

    public async Task<ProductWriteResult> RestoreAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetDeletedByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return NotFoundResult();
        }

        var categoryExists = await _categoryRepository.ExistsByIdAsync(
            product.CategoryId,
            cancellationToken);

        if (!categoryExists)
        {
            return CategoryNotFoundResult();
        }

        var isDuplicate =
            await _productRepository.ExistsByNameAndCategoryAsync(
                product.Name,
                product.CategoryId,
                excludedProductId: product.Id,
                cancellationToken);

        if (isDuplicate)
        {
            return DuplicateNameResult();
        }

        _productRepository.Restore(product);

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

    private static ProductResponseDto ToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
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

    private static ProductWriteResult CategoryNotFoundResult()
    {
        return new ProductWriteResult
        {
            Status = ProductWriteStatus.CategoryNotFound
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
