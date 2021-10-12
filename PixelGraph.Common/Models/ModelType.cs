using System;

namespace PixelGraph.Common.Models
{
    public static class ModelType
    {
        public const string Bell = "bell";
        public const string Boat = "boat";
        public const string Cow = "cow";
        public const string Cube = "cube";
        public const string Plane = "plane";
        public const string Zombie = "zombie";


        public static bool Is(in string actualType, in string expectedType)
        {
            return string.Equals(expectedType, actualType, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
