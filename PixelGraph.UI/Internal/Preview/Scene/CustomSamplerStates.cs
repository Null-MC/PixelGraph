using SharpDX.Direct3D11;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    internal static class CustomSamplerStates
    {
        public static readonly SamplerStateDescription Default;


        static CustomSamplerStates()
        {
            Default = new SamplerStateDescription {
                Filter = Filter.MinMagPointMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumLod = int.MaxValue,
                MinimumLod = 0,
                MaximumAnisotropy = 16,
            };
        }
    }
}
