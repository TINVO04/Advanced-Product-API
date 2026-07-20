namespace ProductApi.Common.Utilities;

public static class TextNormalizer
{
    public static string NormalizeWhitespace(string value)
    {
        return string.Join(
            ' ',
            value.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries));
    }
}
