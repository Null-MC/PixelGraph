using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.PixelOperations;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace McPbrPipeline.ImageProcessors
{
    internal class OcclusionProcessor : PixelComposeProcessor
    {
        private readonly Options options;
        private readonly Lazy<Vector3[]> rayList;


        public OcclusionProcessor(Options options = null)
        {
            this.options = options ?? new Options();

            rayList = new Lazy<Vector3[]>(() => CreateRays().ToArray());
        }

        protected override void ProcessPixel(ref Rgba32 pixel, in PixelContext context)
        {
            var hitCount = 0;
            var rayCount = rayList.Value.Length;
            for (var i = 0; i < rayCount; i++) {
                if (RayTest(in context.X, in context.Y, in rayList.Value[i], in context))
                    hitCount++;
            }

            MathEx.Saturate(1f - hitCount / (float)rayCount, out pixel.R);
            pixel.G = pixel.B = pixel.R;
            pixel.A = 255;
        }

        private IEnumerable<Vector3> CreateRays()
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.StepCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(options.StepCount),
                    "Occlusion Step-Count must be greater than 0!");

            if (options.Quality < 0 || options.Quality > 1f)
                throw new ArgumentOutOfRangeException(nameof(options.Quality), 
                    "Occlusion Quality must be between 0.0 and 1.0!");

            var hStepCount = 4 + (int)(options.Quality * 356f);
            var vStepCount = 1 + (int)(options.Quality * 88f);

            var hStepSize = 360f / hStepCount;
            var vStepSize = 90f / vStepCount;

            var deg2rad = (float)(Math.PI / 180);

            for (var v = 0; v < vStepCount; v++) {
                for (var h = 0; h < hStepCount; h++) {
                    var hAngleDegrees = h * hStepSize - 180f;
                    var hAngleRadians = hAngleDegrees * deg2rad;

                    var vAngleDegrees = v * vStepSize;
                    var vAngleRadians = vAngleDegrees * deg2rad;

                    yield return new Vector3 {
                        X = (float) Math.Cos(hAngleRadians),
                        Y = (float) Math.Sin(hAngleRadians),
                        Z = (float) Math.Sin(vAngleRadians),
                    };
                }
            }
        }

        private bool RayTest(in int x, in int y, in Vector3 ray, in PixelContext context)
        {
            SampleHeight(in x, in y, out var height);
            var position = new Vector3(x, y, height / 255f * options.ZScale);

            for (var step = 0; step < options.StepCount; step++) {
                position += ray;

                SampleHeight(in position.X, in position.Y, in context, out height);
                var sampleZ = height / 255f * options.ZScale;

                if (position.Z < sampleZ) return true;
            }

            return false;
        }

        private void SampleHeight(in int x, in int y, out byte height)
        {
            options.Source[x, y].GetChannelValue(in options.HeightChannel, out height);
        }

        private void SampleHeight(in float x, in float y, in PixelContext context, out byte height)
        {
            // TODO: use linear sampler instead of point?

            var px = (int) (x + 0.5f);
            var py = (int) (y + 0.5f);

            if (options.Wrap) context.Wrap(ref px, ref py);
            else context.Clamp(ref px, ref py);

            options.Source[px, py].GetChannelValue(in options.HeightChannel, out height);
        }

        public class Options
        {
            public Image<Rgba32> Source;
            public ColorChannel HeightChannel;
            public int StepCount;
            public float ZScale;
            public float Quality;
            public bool Wrap;
        }
    }
}
