using System.Runtime.InteropServices;
using SharpDX;

namespace PixelGraph.Rendering.Minecraft
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MinecraftSceneStruct
    {
        public const int SizeInBytes = 4 * (13 + 3);

        public bool EnableAtmosphere;
        public Vector3 SunDirection;
        public float SunStrength;
        public float TimeOfDay;
        public bool EnableLinearSampling;
        public float Wetness;
        public int WaterMode;
        public bool EnableSlopeNormals;
        //public bool padding;
        public float ParallaxDepth;
        public int ParallaxSamplesMin;
        public int ParallaxSamplesMax;
    }
}
