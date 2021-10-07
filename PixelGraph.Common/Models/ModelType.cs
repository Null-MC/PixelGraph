using System;

namespace PixelGraph.Common.Models
{
    public static class ModelType
    {
        public const string Cube = "cube";
        public const string Cross = "cross";
        public const string Plane = "plane";
        public const string Bell = "bell";
        public const string File = "file";


        public static bool Is(in string actualType, in string expectedType)
        {
            return string.Equals(expectedType, actualType, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
