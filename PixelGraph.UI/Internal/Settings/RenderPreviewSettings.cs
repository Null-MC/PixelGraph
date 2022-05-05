using System;

namespace PixelGraph.UI.Internal.Settings
{
    public class RenderPreviewSettings : ICloneable
    {
        public const bool Default_Enabled = true;
        public const bool Default_EnableBloom = false;
        public const decimal Default_ParallaxDepth = 0.25m;
        public const int Default_ParallaxSamples = 128;
        public const int Default_WaterMode = 0;
        public const int Default_EnvironmentCubeSize = 512;
        public const int Default_IrradianceCubeSize = 64;
        public const decimal Default_SubSurfaceBlur = 0.5m;

        public bool? Enabled {get; set;}
        public bool? EnableBloom {get; set;}
        public int? WaterMode {get; set;}
        public string SelectedMode {get; set;}
        public string PomType {get; set;}
        public decimal? SubSurfaceBlur {get; set;}

        public decimal? ParallaxDepth {get; set;}
        public int? ParallaxSamples {get; set;}

        public bool? EnableAtmosphere {get; set;}
        public int? EnvironmentCubeSize {get; set;}
        public int? IrradianceCubeSize {get; set;}
        public string IblFilename {get; set;}
        public float? IblIntensity {get; set;}


        public object Clone()
        {
            return MemberwiseClone();
        }


        public static readonly string Default_ParallaxDepthText = Default_ParallaxDepth.ToString("N");
        public static readonly string Default_ParallaxSamplesText = Default_ParallaxSamples.ToString("N0");
    }
}
