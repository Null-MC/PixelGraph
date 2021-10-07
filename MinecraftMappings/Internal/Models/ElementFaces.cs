using System;

namespace MinecraftMappings.Internal.Models
{
    public static class ElementFace
    {
        public static bool TryParse(string name, out ElementFaces face) => Enum.TryParse(name, true, out face);
    }

    public enum ElementFaces
    {
        Up,
        Down,
        North,
        South,
        West,
        East,
    }
}
