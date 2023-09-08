using SharpDX.Direct3D11;

namespace PixelGraph.Rendering.Shaders;

internal static class CustomBlendStateDescriptions
{
    public static readonly BlendStateDescription BSPremultipliedAlpha;


    static CustomBlendStateDescriptions()
    {
        BSPremultipliedAlpha.RenderTarget[0] = new RenderTargetBlendDescription {
            SourceBlend = BlendOption.SourceAlpha,
            DestinationBlend = BlendOption.InverseSourceAlpha,
            BlendOperation = BlendOperation.Add,

            SourceAlphaBlend = BlendOption.InverseDestinationAlpha,
            DestinationAlphaBlend = BlendOption.One,
            AlphaBlendOperation = BlendOperation.Add,
                
            RenderTargetWriteMask = ColorWriteMaskFlags.All,
            IsBlendEnabled = true,
        };
    }
}