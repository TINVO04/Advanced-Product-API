using Microsoft.AspNetCore.Mvc;
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
        typeof(ProductListResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductListResponseDto>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return BadRequest(new
            {
                message = "Page must be greater than or equal to 1."
            });
        }

        var response = await _productService.GetAllAsync(
            search,
            page,
            PageSize,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ProductResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return ProductNotFound(id);
        }

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ProductResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponseDto>> Create(
        [FromBody] ProductCreateDto request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(
            request,
            cancellationToken);

        if (result.Status == ProductWriteStatus.DuplicateName)
        {
            return DuplicateNameConflict(request.CategoryId);
        }

        var product = result.Product
            ?? throw new InvalidOperationException(
                "The created product response was not available.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ProductResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponseDto>> Update(
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
            return ProductNotFound(id);
        }

        if (result.Status == ProductWriteStatus.DuplicateName)
        {
            return DuplicateNameConflict(request.CategoryId);
        }

        var product = result.Product
            ?? throw new InvalidOperationException(
                "The updated product response was not available.");

        return Ok(product);
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
