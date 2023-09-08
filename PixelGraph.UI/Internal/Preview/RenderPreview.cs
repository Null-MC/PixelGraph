namespace PixelGraph.UI.Internal.Preview;

public static class RenderPreview
{
#if !NORENDER
    public const bool IsSupported = true;
#else
        public const bool IsSupported = false;
#endif
}