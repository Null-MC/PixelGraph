using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.PixelOperations;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace McPbrPipeline.Internal.ImageProcessors
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

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            var sourceRow = options.HeightSource.GetPixelRowSpan(context.Y);

            var pixelIn = new Rgba32();
            sourceRow[context.X].ToRgba32(ref pixelIn);
            pixelIn.GetNormalizedChannelValue(in options.HeightChannel, out var height);
            var z = height * options.ZScale;

            var hitCount = 0;
            var position = new Vector3(context.X, context.Y, z);
            var rayCount = rayList.Value.Length;
            for (var i = 0; i < rayCount; i++) {
                position.X = context.X;
                position.Y = context.Y;
                position.Z = z;

                if (RayTest(ref position, in rayList.Value[i], in context))
                    hitCount++;
            }

            MathEx.Saturate(1f - hitCount / (float)rayCount, out pixelOut.R);
            pixelOut.G = pixelOut.B = pixelOut.R;
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

                SampleHeightPoint(in position.X, in position.Y, in context, out var height);
                var sampleZ = height * options.ZScale;

                if (position.Z < sampleZ) return true;
            }

            return false;
        }

        private void SampleHeightPoint(in float x, in float y, in PixelContext context, out float height)
        {
            var px = (int) (x + 0.5f);
            var py = (int) (y + 0.5f);

            if (options.Wrap) context.Wrap(ref px, ref py);
            else context.Clamp(ref px, ref py);

            var row = options.HeightSource.GetPixelRowSpan(py);

            var pixel = new Rgba32();
            row[px].ToRgba32(ref pixel);
            pixel.GetNormalizedChannelValue(in options.HeightChannel, out height);
        }

        //private void SampleHeightLinear(in float x, in float y, in PixelContext context, out float height)
        //{
        //    var pxMin = (int)x;
        //    var pxMax = pxMin + 1;
        //    var pyMin = (int)y;
        //    var pyMax = pyMin + 1;

        //    var fx = x - pxMin;
        //    var fy = y - pyMin;

        //    if (options.Wrap) {
        //        context.Wrap(ref pxMin, ref pyMin);
        //        context.Wrap(ref pxMax, ref pyMax);
        //    }
        //    else {
        //        context.Clamp(ref pxMin, ref pyMin);
        //        context.Clamp(ref pxMax, ref pyMax);
        //    }

        //    var rowMin = options.Source.GetPixelRowSpan(pyMin);
        //    var rowMax = options.Source.GetPixelRowSpan(pyMax);

        //    var pixelMatrix = new Rgba32[4];
        //    rowMin[pxMin].ToRgba32(ref pixelMatrix[0]);
        //    rowMin[pxMax].ToRgba32(ref pixelMatrix[1]);
        //    rowMax[pxMin].ToRgba32(ref pixelMatrix[2]);
        //    rowMax[pxMax].ToRgba32(ref pixelMatrix[3]);

        //    var heightMatrix = new float[4];
        //    pixelMatrix[0].GetNormalizedChannelValue(in options.HeightChannel, out heightMatrix[0]);
        //    pixelMatrix[1].GetNormalizedChannelValue(in options.HeightChannel, out heightMatrix[1]);
        //    pixelMatrix[2].GetNormalizedChannelValue(in options.HeightChannel, out heightMatrix[2]);
        //    pixelMatrix[3].GetNormalizedChannelValue(in options.HeightChannel, out heightMatrix[3]);

        //    MathEx.Lerp(in heightMatrix[0], in heightMatrix[1], in fx, out var zMin);
        //    MathEx.Lerp(in heightMatrix[2], in heightMatrix[3], in fx, out var zMax);
        //    MathEx.Lerp(in zMin, in zMax, in fy, out height);
        //}

        public class Options
        {
            public Image<Rgba32> HeightSource;
            public Image<Rgba32> EmissiveSource;
            public ColorChannel EmissiveChannel;
            public ColorChannel HeightChannel;
            public int StepCount;
            public float ZScale;
            public float Quality;
            public bool Wrap;
        }
    }
}
