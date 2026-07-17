using Microsoft.AspNetCore.Mvc;
using ProductApi.Common.Exceptions;
using ProductApi.Common.Responses;
using ProductApi.Dtos;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private const int PageSize = 2;

    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<ProductListResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<ProductListResponseDto>),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductListResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            var errorResponse = new ApiResponse<ProductListResponseDto>
            {
                Success = false,
                Message = "Validation failed.",
                Errors = new
                {
                    page = new[]
                    {
                        "Page must be greater than or equal to 1."
                    }
                }
            };

            return BadRequest(errorResponse);
        }

        var products = await _productService.GetAllAsync(
            search,
            page,
            PageSize,
            cancellationToken);

        var apiResponse = new ApiResponse<ProductListResponseDto>
        {
            Success = true,
            Message = "Products retrieved successfully.",
            Data = products
        };

        return Ok(apiResponse);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<ProductResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(
            id,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Product with id {id} was not found.");

        var response = new ApiResponse<ProductResponseDto>
        {
            Success = true,
            Message = "Product retrieved successfully.",
            Data = product
        };

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<ProductResponseDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> Create(
        [FromBody] ProductCreateDto request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(
            request,
            cancellationToken);

        if (result.Status == ProductWriteStatus.DuplicateName)
        {
            throw new ConflictException(
                "A product with the same name already exists "
                + $"in category {request.CategoryId}.");
        }

        var product = result.Product
            ?? throw new InvalidOperationException(
                "The created product response was not available.");

        var response = new ApiResponse<ProductResponseDto>
        {
            Success = true,
            Message = "Product created successfully.",
            Data = product
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<ProductResponseDto>),
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
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> Update(
        [FromRoute] int id,
        [FromBody] ProductUpdateDto request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (result.Status == ProductWriteStatus.NotFound)
        {
            throw new NotFoundException(
                $"Product with id {id} was not found.");
        }

        if (result.Status == ProductWriteStatus.DuplicateName)
        {
            throw new ConflictException(
                "A product with the same name already exists "
                + $"in category {request.CategoryId}.");
        }

        var product = result.Product
            ?? throw new InvalidOperationException(
                "The updated product response was not available.");

        var response = new ApiResponse<ProductResponseDto>
        {
            Success = true,
            Message = "Product updated successfully.",
            Data = product
        };

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(
            id,
            cancellationToken);

        if (result.Status == ProductWriteStatus.NotFound)
        {
            return ProductNotFound(id);
        }

        return NoContent();
    }

    private NotFoundObjectResult ProductNotFound(int id)
    {
        return NotFound(new
        {
            message = $"Product with id {id} was not found."
        });
    }

    private ConflictObjectResult DuplicateNameConflict(
        int categoryId)
    {
        return Conflict(new
        {
            message =
                "A product with the same name already exists "
                + $"in category {categoryId}."
        });
    }
}
