using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialOcclusionProperties
    {
        public const string DefaultSampler = Samplers.Sampler.Nearest;
        public const float DefaultQuality = 0.08f;
        public const float DefaultZScale = 25f;
        public const float DefaultZBias = 0.1f;
        public const int DefaultSteps = 16;

        public ResourcePackOcclusionChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}
        public decimal? Scale {get; set;}
        public string Sampler {get; set;}
        public decimal? Quality {get; set;}
        public decimal? ZBias {get; set;}
        public decimal? ZScale {get; set;}
        public int? Steps {get; set;}
        public bool? ClipEmissive {get; set;}
    }
}
