using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Internal.Shaders;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    public class CustomEffectsManager : DefaultEffectsManager
    {
        public CustomEffectsManager(IShaderByteCodeManager shaderMgr)
        {
            var meshTechnique = GetTechnique(DefaultRenderTechniqueNames.Mesh);

            meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.PBRSpecular) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderNames.PbrSpecularVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderNames.PbrSpecularPixel, ShaderStage.Pixel),
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
