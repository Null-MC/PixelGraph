namespace PixelGraph.Rendering.Shaders
{
    public class CustomShaderManager : ShaderByteCodeManager
    {
        public const string Name_SkyVertex = "sky_vs";
        public const string Name_SkyPixel = "sky_ps";
        public const string Name_SkyErpPixel = "sky_erp_ps";
        public const string Name_SkyFinalCubePixel = "sky_final_cube_ps";
        public const string Name_SkyFinalErpPixel = "sky_final_erp_ps";
        public const string Name_SkyIrradiancePixel = "sky_irradiance_ps";

        //public const string Name_DiffuseVertex = "diffuse_vs";
        public const string Name_DiffusePixel = "diffuse_ps";

        //public const string Name_NormalsVertex = "normals_vs";
        public const string Name_NormalsPixel = "normals_ps";

        public const string Name_PbrVertex = "pbr_vs";
        public const string Name_PbrFilamentPixel = "pbr_filament_ps";
        public const string Name_PbrJessiePixel = "pbr_jessie_ps";
        public const string Name_PbrNullPixel = "pbr_null_ps";

        public const string Name_ShadowVertex = "shadow_vs";
        public const string Name_ShadowPixel = "shadow_ps";

        public const string Name_OcclusionVertex = "occlusion_vs";
        public const string Name_OcclusionPixel = "occlusion_ps";

        public const string Name_BdrfDielectricLutPixel = "bdrf_dielectric_ps";


        public CustomShaderManager()
        {
            Add("vs_4_0", Name_SkyVertex, "sky_vs");
            Add("ps_4_0", Name_SkyPixel, "sky_ps");
            Add("ps_4_0", Name_SkyErpPixel, "sky_erp_ps");
            Add("ps_4_0", Name_SkyFinalCubePixel, "sky_final_cube_ps");
            Add("ps_4_0", Name_SkyFinalErpPixel, "sky_final_erp_ps");
            Add("ps_4_0", Name_SkyIrradiancePixel, "sky_irradiance_ps");

            Add("vs_4_0", Name_PbrVertex, "pbr_vs");
            Add("ps_4_0", Name_DiffusePixel, "diffuse_ps");
            Add("ps_4_0", Name_NormalsPixel, "normals_ps");
            Add("ps_4_0", Name_PbrFilamentPixel, "pbr_filament_ps");
            Add("ps_4_0", Name_PbrJessiePixel, "pbr_jessie_ps");
            Add("ps_4_0", Name_PbrNullPixel, "pbr_null_ps");

            Add("vs_4_0", Name_ShadowVertex, "shadow_vs");
            Add("ps_4_0", Name_ShadowPixel, "shadow_ps");

            Add("vs_4_0", Name_OcclusionVertex, "occlusion_vs");
            Add("ps_4_0", Name_OcclusionPixel, "occlusion_ps");

            Add("ps_4_0", Name_BdrfDielectricLutPixel, "bdrf_dielectric_ps");
        }
    }
}
