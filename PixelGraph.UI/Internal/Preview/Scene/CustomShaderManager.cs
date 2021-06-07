using PixelGraph.UI.Internal.Preview.Shaders;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    internal interface ICustomShaderManager : IShaderByteCodeManager {}

    internal class CustomShaderManager : ShaderByteCodeManager, ICustomShaderManager
    {
        public const string Name_DiffuseVertex = "diffuse_vs";
        public const string Name_DiffusePixel = "diffuse_ps";
        public const string Name_PbrVertex = "pbr_vs";
        public const string Name_PbrMetalPixel = "pbr_metal_ps";
        public const string Name_PbrSpecularPixel = "pbr_specular_ps";


        public CustomShaderManager()
        {
            Add(Name_DiffuseVertex, new ShaderSourceDescription {
                Profile = "vs_4_0",
                RawFileName = "pbr_vs.hlsl",
                CompiledResourceName = "pbr_vs.cso",
            });

            Add(Name_DiffusePixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "diffuse_ps.hlsl",
                CompiledResourceName = "diffuse_ps.cso",
            });

            Add(Name_PbrVertex, new ShaderSourceDescription {
                Profile = "vs_4_0",
                RawFileName = "pbr_vs.hlsl",
                CompiledResourceName = "pbr_vs.cso",
            });

            Add(Name_PbrMetalPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "pbr_metal_ps.hlsl",
                CompiledResourceName = "pbr_metal_ps.cso",
            });

            Add(Name_PbrSpecularPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "pbr_specular_ps.hlsl",
                CompiledResourceName = "pbr_specular_ps.cso",
            });
        }
    }
}
