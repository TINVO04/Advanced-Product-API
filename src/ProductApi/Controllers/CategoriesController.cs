using Microsoft.AspNetCore.Mvc;
using ProductApi.Common.Exceptions;
using ProductApi.Common.Responses;
using ProductApi.Dtos;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<CategoryResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryResponseDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(
            cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<CategoryResponseDto>>
        {
            Success = true,
            Message = "Categories retrieved successfully.",
            Data = categories
        });
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<CategoryResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(
            id,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Category with id {id} was not found.");

        return Ok(new ApiResponse<CategoryResponseDto>
        {
            Success = true,
            Message = "Category retrieved successfully.",
            Data = category
        });
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<CategoryResponseDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Create(
        [FromBody] CategoryCreateDto request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.CreateAsync(
            request,
            cancellationToken);

        var response = new ApiResponse<CategoryResponseDto>
        {
            Success = true,
            Message = "Category created successfully.",
            Data = category
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<CategoryResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Update(
        [FromRoute] int id,
        [FromBody] CategoryUpdateDto request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.UpdateAsync(
            id,
            request,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Category with id {id} was not found.");

        return Ok(new ApiResponse<CategoryResponseDto>
        {
            Success = true,
            Message = "Category updated successfully.",
            Data = category
        });
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _categoryService.DeleteAsync(
            id,
            cancellationToken);

        if (!deleted)
        {
            throw new NotFoundException(
                $"Category with id {id} was not found.");
        }

        return NoContent();
    }
}
