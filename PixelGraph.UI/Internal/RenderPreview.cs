namespace PixelGraph.UI.Internal
{
    public static class RenderPreview
    {
#if !NORENDER
        public const bool IsSupported = true;
#else
        public const bool IsSupported = false;
#endif
    }
}
