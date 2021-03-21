using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;
using System.Threading;

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
            var rayCountFactor = 1f / (rayCount + 1);
            var pixelOut = new Rgba32(0, 0, 0, 255);
            var position = new Vector3();

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                options.Token.ThrowIfCancellationRequested();

                var (fx, fy) = GetTexCoord(in context, in x);
                options.HeightSampler.SampleScaled(in fx, in fy, in options.HeightMapping.InputColor, out var heightPixel);
                if (!options.HeightMapping.TryUnmap(ref heightPixel, out var heightValue)) continue;

                if (!options.HeightMapping.ValueScale.Equal(1f))
                    heightValue *= options.HeightMapping.ValueScale;

                var z = (1f - heightValue) + options.ZBias;

                var hitFactor = 0f;
                if (z < options.ZScale) {
                    for (var i = 0; i < rayCount; i++) {
                        options.Token.ThrowIfCancellationRequested();

                        position.X = x;
                        position.Y = context.Y;
                        position.Z = z;

                        if (RayTest(in context, ref position, in rayList.Value[i], out var rayHitFactor))
                            hitFactor += rayHitFactor * rayCountFactor;
                    }
                }

                MathEx.Saturate(1f - hitFactor, out pixelOut.R);
                pixelOut.B = pixelOut.G = pixelOut.R;
                row[x].FromRgba32(pixelOut);
            }
        }

        private bool RayTest(in PixelRowContext context, ref Vector3 position, in Vector3 ray, out float factor)
        {
            for (var step = 1; step <= options.StepCount; step++) {
                position.Add(in ray);

                if (position.Z >= 1f) break;

                var (fx, fy) = GetTexCoord(in context, in position.X, in position.Y);
                options.HeightSampler.Sample(in fx, in fy, options.HeightMapping.InputColor, out var heightPixel);

                if (!options.HeightMapping.TryUnmap(ref heightPixel, out var heightValue)) {
                    // no height data
                    continue;
                }

                if (!options.HeightMapping.ValueScale.Equal(1f))
                    heightValue *= options.HeightMapping.ValueScale;

                if (!(position.Z < 1f - heightValue)) continue;

                // hit, return 
                factor = (float)step / options.StepCount;
                factor = 1f - MathF.Pow(factor, options.HitPower);
                return true;
            }

            factor = 0f;
            return false;
        }

        private Vector3[] CreateRays()
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.StepCount < 1)
                throw new ArgumentOutOfRangeException(nameof(options.StepCount),
                    "Occlusion Step-Count must be greater than 0!");

            if (options.ZScale <= float.Epsilon)
                throw new ArgumentOutOfRangeException(nameof(options.ZScale),
                    "Occlusion Z-Scale must be greater than 0!");

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
                    result[z].Z = MathF.Sin(vAngleRadians) / options.ZScale;
                }
            }

            return result;
        }

        public class Options
        {
            public CancellationToken Token;

            public ISampler<THeight> HeightSampler;
            public TextureChannelMapping HeightMapping;

            public int StepCount;
            public float HitPower = 1.5f;
            public float ZScale;
            public float ZBias;
            public float Quality;
        }
    }
}
