namespace ProductApi.Common.Responses;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public object? Errors { get; init; }

    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
}
