using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Shaders;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Rendering.Shaders;
using System;
using System.Collections.Generic;
using SharpDX.Direct3D;

namespace PixelGraph.Rendering;

public class PreviewEffectsManager : DefaultEffectsManager
{
    private readonly IShaderByteCodeManager shaderMgr;


    public PreviewEffectsManager(IServiceProvider provider)
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
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSNoDepthNoStencil,
                    BlendStateDescription = DefaultBlendStateDescriptions.BSSourceAlways,
                    RasterStateDescription = DefaultRasterDescriptions.RSSkybox,
                },
                new(CustomPassNames.Sky_ERP) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_SkyVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_SkyErpPixel, ShaderStage.Pixel),
                    },
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSNoDepthNoStencil,
                    BlendStateDescription = DefaultBlendStateDescriptions.BSSourceAlways,
                    RasterStateDescription = DefaultRasterDescriptions.RSSkybox,
                },
                new(CustomPassNames.SkyFinal_Cube) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_SkyVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_SkyFinalCubePixel, ShaderStage.Pixel),
                    },
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSLessEqualNoWrite,
                    BlendStateDescription = DefaultBlendStateDescriptions.BSSourceAlways,
                    RasterStateDescription = DefaultRasterDescriptions.RSSkybox,
                },
                new(CustomPassNames.SkyFinal_ERP) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_SkyVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_SkyFinalErpPixel, ShaderStage.Pixel),
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
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSNoDepthNoStencil,
                    BlendStateDescription = DefaultBlendStateDescriptions.BSSourceAlways,
                    RasterStateDescription = DefaultRasterDescriptions.RSSkybox,
                },
            },
        });

        AddTechnique(new TechniqueDescription(CustomRenderTechniqueNames.BdrfDielectricLut) {
            InputLayoutDescription = InputLayoutDescription.EmptyInputLayout,
            PassDescriptions = new[] {
                new ShaderPassDescription(DefaultPassNames.Default) {
                    ShaderList = new[] {
                        DefaultVSShaderDescriptions.VSScreenQuad,
                        shaderMgr.BuildDescription(CustomShaderManager.Name_BdrfDielectricLutPixel, ShaderStage.Pixel),
                    },
                    Topology = PrimitiveTopology.TriangleStrip,
                    BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLessEqual,
                    RasterStateDescription = DefaultRasterDescriptions.RSSkybox,
                }
            }
        });

        RemoveTechnique(DefaultRenderTechniqueNames.Mesh);
        var meshTechnique = new TechniqueDescription(DefaultRenderTechniqueNames.Mesh) {
            InputLayoutDescription = new InputLayoutDescription(shaderMgr.GetCode(CustomShaderManager.Name_PbrVertex), CustomInputLayout.VSBlockInput),
            PassDescriptions = new List<ShaderPassDescription> {
                new ShaderPassDescription(CustomPassNames.Diffuse) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_DiffusePixel, ShaderStage.Pixel),
                    },
                    BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLessEqual,
                },

                // TODO: Diffuse OIT

                new ShaderPassDescription(CustomPassNames.Normals) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_NormalsPixel, ShaderStage.Pixel),
                    },
                    BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLessEqual,
                },

                // TODO: Normals OIT

                new ShaderPassDescription(CustomPassNames.PbrFilament) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_PbrFilamentPixel, ShaderStage.Pixel),
                    },
                    BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLessEqual,
                },

                // TODO: PBR-Metal OIT

                new ShaderPassDescription(CustomPassNames.PbrJessie) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_PbrJessiePixel, ShaderStage.Pixel),
                    },
                    BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLessEqual,
                },

                new ShaderPassDescription(CustomPassNames.PbrNull) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_PbrVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_PbrNullPixel, ShaderStage.Pixel),
                    },
                    BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLessEqual,
                },

                //meshTechnique.AddPass(new ShaderPassDescription(CustomPassNames.PBRSpecularOIT) {
                //    ShaderList = new[] {
                //        DefaultVSShaderDescriptions.VSMeshDefault,
                //        CustomPSShaderDescription.PSPbrSpecular,
                //    },
                //    BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                //    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess,
                //});

                new ShaderPassDescription(DefaultPassNames.ShadowPass) {
                    ShaderList = new[] {
                        shaderMgr.BuildDescription(CustomShaderManager.Name_ShadowVertex, ShaderStage.Vertex),
                        shaderMgr.BuildDescription(CustomShaderManager.Name_ShadowPixel, ShaderStage.Pixel),
                    },
                    BlendStateDescription = DefaultBlendStateDescriptions.NoBlend,
                    DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLessEqual,
                },
            }
        };
        AddTechnique(meshTechnique);
    }
}