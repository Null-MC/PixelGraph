using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OcclusionProcessor : PixelProcessor
    {
        private readonly Options options;
        private readonly Lazy<Vector3[]> rayList;


        public OcclusionProcessor(Options options = null)
        {
            this.options = options ?? new Options();

            rayList = new Lazy<Vector3[]>(() => CreateRays().ToArray());
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            //var sourceRow = options.Sampler.Image.GetPixelRowSpan(context.Y);

            //var pixelIn = new Rgba32();
            //sourceRow[context.X].ToRgba32(ref pixelIn);
            //pixelIn.GetChannelValueScaled(in options.HeightChannel, out var height);
            options.Sampler.SampleScaled(context.X, context.Y, in options.HeightChannel, out var height);

            // TODO: range, shift, power
            //if (height < options.HeightMin || height > options.HeightMax) return;
            if (!options.HeightInvert) height = 1f - height;

            var z = height * options.ZScale + options.ZBias;

            var hitCount = 0;
            var position = new Vector3();

            var rayCount = rayList.Value.Length;
            for (var i = 0; i < rayCount; i++) {
                position.X = context.X;
                position.Y = context.Y;
                position.Z = z;

                if (RayTest(ref position, in rayList.Value[i], in context))
                    hitCount++;
            }

            MathEx.Saturate(1f - hitCount / (float)rayCount, out pixelOut.R);
            pixelOut.B = pixelOut.G = pixelOut.R;
            pixelOut.A = 255;
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

            for (var v = 0; v < vStepCount; v++) {
                for (var h = 0; h < hStepCount; h++) {
                    var hAngleDegrees = h * hStepSize - 180f;
                    var hAngleRadians = hAngleDegrees * MathEx.Deg2Rad;

                    var vAngleDegrees = v * vStepSize;
                    var vAngleRadians = vAngleDegrees * MathEx.Deg2Rad;

                    yield return new Vector3 {
                        X = (float) Math.Cos(hAngleRadians),
                        Y = (float) Math.Sin(hAngleRadians),
                        Z = (float) Math.Sin(vAngleRadians),
                    };
                }
            }
        }

        private bool RayTest(ref Vector3 position, in Vector3 ray, in PixelContext context)
        {
            for (var step = 0; step < options.StepCount; step++) {
                position += ray;

                if (position.Z > options.ZScale) return false;

                options.Sampler.SampleScaled(in position.X, in position.Y, options.HeightChannel, out var height);

                // TODO: range, shift, power
                //if (height < options.HeightMin || height > options.HeightMax) return;
                if (!options.HeightInvert) height = 1f - height;

                if (position.Z < height * options.ZScale) return true;
            }

            return false;
        }

        public class Options
        {
            //public Image<Rgba32> HeightSource;
            public ColorChannel HeightChannel;
            public ISampler Sampler;
            public byte HeightMin;
            public byte HeightMax;
            public short HeightShift;
            public float HeightPower;
            public bool HeightInvert;
            public int StepCount;
            public float ZScale;
            public float ZBias;
            public float Quality;
            public bool Wrap;
        }
    }
}
