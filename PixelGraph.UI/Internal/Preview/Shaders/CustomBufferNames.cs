namespace PixelGraph.UI.Internal.Preview.Shaders
{
    internal static class CustomBufferNames
    {
        public const string DiffuseAlphaTB = "tex_diffuse_alpha";
        public const string EmissiveTB = "tex_emissive";
        public const string AlbedoAlphaTB = "tex_albedo_alpha";
        public const string NormalHeightTB = "tex_normal_height";
        public const string RoughF0OcclusionTB = "tex_rough_f0_occlusion";
        public const string PorositySssEmissiveTB = "tex_porosity_sss_emissive";
        public const string EnvironmentCubeTB = "tex_environment";
        public const string IrradianceCubeTB = "tex_irradiance";
        public const string ShadowMapTB = "tex_shadow";

        public const string MinecraftSceneCB = "cbMinecraftScene";
    }
}
