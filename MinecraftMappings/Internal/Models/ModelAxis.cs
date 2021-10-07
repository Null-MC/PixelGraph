using System;

namespace MinecraftMappings.Internal.Models
{
    public static class ModelAxiss
    {
        public static bool TryParse(string axisName, out ModelAxis axis) => Enum.TryParse(axisName, true, out axis);
    }

    public enum ModelAxis
    {
        X,
        Y,
        Z,
    }
}
