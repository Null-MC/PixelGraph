using System;

namespace PixelGraph.Tests.Internal.Extensions
{
    internal static class StringExtensions
    {
        public static string TrimStart(this string text, string trimText, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return !text.StartsWith(trimText, comparison) ? text : text[trimText.Length..];
        }
    }
}
