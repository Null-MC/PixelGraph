using SharpDX.Direct3D11;

namespace PixelGraph.UI.Internal.Preview.Shaders
{
    internal static class CustomSamplerStates
    {
        public static readonly SamplerStateDescription Color_Point;
        public static readonly SamplerStateDescription Color_Linear;
        public static readonly SamplerStateDescription Height_Point;
        public static readonly SamplerStateDescription Height_Linear;


        static CustomSamplerStates()
        {
            Color_Point = new SamplerStateDescription {
                Filter = Filter.MinLinearMagPointMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MaximumLod = int.MaxValue,
                MinimumLod = 0,
            };

            Color_Linear = new SamplerStateDescription {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MaximumLod = int.MaxValue,
                MinimumLod = 0,
            };

            Height_Point = new SamplerStateDescription {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MaximumLod = int.MaxValue,
                MinimumLod = 0,
            };

            Height_Linear = new SamplerStateDescription {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MaximumLod = int.MaxValue,
                MinimumLod = 0,
            };
        }
    }
}
