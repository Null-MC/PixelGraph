using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalRotateProcessor : PixelRowProcessor
    {
        private readonly Options options;
        private readonly bool hasRotation, hasNoise;
        //private readonly bool hasCurveTop, hasCurveBottom, hasCurveLeft, hasCurveRight;
        private readonly float offsetTop, offsetBottom, offsetLeft, offsetRight;
        private readonly float invRadiusTop, invRadiusBottom, invRadiusLeft, invRadiusRight;


        public NormalRotateProcessor(Options options)
        {
            this.options = options;

            var hasCurveTop = MathF.Abs(options.CurveTop) > float.Epsilon && options.RadiusTop > float.Epsilon;
            var hasCurveBottom = MathF.Abs(options.CurveBottom) > float.Epsilon && options.RadiusBottom > float.Epsilon;
            var hasCurveLeft = MathF.Abs(options.CurveLeft) > float.Epsilon && options.RadiusLeft > float.Epsilon;
            var hasCurveRight = MathF.Abs(options.CurveRight) > float.Epsilon && options.RadiusRight > float.Epsilon;

            hasRotation = hasCurveTop || hasCurveBottom || hasCurveLeft || hasCurveRight;
            hasNoise = options.Noise > float.Epsilon;

            if (hasCurveTop) {
                offsetTop = 1f - options.RadiusTop;
                invRadiusTop = 1f / options.RadiusTop;
            }
            else invRadiusTop = offsetTop = 0f;

            if (hasCurveBottom) {
                offsetBottom = 1f - options.RadiusBottom;
                invRadiusBottom = 1f / options.RadiusBottom;
            }
            else invRadiusBottom = offsetBottom = 0f;

            if (hasCurveLeft) {
                offsetLeft = 1f - options.RadiusLeft;
                invRadiusLeft = 1f / options.RadiusLeft;
            }
            else invRadiusLeft = offsetLeft = 0f;

            if (hasCurveRight) {
                offsetRight = 1f - options.RadiusRight;
                invRadiusRight = 1f / options.RadiusRight;
            }
            else invRadiusRight = offsetRight = 0f;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixel = new Rgba32();
            byte[] noiseX = null, noiseY = null;

            if (hasNoise) {
                GenerateNoise(context.Bounds.Width, out noiseX);
                GenerateNoise(context.Bounds.Width, out noiseY);
            }

            var v = new Vector3();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                if (x < 0 || x >= row.Length) continue;

                row[x].ToRgba32(ref pixel);
                pixel.GetChannelValue(ColorChannel.Red, out var normalX);
                pixel.GetChannelValue(ColorChannel.Green, out var normalY);

                v.X = Math.Clamp(normalX / 127f - 1f, -1f, 1f);
                v.Y = Math.Clamp(normalY / 127f - 1f, -1f, 1f);

                if (!options.RestoreNormalZ) {
                    pixel.GetChannelValue(ColorChannel.Blue, out var normalZ);
                    v.Z = Math.Clamp(normalZ / 127f - 1f, -1f, 1f);
                    MathEx.Normalize(ref v);
                }

                ProcessPixel(in context, ref v, in noiseX, in noiseY, in x);

                pixel.SetChannelValueScaledF(ColorChannel.Red, v.X * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Blue, v.Z * 0.5f + 0.5f);

                row[x].FromRgba32(pixel);
            }
        }

        private void ProcessPixel(in PixelRowContext context, ref Vector3 v, in byte[] noiseX, in byte[] noiseY, in int x)
        {
            if (hasRotation || hasNoise) {
                var fx = (x - context.Bounds.X + 0.5f) / context.Bounds.Width * 2f - 1f;
                var fy = (context.Y - context.Bounds.Y + 0.5f) / context.Bounds.Height * 2f - 1f;

                var rotation_top = MathF.Min(fy + offsetTop, 0f) * invRadiusTop;
                var rotation_bottom = MathF.Max(fy - offsetBottom, 0f) * invRadiusBottom;
                var rotation_left = MathF.Min(fx + offsetLeft, 0f) * invRadiusLeft;
                var rotation_right = MathF.Max(fx - offsetRight, 0f) * invRadiusRight;

                var angle_top =  MathF.Sin(rotation_top *options.CurveTop * 2f * MathEx.Deg2RadF);
                var angle_bottom = MathF.Sin(rotation_bottom * options.CurveBottom * 2f * MathEx.Deg2RadF);
                var angle_left = MathF.Sin(rotation_left * options.CurveLeft * 2f * MathEx.Deg2RadF);
                var angle_right = MathF.Sin(rotation_right * options.CurveRight * 2f * MathEx.Deg2RadF);

                var direction = Vector3.Zero;
                direction.Y = -(angle_left + angle_right);
                direction.X = angle_top + angle_bottom;

                if (!direction.X.NearEqual(0f) || !direction.Y.NearEqual(0f)) {
                    var len_x = -(rotation_left + rotation_right);
                    var len_y = rotation_top + rotation_bottom;

                    var len = direction.X*direction.X + direction.Y*direction.Y;
                    //var len = MathF.Abs(direction.X) + MathF.Abs(direction.Y);
                    len = MathF.Sqrt(len);
                    len = MathF.Min(len, 1f);

                    //var max = MathF.Abs(len_x) + MathF.Abs(len_y);
                    var max = len_x*len_x + len_y*len_y;
                    max = MathF.Sqrt(max);
                    max = MathF.Max(max, 1f);

                    //len = MathF.Sqrt(len / max);
                    //len = MathF.Min(len / (1f + max), 1f);

                    var rotation = -MathF.Asin(len);

                    MathEx.Normalize(ref direction);
                    //var axis = Vector3.Cross(direction, Vector3.UnitZ);
                    //var axis = Vector3.Normalize(new Vector3(direction.Y, direction.X, 0f));

                    var q = Quaternion.CreateFromAxisAngle(direction, rotation);

                    if (hasNoise) {
                        var z = x - context.Bounds.Left;
                        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, (noiseX[z] / 127.5f - 1f) * options.Noise * MathEx.Deg2RadF);
                        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, (noiseY[z] / 127.5f - 1f) * -options.Noise * MathEx.Deg2RadF);
                    }

                    v = Vector3.Transform(v, q);
                }
            }
            else if (options.RestoreNormalZ) {
                var v2 = new Vector2(v.X, v.Y);
                var d = Vector2.Dot(v2, v2);
                MathEx.Clamp(ref d, 0f, 1f);
                v.Z = MathF.Sqrt(1f - d);
            }

            MathEx.Normalize(ref v);
        }

        private static void GenerateNoise(in int size, out byte[] buffer)
        {
            var random = new Random();
            buffer = new byte[size];
            random.NextBytes(buffer);
        }

        public class Options
        {
            public bool RestoreNormalZ;
            public float CurveTop = 0f;
            public float CurveBottom = 0f;
            public float CurveLeft = 0f;
            public float CurveRight = 0f;
            public float RadiusTop = 1f;
            public float RadiusBottom = 1f;
            public float RadiusLeft = 1f;
            public float RadiusRight = 1f;
            public float Noise = 0f;
        }
    }
}
