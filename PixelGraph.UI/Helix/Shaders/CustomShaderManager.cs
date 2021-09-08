namespace PixelGraph.UI.Helix.Shaders
{
    internal class CustomShaderManager : ShaderByteCodeManager
    {
        public const string Name_SkyVertex = "sky_vs";
        public const string Name_SkyPixel = "sky_ps";
        public const string Name_SkyFinalPixel = "sky_final_ps";
        public const string Name_SkyIrradiancePixel = "sky_irradiance_ps";

        public const string Name_DiffuseVertex = "diffuse_vs";
        public const string Name_DiffusePixel = "diffuse_ps";

        public const string Name_PbrVertex = "pbr_vs";
        public const string Name_PbrFilamentPixel = "pbr_filament_ps";
        public const string Name_PbrJessiePixel = "pbr_jessie_ps";
        public const string Name_PbrNullPixel = "pbr_null_ps";

        public const string Name_ShadowVertex = "shadow_vs";
        public const string Name_ShadowPixel = "shadow_ps";

        public const string Name_OcclusionVertex = "occlusion_vs";
        public const string Name_OcclusionPixel = "occlusion_ps";


        public CustomShaderManager()
        {
            DefineShaderSources();
        }

        private void DefineShaderSources()
        {
            Add(Name_SkyVertex, new ShaderSourceDescription {
                Profile = "vs_4_0",
                RawFileName = "sky_vs.hlsl",
                CompiledResourceName = "sky_vs.cso",
            });

            Add(Name_SkyPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "sky_ps.hlsl",
                CompiledResourceName = "sky_ps.cso",
            });

            Add(Name_SkyFinalPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "sky_final_ps.hlsl",
                CompiledResourceName = "sky_final_ps.cso",
            });

            Add(Name_SkyIrradiancePixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "sky_irradiance_ps.hlsl",
                CompiledResourceName = "sky_irradiance_ps.cso",
            });

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

            Add(Name_PbrFilamentPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "pbr_filament_ps.hlsl",
                CompiledResourceName = "pbr_filament_ps.cso",
            });

            Add(Name_PbrJessiePixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "pbr_jessie_ps.hlsl",
                CompiledResourceName = "pbr_jessie_ps.cso",
            });

            Add(Name_PbrNullPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "pbr_null_ps.hlsl",
                CompiledResourceName = "pbr_null_ps.cso",
            });

            Add(Name_ShadowVertex, new ShaderSourceDescription {
                Profile = "vs_4_0",
                RawFileName = "shadow_vs.hlsl",
                CompiledResourceName = "shadow_vs.cso",
            });

            Add(Name_ShadowPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "shadow_ps.hlsl",
                CompiledResourceName = "shadow_ps.cso",
            });

            Add(Name_OcclusionVertex, new ShaderSourceDescription {
                Profile = "vs_4_0",
                RawFileName = "occlusion_vs.hlsl",
                CompiledResourceName = "occlusion_vs.cso",
            });

            Add(Name_OcclusionPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "occlusion_ps.hlsl",
                CompiledResourceName = "occlusion_ps.cso",
            });
        }
    }
}
