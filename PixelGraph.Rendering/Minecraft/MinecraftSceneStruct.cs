using System.Runtime.InteropServices;
using SharpDX;

namespace PixelGraph.Rendering.Minecraft
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MinecraftSceneStruct
    {
        public const int SizeInBytes = 4 * (10 + 2);

        public bool EnableAtmosphere;
        public Vector3 SunDirection;
        public float SunStrength;
        public float TimeOfDay;
        public bool EnableLinearSampling;
        public float Wetness;
        //public int WaterMode;
        public bool EnableSlopeNormals;
        //public float ParallaxDepth;
        //public int ParallaxSamples;
        public float ErpExposure;
    }
}
