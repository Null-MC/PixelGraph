using System;
using PixelGraph.Common.Extensions;
using SharpDX;

namespace PixelGraph.UI.Helix
{
    public static class MinecraftTime
    {
        //public const int TimeMax = 24_000;


        public static void GetSunAngle(in float azimuth, in float roll, in float time, out Vector3 sunAngle)
        {
            //var azimuthR = azimuth * MathEx.Deg2RadF;
            //var pitchR = time * 360f * MathEx.Deg2RadF;
            //var rollR = roll * MathEx.Deg2RadF;

            //var q = Quaternion.RotationAxis(Vector3.Up, azimuthR)
            //    * Quaternion.RotationAxis(Vector3.Right, pitchR)
            //    * Quaternion.RotationAxis(Vector3.ForwardRH, rollR);

            //var forward = new Vector3(0, 0, -1);
            //Vector3.Transform(ref forward, ref q, out sunAngle);

            Vector2 sunRotationData;
            sunRotationData.X = MathEx.CosD(roll);
            sunRotationData.Y = -MathEx.SinD(roll);

            //minecraft's native calculateCelestialAngle() function, ported to GLSL.
            var ang = (time - 0.25f) % 1f;
            ang = (ang + (MathF.Cos(ang * 3.14159265358979f) * -0.5f + 0.5f - ang) / 3f) * 6.28318530717959f; //0-2pi, rolls over from 2pi to 0 at noon.

            sunAngle.X = -MathF.Sin(ang);
            sunAngle.Y = MathF.Cos(ang) * sunRotationData.X;
            sunAngle.Z = MathF.Cos(ang) * sunRotationData.Y;
            sunAngle.Normalize();
        }

        public static float GetSunStrength(in float time, in float overlap, in float power)
        {
            var s = MathEx.SinD(time * 360f);
            var f = MathF.Max(s * (1f - overlap) + overlap, 0f);
            return MathF.Pow(f, power);
        }
    }
}
