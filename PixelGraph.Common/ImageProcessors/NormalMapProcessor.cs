using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalMapProcessor<THeight> : PixelProcessor
        where THeight : unmanaged, IPixel<THeight>
    {
        private readonly Options options;

        public bool EnableFloatingPoint {get; set;} = true;


        public NormalMapProcessor(Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel<TPixel>(ref TPixel pixel, in PixelContext context)
        {
            Vector3 normal;
            switch (options.Method) {
                case NormalMapMethods.SobelHigh:
                    CalculateSobel3HighPass(in context, out normal);
                    break;
                case NormalMapMethods.SobelLow:
                    CalculateSobel3LowPass(in context, out normal);
                    break;
                case NormalMapMethods.Sobel3:
                    CalculateSobel3(in context, out normal);
                    break;
                default:
                    throw new ApplicationException($"Unsupported filter '{options.Method}'!");
            }

            var tp = new Vector4();
            tp.SetChannelValue(ColorChannel.Red, normal.X * 0.5f + 0.5f);
            tp.SetChannelValue(ColorChannel.Green, normal.Y * 0.5f + 0.5f);
            tp.SetChannelValue(ColorChannel.Blue, normal.Z);
            pixel.FromScaledVector4(tp);
        }

        protected override void ProcessPixel(ref Rgba32 pixel, in PixelContext context)
        {
            Vector3 normal;
            switch (options.Method) {
                case NormalMapMethods.SobelHigh:
                    CalculateSobel3HighPass(in context, out normal);
                    break;
                case NormalMapMethods.SobelLow:
                    CalculateSobel3LowPass(in context, out normal);
                    break;
                case NormalMapMethods.Sobel3:
                    CalculateSobel3(in context, out normal);
                    break;
                default:
                    throw new ApplicationException($"Unsupported filter '{options.Method}'!");
            }

            pixel.SetChannelValueScaledF(ColorChannel.Red, normal.X * 0.5f + 0.5f);
            pixel.SetChannelValueScaledF(ColorChannel.Green, normal.Y * 0.5f + 0.5f);
            pixel.SetChannelValueScaledF(ColorChannel.Blue, normal.Z);
        }

        private void CalculateSobel3(in PixelContext context, out Vector3 normal)
        {
            var k = new float[3, 3];
            PopulateKernel_3x3(ref k, in context);
            GetSobelDerivative(ref k, out var derivative);
            CalculateNormal(in derivative, out normal);
        }

        private void CalculateSobel3HighPass(in PixelContext context, out Vector3 normal)
        {
            var k = new float[3, 3];
            PopulateKernel_3x3(ref k, in context);

            ApplyHighPass(ref k[0,0], in k[1,1]);
            ApplyHighPass(ref k[1,0], in k[1,1]);
            ApplyHighPass(ref k[2,0], in k[1,1]);
            ApplyHighPass(ref k[0,1], in k[1,1]);
            ApplyHighPass(ref k[2,1], in k[1,1]);
            ApplyHighPass(ref k[0,2], in k[1,1]);
            ApplyHighPass(ref k[1,2], in k[1,1]);
            ApplyHighPass(ref k[2,2], in k[1,1]);

            GetSobelDerivative(ref k, out var derivative);
            CalculateNormal(in derivative, out normal);
        }

        private void CalculateSobel3LowPass(in PixelContext context, out Vector3 normal)
        {
            var k = new float[3, 3];
            PopulateKernel_3x3(ref k, in context);

            ApplyLowPass(ref k[0,0], in k[1,1]);
            ApplyLowPass(ref k[1,0], in k[1,1]);
            ApplyLowPass(ref k[2,0], in k[1,1]);
            ApplyLowPass(ref k[0,1], in k[1,1]);
            ApplyLowPass(ref k[2,1], in k[1,1]);
            ApplyLowPass(ref k[0,2], in k[1,1]);
            ApplyLowPass(ref k[1,2], in k[1,1]);
            ApplyLowPass(ref k[2,2], in k[1,1]);

            GetSobelDerivative(ref k, out var derivative);
            CalculateNormal(in derivative, out normal);
        }

        private void PopulateKernel_3x3(ref float[,] kernel, in PixelContext context)
        {
            var p = new Rgba32();

            for (var kY = 0; kY < 3; kY++) {
                var pY = context.Y + kY - 1;

                if (options.WrapY) context.WrapY(ref pY);
                else context.ClampY(ref pY);

                var row = options.Source.GetPixelRowSpan(pY);

                for (var kX = 0; kX < 3; kX++) {
                    var pX = context.X + kX - 1;

                    if (options.WrapX) context.WrapX(ref pX);
                    else context.ClampX(ref pX);

                    if (EnableFloatingPoint) {
                        var pixel = row[pX].ToScaledVector4();
                        pixel.GetChannelValue(in options.HeightChannel, out kernel[kX, kY]);
                    }
                    else {
                        row[pX].ToRgba32(ref p);
                        p.GetChannelValueScaled(in options.HeightChannel, out kernel[kX, kY]);
                    }
                }
            }
        }

        private void CalculateNormal(in Vector2 derivative, out Vector3 normal)
        {
            normal.X = derivative.X;
            normal.Y = derivative.Y;
            normal.Z = 1f / options.Strength;
            MathEx.Normalize(ref normal);
        }

        private static void GetSobelDerivative(ref float[,] kernel, out Vector2 derivative)
        {
            var topSide = kernel[0, 0] + 2f * kernel[1, 0] + kernel[2, 0];
            var bottomSide = kernel[0, 2] + 2f * kernel[1, 2] + kernel[2, 2];
            var leftSide = kernel[0, 0] + 2f * kernel[0, 1] + kernel[0, 2];
            var rightSide = kernel[2, 0] + 2f * kernel[2, 1] + kernel[2, 2];

            derivative.X = leftSide - rightSide;
            derivative.Y = topSide - bottomSide;
        }

        private static void ApplyHighPass(ref float value, in float level)
        {
            if (value < level) value = level;
        }

        private static void ApplyLowPass(ref float value, in float level)
        {
            if (value > level) value = level;
        }

        public class Options
        {
            public Image<THeight> Source;
            public ColorChannel HeightChannel;
            public NormalMapMethods Method;
            public float Strength = 1f;
            public bool WrapX = true;
            public bool WrapY = true;
        }
    }
}
