using SharpDX;
using System.Runtime.InteropServices;

namespace PixelGraph.UI.Internal.Preview.Shaders
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MinecraftSceneStruct
    {
        public const int SizeInBytes = 4 * (9 + 3);

        public Vector3 SunDirection;
        public float SunStrength;
        public float TimeOfDay;
        public float Wetness;
        public float ParallaxDepth;
        public int ParallaxSamplesMin;
        public int ParallaxSamplesMax;
        public Vector3 padding;
    }
}
