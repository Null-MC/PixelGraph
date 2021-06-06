using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Internal.Utilities;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    internal static class CustomPSShaderDescriptions
    {
        public static ShaderDescription PbrSpecular = new(nameof(PbrSpecular), ShaderStage.Pixel,
            new ShaderReflector(), ResourceLoader.GetShaderByteCode("ps_pbr_specular"));
    }
}
