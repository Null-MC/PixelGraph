using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Internal.Utilities;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    internal static class CustomVSShaderDescriptions
    {
        public static ShaderDescription PbrSpecular = new(nameof(PbrSpecular), ShaderStage.Vertex,
            new ShaderReflector(), ResourceLoader.GetShaderByteCode("vs_pbr_specular"));
    }
}
