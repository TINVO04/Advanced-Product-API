using System.ComponentModel.DataAnnotations;

namespace ProductApi.Dtos;

public class ProductQueryDto
{
    public string? Search { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "CategoryId must be greater than 0.")]
    public int? CategoryId { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; set; } = 1;

    [Range(
        1,
        100,
        ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    [RegularExpression(
        "^(id|name|price|quantity)$",
        ErrorMessage = "SortBy must be one of: id, name, price, quantity.")]
    public string SortBy { get; set; } = "id";

    [RegularExpression(
        "^(asc|desc)$",
        ErrorMessage = "SortOrder must be either asc or desc.")]
    public string SortOrder { get; set; } = "asc";
}
