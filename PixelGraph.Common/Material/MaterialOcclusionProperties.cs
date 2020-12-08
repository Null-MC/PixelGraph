using PixelGraph.Common.Encoding;

namespace PixelGraph.Common.Material
{
    public class MaterialOcclusionProperties
    {
        public const float DefaultQuality = 0.1f;
        public const float DefaultZScale = 25f;
        public const float DefaultZBias = 1.0f;
        public const int DefaultSteps = 32;

        public TextureEncoding Input {get; set;}
        public string Texture {get; set;}
        public byte? Value {get; set;}
        public decimal? Scale {get; set;}
        public decimal? Quality {get; set;}
        public decimal? ZBias {get; set;}
        public decimal? ZScale {get; set;}
        public int? Steps {get; set;}
        public bool? ClipEmissive {get; set;}
    }
}
