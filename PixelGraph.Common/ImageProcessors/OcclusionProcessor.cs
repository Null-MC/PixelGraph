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
            var bias = options.ZBias / 100f;
            var position = new Vector3();

            var zScale = options.ZScale; // * options.HeightMapping.ValueScale;
            var hasZScale = !zScale.Equal(1f);

            float fx, fy, heightPixel, heightValue, rayHitFactor;
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                options.Token.ThrowIfCancellationRequested();

                GetTexCoord(in context, in x, out fx, out fy);
                options.HeightSampler.SampleScaled(in fx, in fy, in options.HeightMapping.InputColor, out heightPixel);
                if (!options.HeightMapping.TryUnmap(in heightPixel, out heightValue)) continue;

                MathEx.Invert(ref heightValue, 0f, 1f);
                if (hasZScale) heightValue *= zScale;

                heightValue += bias * zScale;

                var hitFactor = 0f;
                //if (!z.Equal(in zScale)) {
                    for (var i = 0; i < rayCount; i++) {
                        options.Token.ThrowIfCancellationRequested();

                        position.X = x;
                        position.Y = context.Y;
                        position.Z = heightValue;

                        if (RayTest(in context, ref position, in rayList.Value[i], out rayHitFactor))
                            hitFactor += rayHitFactor * rayCountFactor;
                    }
                //}

                MathEx.Saturate(1f - hitFactor, out pixelOut.R);
                pixelOut.B = pixelOut.G = pixelOut.R;
                row[x].FromRgba32(pixelOut);
            }
        }

        private bool RayTest(in PixelRowContext context, ref Vector3 position, in Vector3 ray, out float factor)
        {
            var zScale = options.ZScale; // * options.HeightMapping.ValueScale;
            var hasZScale = !zScale.Equal(1f);
            var hasHitPower = !options.HitPower.Equal(1f);

            float fx, fy, heightPixel, heightValue;
            for (var step = 1; step <= options.StepCount; step++) {
                position.Add(in ray);

                if (position.Z >= zScale) break;

                GetTexCoord(in context, in position.X, in position.Y, out fx, out fy);
                options.HeightSampler.SampleScaled(in fx, in fy, options.HeightMapping.InputColor, out heightPixel);
                if (!options.HeightMapping.TryUnmap(in heightPixel, out heightValue)) continue;

                MathEx.Invert(ref heightValue, 0f, 1f);
                if (hasZScale) heightValue *= zScale;

                if (position.Z - heightValue > -float.Epsilon) continue;

                // hit, return 
                var hit = (float)step / options.StepCount;

                if (hasHitPower)
                    factor = 1 - MathF.Pow(hit, options.HitPower);
                else
                    factor = 1 - hit;

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

            //var zScale = options.ZScale * options.HeightMapping.ValueScale;

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

        public class Options
        {
            public CancellationToken Token;

            public ISampler<THeight> HeightSampler;
            public TextureChannelMapping HeightMapping;

            public float Quality;
            public int StepCount;
            public float HitPower = 1.5f;
            public float ZScale;
            public float ZBias;
        }
    }
}
