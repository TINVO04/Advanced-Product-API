using System.ComponentModel.DataAnnotations;

namespace ProductApi.Dtos;

public class ProductUpdateDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "Product name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "CategoryId must be greater than 0.")]
    public int CategoryId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "79228162514264337593543950335",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(
        0,
        int.MaxValue,
        ErrorMessage = "Quantity must be greater than or equal to 0.")]
    public int Quantity { get; set; }
}
