using System;

namespace PixelGraph.UI.Internal.Settings
{
    public class RenderPreviewSettings
    {
        public const bool Default_Enabled = true;
        //public const bool Default_EnableLinearSampling = false;
        //public const bool Default_EnableSlopeNormals = true;
        public const bool Default_EnableBloom = false;
        //public const bool Default_ParallaxEnabled = true;
        public const decimal Default_ParallaxDepth = 0.25m;
        //public const int Default_ParallaxSamplesMin = 4;
        public const int Default_ParallaxSamples = 128;
        public const int Default_WaterMode = 0;


        public bool? Enabled {get; set;}
        //public bool? EnableLinearSampling {get; set;}
        //public bool? EnableSlopeNormals {get; set;}
        public bool? EnableBloom {get; set;}
        public int? WaterMode {get; set;}
        public string SelectedMode {get; set;}

        //public bool? ParallaxEnabled {get; set;}
        public decimal? ParallaxDepth {get; set;}
        //public int? ParallaxSamplesMin {get; set;}
        public int? ParallaxSamples {get; set;}

        #region Deprecated

        [Obsolete("Replace usages of EnablePuddles with WaterMode.")]
        public bool? EnablePuddles {
            get => null;
            set => WaterMode = (value ?? false) ? 1 : 0;
        }

        #endregion
    }
}
