using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OcclusionGeneratorX
    {
        private const int TileSize = 8;

        private readonly CancellationToken token;
        private readonly ISampler heightSampler;
        private readonly ColorChannel heightInputColor;
        private readonly PixelMapping heightMapping;
        private readonly Vector3[] rayList;
        private readonly int rayCount, stepCount;
        private readonly float rayCountFactor;
        private readonly float bias, zScale, hitPower;
        private readonly bool hasZScale, hasHitPower;

        private float[] nearField;
        private int nearFieldWidth;
        private int nearFieldHeight;

        //[ThreadStatic]
        //private static Vector3 position;

        //[ThreadStatic]
        //private static float heightValue, heightPixelValue, ray_HitFactor;

        //[ThreadStatic]
        //private static int ray_step;

        public Rectangle Bounds {get; set;}


        public OcclusionGeneratorX(OcclusionProcessorOptions options)
        {
            token = options.Token;
            heightSampler = options.HeightSampler;
            heightInputColor = options.HeightInputColor;
            heightMapping = options.HeightMapping;

            CreateRays(options, ref rayList);

            rayCount = rayList.Length;
            rayCountFactor = 1f / (rayCount + 1);
            bias = options.ZBias / 100f;

            zScale = options.ZScale;
            hasZScale = !zScale.NearEqual(1f);

            hitPower = options.HitPower;
            hasHitPower = !options.HitPower.NearEqual(1f);

            stepCount = options.StepCount;
        }

        public void PopulateNearField<TPixel>(Image<TPixel> heightImage)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            nearFieldWidth = (int)Math.Ceiling(Bounds.Width / (double)TileSize);
            nearFieldHeight = (int)Math.Ceiling(Bounds.Height / (double)TileSize);
            nearField = new float[nearFieldWidth * nearFieldHeight];

            int iy, ix, gy, gx;
            for (iy = 0; iy < nearFieldHeight; iy++) {
                var groupRowCount = Math.Min(TileSize, Bounds.Height - iy - 1);
                var groupRows = Enumerable.Range(0, groupRowCount)
                    .Select(i => heightImage.DangerousGetPixelRowMemory(Bounds.Y + iy*TileSize + i)
                        .Slice(Bounds.X, Bounds.Width)).ToArray();

                for (ix = 0; ix < nearFieldWidth; ix++) {
                    var groupColCount = Math.Min(TileSize, Bounds.Width - ix - 1);
                    var groupMax = 0f;

                    for (gy = 0; gy < groupRowCount; gy++) {
                        var groupRowSlice = groupRows[gy].Slice(ix * TileSize, groupColCount).Span;

                        for (gx = 0; gx < groupColCount; gx++) {
                            var heightPixel = groupRowSlice[gx].ToScaledVector4();
                            heightPixel.GetChannelValue(in heightInputColor, out var heightPixelValue);
                            if (!heightMapping.TryUnmap(in heightPixelValue, out var heightValue)) continue;

                            MathEx.InvertRef(ref heightValue, 0f, 1f);
                            if (heightValue > groupMax) groupMax = heightValue;
                        }
                    }

                    if (hasZScale) groupMax *= zScale;

                    nearField[iy * nearFieldWidth + ix] = groupMax;
                }
            }
        }

        public void ProcessRow<TPixel>(in PixelRowContext context, Span<TPixel> row)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var position = new Vector3();
            var pixelOut = new L16(0);

            GetTexCoordY(context.Y, out var rfy);
            var heightRowSampler = heightSampler.ForRow(in rfy);

            int i;
            float hitFactor, heightPixel, heightValue;
            double fx, fy;
            for (var x = 0; x < context.Bounds.Width; x++) {
                token.ThrowIfCancellationRequested();

                GetTexCoordX(context.Bounds.Left + x, out fx);
                GetTexCoordY(context.Y, out fy);

                heightRowSampler.SampleScaled(in fx, in fy, in heightInputColor, out heightPixel);
                if (!heightMapping.TryUnmap(in heightPixel, out heightValue)) continue;

                MathEx.InvertRef(ref heightValue, 0f, 1f);
                if (hasZScale) heightValue *= zScale;
                heightValue += bias * zScale;

                hitFactor = 1f;
                for (i = 0; i < rayCount; i++) {
                    //token.ThrowIfCancellationRequested();

                    position.X = context.Bounds.Left + x;
                    position.Y = context.Y;
                    position.Z = heightValue;

                    if (RayTest(ref position, in i, out var ray_HitFactor))
                        hitFactor -= ray_HitFactor * rayCountFactor;
                }

                //GetNearFieldIndex(out var nfIndex);
                //var z = nearField[nfIndex] / zScale;

                MathEx.Clamp(ref hitFactor, float.Epsilon, 1f - float.Epsilon);
                pixelOut.PackedValue = (ushort)(hitFactor * ushort.MaxValue);
                row[x].FromL16(pixelOut);
            }
        }

        private bool RayTest(ref Vector3 position, in int rayIndex, out float factor)
        {
            double fx, fy;
            int nfIndex;
            float heightPixel, heightValue;

            for (var ray_step = 1; ray_step <= stepCount; ray_step++) {
                position.Add(in rayList[rayIndex]);

                if (position.Z >= zScale) break;

                GetNearFieldIndex(in position.X, in position.Y, out nfIndex);
                if (position.Z - nearField[nfIndex] > float.Epsilon) continue;

                //GetTexCoord(in context, in position.X, in position.Y, out fx, out fy);
                GetTexCoordX(position.X, out fx);
                GetTexCoordY(position.Y, out fy);
                heightSampler.SampleScaled(in fx, in fy, heightInputColor, out heightPixel);
                if (!heightMapping.TryUnmap(in heightPixel, out heightValue)) continue;

                MathEx.InvertRef(ref heightValue, 0f, 1f);
                if (hasZScale) heightValue *= zScale;

                if (position.Z - heightValue > -float.Epsilon) continue;

                // hit, return 
                factor = 1f - ray_step / (float)stepCount;

                if (hasHitPower) factor = 1f - MathF.Pow(1f - factor, hitPower);

                return true;
            }

            factor = 0f;
            return false;
        }

        private void GetTexCoordX(in float x, out double fx)
        {
            fx = (x - Bounds.X + 0.5f) / Bounds.Width;
        }

        private void GetTexCoordY(in float y, out double fy)
        {
            fy = (y - Bounds.Y + 0.5f) / Bounds.Height;
        }

        private void GetNearFieldIndex(in float x, in float y, out int index)
        {
            var nx = (int)Math.Floor(x / (double)TileSize);
            var ny = (int)Math.Floor(y / (double)TileSize);

            MathEx.WrapExclusive(ref nx, 0, nearFieldWidth);
            MathEx.WrapExclusive(ref ny, 0, nearFieldHeight);

            index = ny * nearFieldWidth + nx;
        }

        private static void CreateRays(OcclusionProcessorOptions options, ref Vector3[] rays)
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

            var hStepCount = (1 + (int)(options.Quality * 89f)) * 4;
            var vStepCount = 1 + (int)(options.Quality * 89f);

            var hStepSize = 360f / hStepCount;
            var vStepSize = 90f / vStepCount;

            var count = hStepCount * vStepCount;
            rays = new Vector3[count];

            //var zScale = options.ZScale * options.HeightMapping.ValueScale;
            int v, h, z;
            float hAngleDegrees, vAngleDegrees;
            for (v = 0; v < vStepCount; v++) {
                for (h = 0; h < hStepCount; h++) {
                    hAngleDegrees = (h + 0.5f) * hStepSize - 180f;
                    vAngleDegrees = (v + 0.5f) * vStepSize;

                    z = hStepCount * v + h;
                    MathEx.CosD(in hAngleDegrees, out rays[z].X);
                    MathEx.SinD(in hAngleDegrees, out rays[z].Y);
                    MathEx.SinD(in vAngleDegrees, out rays[z].Z);

                    //MathEx.Normalize(ref result[z]);
                }
            }
        }
    }

    internal class OcclusionProcessor<THeight> : PixelRowProcessor
        where THeight : unmanaged, IPixel<THeight>
    {
        //private readonly OcclusionProcessorOptions options;
        private readonly OcclusionGeneratorX generator;


        public OcclusionProcessor(OcclusionProcessorOptions options)
        {
            //this.options = options ?? throw new ArgumentNullException(nameof(options));

            generator = new OcclusionGeneratorX(options);
        }

        //public void Generate<TPixel>(Image<TPixel> image, Rectangle bounds)
        //    where TPixel : unmanaged, IPixel<TPixel>
        //{
        //    options.PopulateNearField(bounds);

        //    image.Mutate(c => c.ApplyProcessor(this, bounds));
        //}
        public void PopulateNearField(Image<THeight> heightImage, in Rectangle bounds)
        {
            generator.Bounds = bounds;
            generator.PopulateNearField(heightImage);
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            generator.ProcessRow(in context, row);
        }
    }

    internal class OcclusionProcessorOptions
    {
        public CancellationToken Token;

        public ISampler HeightSampler;
        public ColorChannel HeightInputColor;
        public PixelMapping HeightMapping;

        public float Quality;
        public int StepCount;
        public float HitPower = 1.5f;
        public float ZScale;
        public float ZBias;
    }
}
