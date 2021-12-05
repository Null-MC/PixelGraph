using System.Runtime.InteropServices;
using SharpDX;

namespace PixelGraph.Rendering.Minecraft
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MinecraftMeshStruct
    {
        public const int SizeInBytes = 4 * (4 + 0);

        public int BlendMode;
        public Vector3 TintColor;
    }
}
