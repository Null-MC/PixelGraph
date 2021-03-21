using ComputeSharp;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.GpuProcessors.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.GpuProcessors
{
    internal class OcclusionGpuProcessor<THeight> : GpuProcessorBase
        where THeight : unmanaged, IPixel<THeight>
    {
        private readonly Options options;
        private readonly Lazy<Vector3[]> rayList;


        public OcclusionGpuProcessor(Options options = null)
        {
            this.options = options ?? new Options();

            rayList = new Lazy<Vector3[]>(CreateRays);
        }

        public void Process<TOcclusion>(Image<TOcclusion> outputImage, Rectangle outputBounds)
            where TOcclusion : unmanaged, IPixel<TOcclusion>
        {
            using var shader = new Shader(options, outputBounds, rayList.Value);
            Gpu.Default.For(outputBounds.Width, outputBounds.Height, shader);
            FillImage(outputImage, shader.GetData());
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

        public class Options
        {
            public TextureChannelMapping HeightMapping;
            public Image<THeight> HeightImage;
            public int HeightWidth;
            public int HeightHeight;
            //public RectangleF HeightBounds;

            public int StepCount;
            public float ZScale;
            public float ZBias;
            public float Quality;
            public bool WrapX;
            public bool WrapY;
        }

        private readonly struct Shader : IComputeShader, IDisposable
        {
            private const float HitPower = 1.5f;

            private readonly ReadOnlyBuffer<float> gpuHeightBuffer;
            private readonly ReadOnlyBuffer<Vector3> gpuRayBuffer;
            private readonly ReadWriteBuffer<float> gpuOutput;

            private readonly int stepCount;
            private readonly float zScale;
            private readonly float zBias;
            //private readonly Rectangle heightBounds;
            private readonly TextureChannelMapping heightMapping;
            private readonly int outputWidth;
            private readonly int outputHeight;

            private readonly IGpuSampler<float> heightSampler;


            public Shader(Options options, Rectangle bounds, Vector3[] rays)
            {
                var heightBuffer = GetFloatBuffer(options.HeightImage, options.HeightMapping.InputColor);
                var outputLength = bounds.Width * bounds.Height;

                gpuHeightBuffer = Gpu.Default.AllocateReadOnlyBuffer(heightBuffer);
                gpuRayBuffer = Gpu.Default.AllocateReadOnlyBuffer(rays);
                gpuOutput = Gpu.Default.AllocateReadWriteBuffer<float>(outputLength);

                stepCount = options.StepCount;
                zScale = options.ZScale;
                zBias = options.ZBias;
                //heightBounds = options.HeightBounds;
                heightMapping = options.HeightMapping;

                heightSampler = new GpuNearestSampler<float> {
                    Buffer = gpuHeightBuffer,
                    BufferWidth = options.HeightWidth,
                    BufferHeight = options.HeightHeight,
                    WrapX = options.WrapX,
                    WrapY = options.WrapY,
                    //RangeX = options.HeightRangeX,
                    //RangeY = options.HeightRangeY,
                };

                outputWidth = bounds.Width;
                outputHeight = bounds.Height;
            }

            public void Execute(ThreadIds ids)
            {
                //var (fx, fy) = GetTexCoord(ids.X, ids.Y);
                var fx = (ids.X + HalfPixel) / outputWidth;
                var fy = (ids.Y + HalfPixel) / outputHeight;
                heightSampler.Sample(in fx, in fy, out var heightPixel);

                if (heightMapping.TryUnmap(ref heightPixel, out var heightValue)) {
                    var z = (float) (1d - heightValue) * zScale + zBias;
                    var rayCount = gpuRayBuffer.Size;
                    var rayCountFactor = 1d / rayCount;
                    var position = new Vector3();

                    var hitFactor = 0d;
                    if (z < zScale) {
                        for (var r = 0; r < rayCount; r++) {
                            position.X = ids.X;
                            position.Y = ids.Y;
                            position.Z = z;

                            if (RayTest(ref position, gpuRayBuffer[r], out var rayHitFactor))
                                hitFactor += rayHitFactor * rayCountFactor;
                        }
                    }

                    var i = ids.X * outputWidth + ids.Y;
                    gpuOutput[i] = (float)(1d - hitFactor);
                }
            }

            public float[] GetData() => gpuOutput.GetData();

            public void Dispose()
            {
                gpuHeightBuffer?.Dispose();
                gpuRayBuffer?.Dispose();
                gpuOutput?.Dispose();
            }

            private bool RayTest(ref Vector3 position, in Vector3 ray, out float factor)
            {
                for (var step = 1; step <= stepCount; step++) {
                    position += ray;

                    if (position.Z >= zScale) break;

                    var (fx, fy) = GetTexCoord(in position.X, in position.Y);
                    heightSampler.Sample(in fx, in fy, out var heightPixel);

                    if (!heightMapping.TryUnmap(ref heightPixel, out var heightValue)) continue;
                    if (!(position.Z < (1d - heightValue) * zScale)) continue;

                    // hit, return 
                    factor = (float)step / stepCount;
                    factor = 1f - Hlsl.Pow(factor, HitPower);
                    return true;
                }

                factor = 0f;
                return false;
            }

            //private (float fx, float fy) GetTexCoord(in int x, in int y)
            //{
            //    var fx = (x + HalfPixel) / outputWidth;
            //    var fy = (y + HalfPixel) / outputHeight;
            //    return (fx, fy);
            //}

            private (float fx, float fy) GetTexCoord(in float x, in float y)
            {
                var fx = (x + HalfPixel) / outputWidth;
                var fy = (y + HalfPixel) / outputHeight;
                return (fx, fy);
            }
        }
    }
}
