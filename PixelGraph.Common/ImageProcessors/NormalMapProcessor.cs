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
        private readonly float[,] _operator;
        private readonly byte kernelSize;

        public bool EnableFloatingPoint {get; set;} = true;


        public NormalMapProcessor(Options options)
        {
            this.options = options;

            switch (options.Method) {
                case NormalMapMethods.Sobel9:
                    BuildSobelOperator(out _operator, 9);
                    break;
                case NormalMapMethods.Sobel5:
                    BuildSobelOperator(out _operator, 5);
                    break;
                default:
                    BuildSobelOperator(out _operator, 3);
                    break;
            }

            kernelSize = options.Method switch {
                NormalMapMethods.Sobel9 => 9,
                NormalMapMethods.Sobel5 => 5,
                _ => 3,
            };
        }

        protected override void ProcessPixel<TPixel>(ref TPixel pixel, in PixelContext context)
        {
            ProcessPixelNormal(in context, out var normal);

            var tp = new Vector4();
            tp.SetChannelValue(ColorChannel.Red, normal.X * 0.5f + 0.5f);
            tp.SetChannelValue(ColorChannel.Green, normal.Y * 0.5f + 0.5f);
            tp.SetChannelValue(ColorChannel.Blue, normal.Z * 0.5f + 0.5f);
            pixel.FromScaledVector4(tp);
        }

        protected override void ProcessPixel(ref Rgba32 pixel, in PixelContext context)
        {
            ProcessPixelNormal(in context, out var normal);

            pixel.SetChannelValueScaledF(ColorChannel.Red, normal.X * 0.5f + 0.5f);
            pixel.SetChannelValueScaledF(ColorChannel.Green, normal.Y * 0.5f + 0.5f);
            pixel.SetChannelValueScaledF(ColorChannel.Blue, normal.Z * 0.5f + 0.5f);
        }

        private void ProcessPixelNormal(in PixelContext context, out Vector3 normal)
        {
            var k = new float[kernelSize, kernelSize];
            PopulateKernel(ref k, in context);

            if (options.Method == NormalMapMethods.SobelHigh) {
                ApplyHighPass(ref k[0,0], in k[1,1]);
                ApplyHighPass(ref k[1,0], in k[1,1]);
                ApplyHighPass(ref k[2,0], in k[1,1]);
                ApplyHighPass(ref k[0,1], in k[1,1]);
                ApplyHighPass(ref k[2,1], in k[1,1]);
                ApplyHighPass(ref k[0,2], in k[1,1]);
                ApplyHighPass(ref k[1,2], in k[1,1]);
                ApplyHighPass(ref k[2,2], in k[1,1]);
            }
            else if (options.Method == NormalMapMethods.SobelLow) {
                ApplyLowPass(ref k[0,0], in k[1,1]);
                ApplyLowPass(ref k[1,0], in k[1,1]);
                ApplyLowPass(ref k[2,0], in k[1,1]);
                ApplyLowPass(ref k[0,1], in k[1,1]);
                ApplyLowPass(ref k[2,1], in k[1,1]);
                ApplyLowPass(ref k[0,2], in k[1,1]);
                ApplyLowPass(ref k[1,2], in k[1,1]);
                ApplyLowPass(ref k[2,2], in k[1,1]);
            }

            ApplyOperator(ref k, in _operator);
            GetSobelDerivative(ref k, out var derivative);
            CalculateNormal(in derivative, out normal);
        }

        private void PopulateKernel(ref float[,] kernel, in PixelContext context)
        {
            var p = new Rgba32();

            var ox = (kernelSize - 1) / 2;
            var oy = (kernelSize - 1) / 2;

            for (byte kY = 0; kY < kernelSize; kY++) {
                var pY = context.Y + kY - oy;

                if (options.WrapY) context.WrapY(ref pY);
                else context.ClampY(ref pY);

                var row = options.Source.GetPixelRowSpan(pY);

                for (byte kX = 0; kX < kernelSize; kX++) {
                    var pX = context.X + kX - ox;

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

        private void GetSobelDerivative(ref float[,] kernel, out Vector2 derivative)
        {
            byte i;
            float value;
            var c = (kernelSize - 1) / 2;

            derivative.X = 0f;
            for (i = 0; i < kernelSize; i++) {
                if (i == c) continue;

                GetColSum(in kernel, in kernelSize, in i, out value);

                if (i < c) derivative.X += value;
                else derivative.X -= value;
            }

            derivative.Y = 0f;
            for (i = 0; i < kernelSize; i++) {
                if (i == c) continue;

                GetRowSum(in kernel, in kernelSize, in i, out value);

                if (i < c) derivative.Y += value;
                else derivative.Y -= value;
            }
        }

        private static void BuildSobelOperator(out float[,] kernel, in byte size)
        {
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size));
            
            kernel = new float[size, size];
            var c = (size - 1) / 2;

            byte kX, kY;
            float i, j, f;
            for (kY = 0; kY < size; kY++) {
                for (kX = 0; kX < size; kX++) {
                    if (kX == c && kY == c) continue;

                    i = MathF.Abs(kX - c);
                    j = MathF.Abs(kY - c);
                    f = i > j ? i : j;
                    kernel[kX, kY] = f / (i * i + j * j);
                }
            }
        }

        private void ApplyOperator(ref float[,] kernel, in float[,] @operator)
        {
            var c = (kernelSize - 1) / 2;

            byte kX, kY;
            for (kY = 0; kY < kernelSize; kY++) {
                for (kX = 0; kX < kernelSize; kX++) {
                    if (kX == c && kY == c) continue;

                    kernel[kX, kY] *= @operator[kX, kY];
                }
            }
        }

        //private static void ApplyHighPass(ref float[,] kernel, in byte size)
        //{
        //    if (kernel == null) throw new ArgumentNullException(nameof(kernel));
        //    if (size < 1) throw new ArgumentOutOfRangeException(nameof(size));

        //    var c = (size - 1) / 2;

        //    for (byte kY = 0; kY < size; kY++) {
        //        for (byte kX = 0; kX < size; kX++) {
        //            if (kX == c && kY == c) continue;
        //            ApplyHighPass(ref kernel[kX, kY], in kernel[c, c]);
        //        }
        //    }
        //}

        private static void GetRowSum(in float[,] kernel, in byte size, in byte row, out float value)
        {
            value = 0f;
            for (var i = 0; i < size; i++)
                value += kernel[i, row];
        }

        private static void GetColSum(in float[,] kernel, in byte size, in byte col, out float value)
        {
            value = 0f;
            for (var i = 0; i < size; i++)
                value += kernel[col, i];
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
