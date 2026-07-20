using System.ComponentModel.DataAnnotations;
using ProductApi.Common.Validation;

namespace ProductApi.Dtos;

public class CategoryCreateDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [NormalizedStringLength(
        2,
        100,
        ErrorMessage = "Category name must be between 2 and 100 characters after whitespace is normalized.")]
    public string Name { get; set; } = string.Empty;
}
