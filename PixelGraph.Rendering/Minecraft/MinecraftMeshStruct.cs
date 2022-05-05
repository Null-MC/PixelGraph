using SharpDX;
using System.Runtime.InteropServices;

namespace PixelGraph.Rendering.Minecraft
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MinecraftMeshStruct
    {
        public const int SizeInBytes = 4 * (9 + 3);

        public int BlendMode;
        public Vector3 TintColor;
        public bool EnableLinearSampling;
        public float ParallaxDepth;
        public int ParallaxSamples;
        public bool EnableSlopeNormals;
        public int WaterMode;
        public float SubSurfaceBlur;
    }
}
