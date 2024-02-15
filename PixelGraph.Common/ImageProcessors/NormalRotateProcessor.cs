using PixelGraph.Common.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors;

internal class NormalRotateProcessor
{
    private bool hasRotation, hasNoise;
    private float offsetTop, offsetBottom, offsetLeft, offsetRight;
    private float invRadiusTop, invRadiusBottom, invRadiusLeft, invRadiusRight;

    public Rectangle Bounds;
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


    public void Apply(Image image)
    {
        var hasCurveTop = MathF.Abs(CurveTop) > float.Epsilon && RadiusTop > float.Epsilon;
        var hasCurveBottom = MathF.Abs(CurveBottom) > float.Epsilon && RadiusBottom > float.Epsilon;
        var hasCurveLeft = MathF.Abs(CurveLeft) > float.Epsilon && RadiusLeft > float.Epsilon;
        var hasCurveRight = MathF.Abs(CurveRight) > float.Epsilon && RadiusRight > float.Epsilon;

        hasRotation = hasCurveTop || hasCurveBottom || hasCurveLeft || hasCurveRight;
        hasNoise = Noise > float.Epsilon;

        if (hasCurveTop) {
            offsetTop = 1f - RadiusTop;
            invRadiusTop = 1f / RadiusTop;
        }
        else invRadiusTop = offsetTop = 0f;

        if (hasCurveBottom) {
            offsetBottom = 1f - RadiusBottom;
            invRadiusBottom = 1f / RadiusBottom;
        }
        else invRadiusBottom = offsetBottom = 0f;

        if (hasCurveLeft) {
            offsetLeft = 1f - RadiusLeft;
            invRadiusLeft = 1f / RadiusLeft;
        }
        else invRadiusLeft = offsetLeft = 0f;

        if (hasCurveRight) {
            offsetRight = 1f - RadiusRight;
            invRadiusRight = 1f / RadiusRight;
        }
        else invRadiusRight = offsetRight = 0f;

        image.Mutate(c => {
            c.ProcessPixelRowsAsVector4(ProcessRow, Bounds);
        });
    }

    private void ProcessRow(Span<Vector4> row, Point pos)
    {
        byte[]? noiseX = null, noiseY = null;

        if (hasNoise) {
            GenerateNoise(Bounds.Width, out noiseX);
            GenerateNoise(Bounds.Width, out noiseY);
        }

        Vector3 vec, normal;
        for (var x = 0; x < Bounds.Width; x++) {
            vec.X = row[x].X * 2f - (254f/255f);
            vec.Y = row[x].Y * 2f - (254f/255f);
            vec.Z = row[x].Z;

            NormalEncoding.DecodeNormal(in vec, RestoreNormalZ, out normal);

            ProcessPixel(x, pos.Y - Bounds.Y, ref normal, in noiseX, in noiseY);

            NormalEncoding.EncodeNormal(in normal, out vec);

            row[x].X = vec.X * 0.5f + (127f/255f);
            row[x].Y = vec.Y * 0.5f + (127f/255f);
            row[x].Z = vec.Z;
        }
    }

    private void ProcessPixel(in int x, in int y, ref Vector3 v, in byte[]? noiseX, in byte[]? noiseY)
    {
        if (!hasRotation && !hasNoise) return;

        var fx = (x + 0.5f) / Bounds.Width * 2f - 1f;
        var fy = (y + 0.5f) / Bounds.Height * 2f - 1f;

        var f_top = MathF.Min(fy + offsetTop, 0f) * invRadiusTop;
        var f_bottom = MathF.Max(fy - offsetBottom, 0f) * invRadiusBottom;
        var f_left = MathF.Min(fx + offsetLeft, 0f) * invRadiusLeft;
        var f_right = MathF.Max(fx - offsetRight, 0f) * invRadiusRight;

        var baseRotation = Vector3.One;
        baseRotation.X = f_left + f_right;
        baseRotation.Y = f_top + f_bottom;

        var q = Quaternion.Identity;
        if (!baseRotation.X.NearEqual(0f) || !baseRotation.Y.NearEqual(0f)) {
            var rotationAxis = new Vector3 {
                X = -baseRotation.Y,
                Y = baseRotation.X,
                Z = 0f,
            };

            MathEx.Normalize(ref rotationAxis);
            MathEx.Normalize(ref baseRotation);
            var baseAngle = MathF.Acos(baseRotation.Z);

            var mix = MathF.Abs(MathF.Asin(rotationAxis.X) / (MathF.PI / 2f));
            var angle_x = MathF.Sign(-f_left) * CurveLeft + MathF.Sign(f_right) * CurveRight;
            var angle_y = MathF.Sign(-f_top) * CurveTop + MathF.Sign(f_bottom) * CurveBottom;
            MathEx.Lerp(angle_x, angle_y, mix, out var angle);
                    
            q = Quaternion.CreateFromAxisAngle(rotationAxis, baseAngle * (angle / 45f));
        }

        if (hasNoise) {
            q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, ((noiseX?[x] ?? 127) / 127.5f - 1f) * Noise * MathEx.Deg2RadF);
            q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, ((noiseY?[x] ?? 127) / 127.5f - 1f) * -Noise * MathEx.Deg2RadF);
        }

        v = Vector3.Transform(v, q);
        //else if (RestoreNormalZ) {
        //    var v2 = new Vector2(v.X, v.Y);
        //    var d = Vector2.Dot(v2, v2);
        //    MathEx.Clamp(ref d, 0f, 1f);
        //    v.Z = MathF.Sqrt(1f - d);
        //}

        //MathEx.Normalize(ref v);
    }

    private static void GenerateNoise(in int size, out byte[] buffer)
    {
        var random = new Random();
        buffer = new byte[size];
        random.NextBytes(buffer);
    }
}