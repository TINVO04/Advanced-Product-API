using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductApi.Common.Exceptions;
using ProductApi.Common.Utilities;
using ProductApi.Dtos;
using ProductApi.Models;
using ProductApi.Repositories;

namespace ProductApi.Services;

public class CategoryService : ICategoryService
{
    private const string CategoryNameConstraint =
        "IX_Categories_Name";

    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryResponseDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(
            cancellationToken);

        return categories
            .Select(ToResponseDto)
            .ToList();
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id,
            cancellationToken);

        return category is null
            ? null
            : ToResponseDto(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(
        CategoryCreateDto request,
        CancellationToken cancellationToken)
    {
        var normalizedName = TextNormalizer.NormalizeWhitespace(request.Name);

        await EnsureNameIsUniqueAsync(
            normalizedName,
            excludedCategoryId: null,
            cancellationToken);

        var category = new Category
        {
            Name = normalizedName
        };

        await _categoryRepository.AddAsync(
            category,
            cancellationToken);

        await SaveChangesAsync(cancellationToken);

        return ToResponseDto(category);
    }

    public async Task<CategoryResponseDto?> UpdateAsync(
        int id,
        CategoryUpdateDto request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (category is null)
        {
            return null;
        }

        var normalizedName = TextNormalizer.NormalizeWhitespace(request.Name);

        await EnsureNameIsUniqueAsync(
            normalizedName,
            excludedCategoryId: id,
            cancellationToken);

        category.Name = normalizedName;

        await SaveChangesAsync(cancellationToken);

        return ToResponseDto(category);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (category is null)
        {
            return false;
        }

        var hasProducts = await _categoryRepository.HasProductsAsync(
            id,
            cancellationToken);

        if (hasProducts)
        {
            throw new ConflictException(
                "Category cannot be deleted because it still has products.");
        }

        _categoryRepository.SoftDelete(category);

        await _categoryRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureNameIsUniqueAsync(
        string name,
        int? excludedCategoryId,
        CancellationToken cancellationToken)
    {
        var exists = await _categoryRepository.ExistsByNameAsync(
            name,
            excludedCategoryId,
            cancellationToken);

        if (exists)
        {
            throw new ConflictException(
                $"A category with the name '{name}' already exists.");
        }
    }

    private async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _categoryRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateNameException(exception))
        {
            throw new ConflictException(
                "A category with the same name already exists.");
        }
    }

    private static bool IsDuplicateNameException(
        DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState
                == PostgresErrorCodes.UniqueViolation
            && postgresException.ConstraintName
                == CategoryNameConstraint;
    }

    private static CategoryResponseDto ToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

}
