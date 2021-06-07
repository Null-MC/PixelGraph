using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Internal.Preview.Shaders;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    public class CustomEffectsManager : DefaultEffectsManager
    {
        public CustomEffectsManager(IShaderByteCodeManager shaderMgr)
        {
            var meshTechnique = GetTechnique(DefaultRenderTechniqueNames.Mesh);

            meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.Diffuse) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderManager.Name_DiffuseVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderManager.Name_DiffusePixel, ShaderStage.Pixel),
                },
                BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess,
            });

            // TODO: Diffuse OIT

            meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.PbrMetal) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrMetalPixel, ShaderStage.Pixel),
                },
                BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess,
            });

            // TODO: PBR-Metal OIT

            meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.PbrSpecular) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrSpecularPixel, ShaderStage.Pixel),
                },
                BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess,
            });

            //meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.PBRSpecularOIT) {
            //    ShaderList = new[] {
            //        DefaultVSShaderDescriptions.VSMeshDefault,
            //        CustomPSShaderDescription.PSPbrSpecular,
            //    },
            //    BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
            //    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess,
            //});
        }
    }
}
