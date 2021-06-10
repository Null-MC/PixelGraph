using SharpDX;
using System.Runtime.InteropServices;

namespace PixelGraph.UI.Internal.Preview.Shaders
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MinecraftSceneStruct
    {
        public const int SizeInBytes = 4 * 8;

        public float TimeOfDay;
        public Vector3 SunDirection;
        public float Wetness;
        public float ParallaxDepth;
        public int ParallaxSamplesMin;
        public int ParallaxSamplesMax;
    }
}
