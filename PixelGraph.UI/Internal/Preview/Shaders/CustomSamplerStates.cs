using SharpDX.Direct3D11;

namespace PixelGraph.UI.Internal.Preview.Shaders
{
    internal static class CustomSamplerStates
    {
        public static readonly SamplerStateDescription Color;
        public static readonly SamplerStateDescription Height;


        static CustomSamplerStates()
        {
            Color = new SamplerStateDescription {
                Filter = Filter.MinMagPointMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MaximumLod = int.MaxValue,
                MinimumLod = 0,
            };

            Height = new SamplerStateDescription {
                Filter = Filter.MinMagMipPoint,
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
