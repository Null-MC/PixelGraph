using PixelGraph.Common.ResourcePack;
using System;

namespace PixelGraph.Common.Material
{
    public class MaterialOcclusionProperties
    {
        public const string DefaultSampler = Samplers.Samplers.Nearest;
        public const float DefaultQuality = 0.1f;
        public const float DefaultZScale = 8f;
        public const float DefaultZBias = 0.1f;
        //public const int DefaultSteps = 16;
        public const float DefaultStepDistance = 0.1f;

        public ResourcePackOcclusionChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}
        public decimal? Scale {get; set;}
        public string Sampler {get; set;}
        public decimal? Quality {get; set;}
        public decimal? ZBias {get; set;}
        public decimal? ZScale {get; set;}
        public decimal? StepDistance {get; set;}
        public bool? ClipEmissive {get; set;}


        [Obsolete] public int? Steps {
            get => null;
            set {}
        }
    }
}
