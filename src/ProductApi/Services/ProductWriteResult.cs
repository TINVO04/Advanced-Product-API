using ProductApi.Dtos;

namespace ProductApi.Services;

public enum ProductWriteStatus
{
    Success,
    NotFound,
    DuplicateName
}

public class ProductWriteResult
{
    public ProductWriteStatus Status { get; set; }

    public ProductResponseDto? Product { get; set; }
}
