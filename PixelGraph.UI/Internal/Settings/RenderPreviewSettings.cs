using System;

namespace PixelGraph.UI.Internal.Settings
{
    public class RenderPreviewSettings
    {
        public const bool Default_Enabled = true;
        public const bool Default_EnableBloom = false;
        public const decimal Default_ParallaxDepth = 0.25m;
        public const int Default_ParallaxSamples = 128;
        public const int Default_WaterMode = 0;
        public const int Default_EnvironmentCubeSize = 512;
        public const int Default_IrradianceCubeSize = 64;


        public bool? Enabled {get; set;}
        public bool? EnableBloom {get; set;}
        public int? WaterMode {get; set;}
        public string SelectedMode {get; set;}
        public string PomType {get; set;}

        public decimal? ParallaxDepth {get; set;}
        public int? ParallaxSamples {get; set;}

        public bool? EnableAtmosphere {get; set;}
        public int? EnvironmentCubeSize {get; set;}
        public int? IrradianceCubeSize {get; set;}
        public string IblFilename {get; set;}
        public float? IblIntensity {get; set;}

        #region Deprecated

        [Obsolete("Replace usages of EnablePuddles with WaterMode.")]
        public bool? EnablePuddles {
            get => null;
            set => WaterMode = (value ?? false) ? 1 : 0;
        }

        #endregion
    }
}
