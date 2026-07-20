using System.ComponentModel.DataAnnotations;
using ProductApi.Common.Utilities;

namespace ProductApi.Common.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NormalizedStringLengthAttribute : ValidationAttribute
{
    public NormalizedStringLengthAttribute(int minimumLength, int maximumLength)
    {
        MinimumLength = minimumLength;
        MaximumLength = maximumLength;
    }

    public int MinimumLength { get; }

    public int MaximumLength { get; }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is not string text)
        {
            return false;
        }

        var normalizedValue = TextNormalizer.NormalizeWhitespace(text);

        return normalizedValue.Length >= MinimumLength
            && normalizedValue.Length <= MaximumLength;
    }
}
