using System.Runtime.InteropServices;
using SharpDX;

namespace PixelGraph.UI.Helix.Shaders
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MinecraftSceneStruct
    {
        public const int SizeInBytes = 4 * (12 + 0);

        public bool EnableLinearSampling;
        public Vector3 SunDirection;
        public float SunStrength;
        public float TimeOfDay;
        public float Wetness;
        public bool EnablePuddles;
        public float ParallaxDepth;
        public int ParallaxSamplesMin;
        public int ParallaxSamplesMax;
        public bool EnableSlopeNormals;
    }
}
