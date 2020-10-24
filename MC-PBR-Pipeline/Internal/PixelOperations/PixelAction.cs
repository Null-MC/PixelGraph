using SixLabors.ImageSharp.PixelFormats;

namespace McPbrPipeline.Internal.PixelOperations
{
    internal delegate void PixelAction(ref Rgba32 value, in PixelContext context);
}
