using System;

namespace PixelGraph.Common.IO
{
    public static class GameEdition
    {
        public const string Java = "java";
        public const string Bedrock = "bedrock";


        public static bool Is(string actual, string expected)
        {
            return string.Equals(expected, actual, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
