namespace PixelGraph.UI.Internal.Extensions;

internal static class StringExtensions
{
    public static string? NullIfEmpty(this string text, string? nullValue = null)
    {
        return !string.IsNullOrEmpty(text) ? text : nullValue;
    }

    public static string? NullIfWhitespace(this string? text, string? nullValue = null)
    {
        return !string.IsNullOrWhiteSpace(text) ? text : nullValue;
    }
}
