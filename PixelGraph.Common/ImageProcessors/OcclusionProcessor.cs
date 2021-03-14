using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OcclusionProcessor<THeight> : PixelRowProcessor
        where THeight : unmanaged, IPixel<THeight>
    {
        private readonly Options options;
        private readonly Lazy<Vector3[]> rayList;


        public OcclusionProcessor(Options options = null)
        {
            this.options = options ?? new Options();

            rayList = new Lazy<Vector3[]>(CreateRays);
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var rayCount = rayList.Value.Length;
            var rayCountFactor = 1d / rayCount;
            var pixelOut = new Rgba32(0, 0, 0, 255);
            var position = new Vector3();

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var (fx, fy) = GetTexCoord(in context, in x);
                options.HeightSampler.SampleScaled(in fx, in fy, in options.HeightChannel, out var height);

                // TODO: range, shift, power
                if (!options.HeightInvert) MathEx.Invert(ref height);

                var z = height * options.ZScale + options.ZBias;

                var hitFactor = 0d;
                for (var i = 0; i < rayCount; i++) {
                    position.X = x;
                    position.Y = context.Y;
                    position.Z = z;

                    if (RayTest(in context, ref position, in rayList.Value[i], out var rayHitFactor))
                        hitFactor += rayHitFactor * rayCountFactor;
                }

                MathEx.Saturate(1d - hitFactor, out pixelOut.R);
                pixelOut.B = pixelOut.G = pixelOut.R;
                row[x].FromRgba32(pixelOut);
            }
        }

        private Vector3[] CreateRays()
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.StepCount < 1)
                throw new ArgumentOutOfRangeException(nameof(options.StepCount),
                    "Occlusion Step-Count must be greater than 0!");

            if (options.Quality < 0 || options.Quality > 1f)
                throw new ArgumentOutOfRangeException(nameof(options.Quality), 
                    "Occlusion Quality must be between 0.0 and 1.0!");

            var hStepCount = 4 + (int)(options.Quality * 356f);
            var vStepCount = 1 + (int)(options.Quality * 88f);

            var hStepSize = 360f / hStepCount;
            var vStepSize = 90f / vStepCount;

            var count = hStepCount * vStepCount;
            var result = new Vector3[count];

            for (var v = 0; v < vStepCount; v++) {
                for (var h = 0; h < hStepCount; h++) {
                    var hAngleDegrees = h * hStepSize - 180f;
                    var hAngleRadians = hAngleDegrees * MathEx.Deg2Rad;

                    var vAngleDegrees = v * vStepSize;
                    var vAngleRadians = vAngleDegrees * MathEx.Deg2Rad;

                    var z = hStepCount * v + h;
                    result[z].X = MathF.Cos(hAngleRadians);
                    result[z].Y = MathF.Sin(hAngleRadians);
                    result[z].Z = MathF.Sin(vAngleRadians);
                }
            }

            return result;
        }

        private bool RayTest(in PixelRowContext context, ref Vector3 position, in Vector3 ray, out float factor)
        {
            //var stepCount = (int)MathF.Max(context.Bounds.Width * options.StepDistance, 1f);

            for (var step = 1; step <= options.StepCount; step++) {
                position += ray;

                if (position.Z > options.ZScale) break;

                var (fx, fy) = GetTexCoord(in context, in position.X, in position.Y);
                options.HeightSampler.SampleScaled(in fx, in fy, options.HeightChannel, out var height);

                // TODO: range, shift, power
                if (!options.HeightInvert) MathEx.Invert(ref height);
                if (!(position.Z < height * options.ZScale)) continue;

                // hit, return 
                factor = (float)step / options.StepCount;
                factor = 1f - MathF.Pow(factor, options.HitPower);
                return true;
            }

            factor = 0f;
            return false;
        }

        public class Options
        {
            public ISampler<THeight> HeightSampler;
            public ColorChannel HeightChannel;
            public float HeightMinValue;
            public float HeightMaxValue;
            public byte HeightRangeMin;
            public byte HeightRangeMax;
            public int HeightShift;
            public float HeightPower;
            public bool HeightInvert;

            //public float StepDistance;
            public int StepCount;
            public float HitPower = 1.5f;
            public float ZScale;
            public float ZBias;
            public float Quality;
        }
    }
}
