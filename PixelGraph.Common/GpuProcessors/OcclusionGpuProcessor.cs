using ComputeSharp;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.GpuProcessors.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;
using ComputeSharp.__Internals;

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

            //options.HeightData = GetFloatBuffer<THeight>(options.he, options.HeightColor);

            rayList = new Lazy<Vector3[]>(CreateRays);
        }

        public void Process<TOcclusion>(Image<TOcclusion> outputImage, Rectangle outputBounds)
            where TOcclusion : unmanaged, IPixel<TOcclusion>
        {
            var outputLength = outputBounds.Width * outputBounds.Height;
            var heightData = GetFloatBuffer(options.HeightImage, options.HeightMapping.InputColor);
            
            using var gpuHeightBuffer = Gpu.Default.AllocateReadOnlyBuffer(heightData);
            using var gpuRayBuffer = Gpu.Default.AllocateReadOnlyBuffer(rayList.Value);
            using var gpuOutput = Gpu.Default.AllocateReadWriteBuffer<float>(outputLength);

            //var shaderOptions = new OcclusionShaderOptions {
            //    HeightMapping = new PixelMapping(options.HeightMapping),
            //    HeightBufferWidth = options.HeightImage.Width,
            //    HeightBufferHeight = options.HeightImage.Height,
            //    HeightBuffer = heightData,

            //    Quality = options.Quality,
            //    StepCount = options.StepCount,
            //    ZScale = options.ZScale,
            //    ZBias = options.ZBias,
            //    WrapX = options.WrapX,
            //    WrapY = options.WrapY,

            //    OutputBounds = outputBounds,
            //    Rays = rayList.Value,
            //};

            var pixelMapping = new PixelMapping(options.HeightMapping);
            var sampler = new GpuNearestSampler<float> {
                Buffer = gpuHeightBuffer,
                BufferWidth = options.HeightImage.Width,
                BufferHeight = options.HeightImage.Height,
                WrapX = options.WrapX,
                WrapY = options.WrapY,
                //RangeX = options.HeightRangeX,
                //RangeY = options.HeightRangeY,
            };

            var shader = new OcclusionComputeShader(
                pixelMapping,
                sampler,
                gpuRayBuffer,
                gpuOutput,
                options.StepCount,
                options.ZScale,
                options.ZBias,
                outputBounds.Width,
                outputBounds.Height);

            Gpu.Default.For(outputBounds.Width, outputBounds.Height, in shader);
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
                    var hAngleRadians = hAngleDegrees * MathEx.Deg2RadF;

                    var vAngleDegrees = v * vStepSize;
                    var vAngleRadians = vAngleDegrees * MathEx.Deg2RadF;

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
            //public PixelMapping HeightMapping;
            public Image<THeight> HeightImage;
            //public ColorChannel HeightColor;
            //public int HeightWidth;
            //public int HeightHeight;
            //public RectangleF HeightBounds;

            public int StepCount;
            public float ZScale;
            public float ZBias;
            public float Quality;
            public bool WrapX;
            public bool WrapY;
        }
    }

    //internal struct OcclusionShaderOptions
    //{
    //    //public TextureChannelMapping HeightMapping;
    //    public PixelMapping HeightMapping;
    //    //public ColorChannel HeightColor;
    //    public float[] HeightBuffer;
    //    public int HeightBufferWidth;
    //    public int HeightBufferHeight;
    //    //public RectangleF HeightBounds;

    //    //public IGpuSampler<float> HeightSampler;

    //    public Rectangle OutputBounds;
    //    public Vector3[] Rays;

    //    public int StepCount;
    //    public float ZScale;
    //    public float ZBias;
    //    public float Quality;
    //    public bool WrapX;
    //    public bool WrapY;
    //}

    [AutoConstructor]
    internal readonly partial struct OcclusionComputeShader : IComputeShader //, IDisposable
    {
        private const float HitPower = 1.5f;
        private const float HalfPixel = 0.5f - float.Epsilon;

        //private readonly OcclusionShaderOptions options;

        //public readonly ReadOnlyBuffer<float> gpuHeightBuffer;
        public readonly PixelMapping HeightMapping;
        public readonly IGpuSampler<float> heightSampler;
        public readonly ReadOnlyBuffer<Vector3> gpuRayBuffer;
        public readonly ReadWriteBuffer<float> gpuOutput;

        //public readonly Rectangle OutputBounds;
        //public readonly Vector3[] Rays;

        public readonly int StepCount;
        public readonly float ZScale;
        public readonly float ZBias;
        //public readonly float Quality;
        //public readonly bool WrapX;
        //public readonly bool WrapY;

        //public readonly int stepCount;
        //public readonly float zScale;
        //public readonly float zBias;
        ////private readonly Rectangle heightBounds;
        //public readonly PixelMapping heightMapping;
        ////private readonly ColorChannel heightColor;
        private readonly int outputWidth;
        private readonly int outputHeight;
        

        //public OcclusionComputeShader(
        //    //ReadOnlyBuffer<float> gpuHeightBuffer,
        //    IGpuSampler<float> heightSampler,
        //    ReadOnlyBuffer<Vector3> gpuRayBuffer,
        //    ReadWriteBuffer<float> gpuOutput)
        //{
        //    //this.options = options;
        //    this.heightSampler = heightSampler;
        //    this.gpuRayBuffer = gpuRayBuffer;
        //    this.gpuOutput = gpuOutput;

        //    var outputLength = options.OutputBounds.Width * options.OutputBounds.Height;

        //    gpuHeightBuffer = Gpu.Default.AllocateReadOnlyBuffer(options.HeightBuffer);
        //    gpuRayBuffer = Gpu.Default.AllocateReadOnlyBuffer(options.Rays);
        //    gpuOutput = Gpu.Default.AllocateReadWriteBuffer<float>(outputLength);

        //    heightSampler = new GpuNearestSampler<float> {
        //        Buffer = gpuHeightBuffer,
        //        BufferWidth = options.HeightBufferWidth,
        //        BufferHeight = options.HeightBufferHeight,
        //        WrapX = options.WrapX,
        //        WrapY = options.WrapY,
        //        //RangeX = options.HeightRangeX,
        //        //RangeY = options.HeightRangeY,
        //    };

        //    //stepCount = options.StepCount;
        //    //zScale = options.ZScale;
        //    //zBias = options.ZBias;
        //    ////heightBounds = options.HeightBounds;
        //    //heightMapping = options.HeightMapping;
        //    ////heightColor

        //    outputWidth = options.OutputBounds.Width;
        //    outputHeight = options.OutputBounds.Height;
        //}

        public void Execute()
        {
            //var (fx, fy) = GetTexCoord(ids.X, ids.Y);
            var fx = (ThreadIds.X + HalfPixel) / outputWidth;
            var fy = (ThreadIds.Y + HalfPixel) / outputHeight;
            heightSampler.Sample(in fx, in fy, out var heightPixel);

            if (HeightMapping.TryUnmap(in heightPixel, out var heightValue)) {
                var z = (float) (1d - heightValue) * ZScale + ZBias;
                var rayCount = gpuRayBuffer.Length;
                var rayCountFactor = 1d / rayCount;
                var position = new Vector3();

                var hitFactor = 0d;
                if (z < ZScale) {
                    for (var r = 0; r < rayCount; r++) {
                        position.X = ThreadIds.X;
                        position.Y = ThreadIds.Y;
                        position.Z = z;

                        if (RayTest(ref position, gpuRayBuffer[r], out var rayHitFactor))
                            hitFactor += rayHitFactor * rayCountFactor;
                    }
                }

                var i = ThreadIds.X * outputWidth + ThreadIds.Y;
                gpuOutput[i] = (float)(1d - hitFactor);
            }
        }

        public float[] GetData() => gpuOutput.ToArray();

        //public void Dispose()
        //{
        //    gpuHeightBuffer?.Dispose();
        //    gpuRayBuffer?.Dispose();
        //    gpuOutput?.Dispose();
        //}

        private bool RayTest(ref Vector3 position, in Vector3 ray, out float factor)
        {
            for (var step = 1; step <= StepCount; step++) {
                position += ray;

                if (position.Z >= ZScale) break;

                var (fx, fy) = GetTexCoord(in position.X, in position.Y);
                heightSampler.Sample(in fx, in fy, out var heightPixel);

                if (!HeightMapping.TryUnmap(in heightPixel, out var heightValue)) continue;
                if (!(position.Z < (1d - heightValue) * ZScale)) continue;

                // hit, return 
                factor = (float)step / StepCount;
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
