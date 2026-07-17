namespace ProductApi.Common.Exceptions;

public sealed class BadRequestException : Exception
{
    public BadRequestException(
        string message,
        object? errors = null)
        : base(message)
    {
        Errors = errors;
    }

    public object? Errors { get; }
}
