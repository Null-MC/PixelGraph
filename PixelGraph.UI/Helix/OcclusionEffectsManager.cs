using System;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Shaders;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Helix.Shaders;

namespace PixelGraph.UI.Helix
{
    public class OcclusionEffectsManager : DefaultEffectsManager
    {
        private readonly IShaderByteCodeManager shaderMgr;


        public OcclusionEffectsManager(IServiceProvider provider)
        {
            shaderMgr = provider.GetRequiredService<IShaderByteCodeManager>();

            Initialize();
        }
        
        private void Initialize()
        {
            var meshTechnique = GetTechnique(DefaultRenderTechniqueNames.Mesh);

            meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.Occlusion) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderManager.Name_OcclusionVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderManager.Name_OcclusionPixel, ShaderStage.Pixel),
                },
                BlendStateDescription = DefaultBlendStateDescriptions.NoBlend,
                DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSNoDepthNoStencil,
            });
        }
    }
}
