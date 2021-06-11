using PixelGraph.Common.Extensions;
using SharpDX;
using System;

namespace PixelGraph.UI.Internal.Preview
{
    public static class MinecraftTime
    {
        //public const int TimeMax = 24_000;


        public static void GetSunAngle(in float azimuth, in float roll, in float time, out Vector3 sunAngle)
        {
            var azimuthR = azimuth * MathEx.Deg2RadF;
            var pitchR = time * 360f * MathEx.Deg2RadF;
            var rollR = roll * MathEx.Deg2RadF;

            var q = Quaternion.RotationYawPitchRoll(azimuthR, pitchR, rollR);

            var forward = new Vector3(0, 0, -1);
            Vector3.Transform(ref forward, ref q, out sunAngle);
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
