using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO
{
    public static class GameEdition
    {
        public const string Java = "java";
        public const string Bedrock = "bedrock";


        public static GameEditions Parse(string text)
        {
            return map.TryGetValue(text, out var value) ? value : throw new ApplicationException($"Unknown game edition '{text}'!");
        }

        public static bool Is(string actual, string expected)
        {
            return string.Equals(expected, actual, StringComparison.InvariantCultureIgnoreCase);
        }

        private static readonly Dictionary<string, GameEditions> map = new(StringComparer.InvariantCultureIgnoreCase) {
            [Java] = GameEditions.Java,
            [Bedrock] = GameEditions.Bedrock,
        };
    }
}
