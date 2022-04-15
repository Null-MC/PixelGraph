using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace PixelGraph.Rendering.Shaders
{
    public static class CustomSamplerStates
    {
        public static readonly SamplerStateDescription Color_Point;
        public static readonly SamplerStateDescription Color_Linear;
        public static readonly SamplerStateDescription Height_Point;
        public static readonly SamplerStateDescription Height_Linear;
        public static readonly SamplerStateDescription Shadow;
        public static readonly SamplerStateDescription Light;
        public static readonly SamplerStateDescription Environment;
        public static readonly SamplerStateDescription Irradiance;
        public static readonly SamplerStateDescription BrdfLut;


        static CustomSamplerStates()
        {
            Color_Point = new SamplerStateDescription {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumLod = float.MaxValue,
                MaximumAnisotropy = 16,
            };

            Color_Linear = new SamplerStateDescription {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumLod = float.MaxValue,
                MaximumAnisotropy = 16,
            };

            Height_Point = new SamplerStateDescription {
                Filter = Filter.MinMagPointMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumLod = float.MaxValue,
                MaximumAnisotropy = 16,
            };

            Height_Linear = new SamplerStateDescription {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumLod = float.MaxValue,
                MaximumAnisotropy = 16,
            };

            Shadow = new SamplerStateDescription {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                Filter = Filter.ComparisonMinMagMipPoint,
                ComparisonFunction = Comparison.Less,
                BorderColor = new RawColor4(1, 1, 1, 0),
            };

            Light = new SamplerStateDescription {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                Filter = Filter.MinMagMipPoint,
                ComparisonFunction = Comparison.Never,
                BorderColor = new RawColor4(1, 1, 1, 0),
            };

            Environment = new SamplerStateDescription {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipLinear,
                MaximumLod = float.MaxValue,
                MaximumAnisotropy = 1,
            };

            Irradiance = new SamplerStateDescription {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagLinearMipPoint,
                MaximumAnisotropy = 1,
                MaximumLod = 0f,
            };

            BrdfLut = new SamplerStateDescription {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipPoint,
                MaximumAnisotropy = 1,
                MaximumLod = 0f,
            };
        }
    }
}
