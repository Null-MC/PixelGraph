using System.Runtime.InteropServices;
using SharpDX;

namespace PixelGraph.Rendering.Minecraft
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MinecraftMeshStruct
    {
        public const int SizeInBytes = 4 * (7 + 1);

        public int BlendMode;
        public Vector3 TintColor;
        public float ParallaxDepth;
        public int ParallaxSamples;
        public int WaterMode;
    }
}
