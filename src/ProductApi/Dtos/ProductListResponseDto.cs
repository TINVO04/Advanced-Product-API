namespace ProductApi.Dtos;

public class ProductListResponseDto
{
    public IReadOnlyList<ProductResponseDto> Items { get; set; } = [];

    public PaginationResponseDto Pagination { get; set; } = new();
}

public class PaginationResponseDto
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}
