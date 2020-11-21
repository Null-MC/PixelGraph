using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewModels
{
    internal class InputVMX : ViewModelBase
    {
        //public string RedTexture {get; set;}
        //public ColorChannel? RedColor {get; set;}
        //public string GreenTexture {get; set;}
        //public ColorChannel? GreenColor {get; set;}
        //public string BlueTexture {get; set;}
        //public ColorChannel? BlueColor {get; set;}
        //public string AlphaTexture {get; set;}
        //public ColorChannel? AlphaColor {get; set;}
        public EncodingChannelMapping Red {get; set;}
        public EncodingChannelMapping Green {get; set;}
        public EncodingChannelMapping Blue {get; set;}
        public EncodingChannelMapping Alpha {get; set;}

        //public string HeightTexture {get; set;}
        //public ColorChannel? HeightColor {get; set;}
        public EncodingChannelMapping Height {get; set;}

        //public string NormalXTexture {get; set;}
        //public ColorChannel? NormalXColor {get; set;}
        //public string NormalYTexture {get; set;}
        //public ColorChannel? NormalYColor {get; set;}
        //public string NormalZTexture {get; set;}
        //public ColorChannel? NormalZColor {get; set;}
        public EncodingChannelMapping NormalX {get; set;}
        public EncodingChannelMapping NormalY {get; set;}
        public EncodingChannelMapping NormalZ {get; set;}

        //public string OcclusionTexture {get; set;}
        //public ColorChannel? OcclusionColor {get; set;}
        public EncodingChannelMapping Occlusion {get; set;}

        //public string SpecularTexture {get; set;}
        //public ColorChannel? SpecularColor {get; set;}
        public EncodingChannelMapping Specular {get; set;}

        //public string SmoothTexture {get; set;}
        //public ColorChannel? SmoothColor {get; set;}
        //public string PerceptualSmoothTexture {get; set;}
        //public ColorChannel? PerceptualSmoothColor {get; set;}
        public EncodingChannelMapping Smooth {get; set;}
        public EncodingChannelMapping PerceptualSmooth {get; set;}

        //public string RoughTexture {get; set;}
        //public ColorChannel? RoughColor {get; set;}
        //public string PerceptualRoughTexture {get; set;}
        //public ColorChannel? PerceptualRoughColor {get; set;}
        public EncodingChannelMapping Rough {get; set;}
        public EncodingChannelMapping PerceptualRough {get; set;}

        //public string MetalTexture {get; set;}
        //public ColorChannel? MetalColor {get; set;}
        public EncodingChannelMapping Metal {get; set;}

        //public string PorosityTexture {get; set;}
        //public ColorChannel? PorosityColor {get; set;}
        public EncodingChannelMapping Porosity {get; set;}

        public EncodingChannelMapping SSS {get; set;}

        public EncodingChannelMapping Porosity_SSS {get; set;}

        public EncodingChannelMapping Emissive {get; set;}
    }

    internal class EncodingChannelMapping
    {
        public string Texture {get; set;}
        public ColorChannel? Color {get; set;}
        public byte? ValueMin {get; set;}
        public byte? ValueMax {get; set;}
    }
}
