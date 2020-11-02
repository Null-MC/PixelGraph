using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalRestoreProcessor : PixelFilterProcessor
    {
        private readonly Options options;


        public NormalRestoreProcessor(Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixel, in PixelContext context)
        {

            pixel.GetChannelValue(in options.NormalX, out var _x);
            pixel.GetChannelValue(in options.NormalY, out var _y);

            Vector3 vector;
            vector.X = _x / 255f;
            vector.Y = _y / 255f;

            var dot = vector.X * vector.X + vector.Y * vector.Y;
            vector.Z = (float)Math.Sqrt(1f - dot);

            // TODO: not "necessary"
            //MathEx.Normalize(ref vector);

            MathEx.Saturate(vector.Z, out var _z);
            pixel.SetChannelValue(in options.NormalZ, in _z);
        }

        public class Options
        {
            public ColorChannel NormalX = ColorChannel.None;
            public ColorChannel NormalY = ColorChannel.None;
            public ColorChannel NormalZ = ColorChannel.None;


            public bool HasAllMappings()
            {
                if (NormalX == ColorChannel.None) return false;
                if (NormalY == ColorChannel.None) return false;
                if (NormalZ == ColorChannel.None) return false;
                return true;
            }
        }
    }
}
