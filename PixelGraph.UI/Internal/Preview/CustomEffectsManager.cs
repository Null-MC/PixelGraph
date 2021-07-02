using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Shaders;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Preview.Shaders;
using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview
{
    public class CustomEffectsManager : DefaultEffectsManager
    {
        private readonly IShaderByteCodeManager shaderMgr;


        public CustomEffectsManager(IServiceProvider provider)
        {
            shaderMgr = provider.GetRequiredService<IShaderByteCodeManager>();

            Initialize();
        }
        
        private void Initialize()
        {
            AddTechnique(new TechniqueDescription(CustomRenderTechniqueNames.DynamicSkybox) {
                InputLayoutDescription = new InputLayoutDescription(shaderMgr.GetCode(CustomShaderManager.Name_SkyVertex), DefaultInputLayout.VSInputSkybox),
                PassDescriptions = new List<ShaderPassDescription> {
                    new(DefaultPassNames.Default) {
                        ShaderList = new[] {
                            shaderMgr.BuildDescription(CustomShaderManager.Name_SkyVertex, ShaderStage.Vertex),
                            shaderMgr.BuildDescription(CustomShaderManager.Name_SkyPixel, ShaderStage.Pixel),
                        },
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSLessEqualNoWrite,
                        BlendStateDescription = DefaultBlendStateDescriptions.BSSourceAlways,
                        RasterStateDescription = DefaultRasterDescriptions.RSSkybox,
                    },
                    new(CustomPassNames.SkyFinal) {
                        ShaderList = new[] {
                            shaderMgr.BuildDescription(CustomShaderManager.Name_SkyVertex, ShaderStage.Vertex),
                            shaderMgr.BuildDescription(CustomShaderManager.Name_SkyFinalPixel, ShaderStage.Pixel),
                        },
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSLessEqualNoWrite,
                        BlendStateDescription = DefaultBlendStateDescriptions.BSSourceAlways,
                        RasterStateDescription = DefaultRasterDescriptions.RSSkybox,
                    },
                    new(CustomPassNames.SkyIrradiance) {
                        ShaderList = new[] {
                            shaderMgr.BuildDescription(CustomShaderManager.Name_SkyVertex, ShaderStage.Vertex),
                            shaderMgr.BuildDescription(CustomShaderManager.Name_SkyIrradiancePixel, ShaderStage.Pixel),
                        },
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSLessEqualNoWrite,
                        BlendStateDescription = DefaultBlendStateDescriptions.BSSourceAlways,
                        RasterStateDescription = DefaultRasterDescriptions.RSSkybox,
                    },
                },
            });

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

            meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.PbrFilament) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrFilamentPixel, ShaderStage.Pixel),
                },
                BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess,
            });

            // TODO: PBR-Metal OIT

            meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.PbrJessie) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrJessiePixel, ShaderStage.Pixel),
                },
                BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess,
            });

            meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.PbrNull) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderManager.Name_PbrNullPixel, ShaderStage.Pixel),
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

            meshTechnique.RemovePass(DefaultPassNames.ShadowPass);
            meshTechnique.AddPass(new ShaderPassDescription(DefaultPassNames.ShadowPass) {
                ShaderList = new[] {
                    shaderMgr.BuildDescription(CustomShaderManager.Name_ShadowVertex, ShaderStage.Vertex),
                    shaderMgr.BuildDescription(CustomShaderManager.Name_ShadowPixel, ShaderStage.Pixel),
                },
                BlendStateDescription = DefaultBlendStateDescriptions.NoBlend,
                DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess,
            });
        }
    }
}
