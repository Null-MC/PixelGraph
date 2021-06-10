using PixelGraph.Common.Extensions;
using SharpDX;
using MathF = System.MathF;

namespace PixelGraph.UI.Internal.Preview
{
    public static class MinecraftTime
    {
        public const int TimeMax = 24_000;


        public static void GetSunAngle(in int time, in float roll, out Vector3 sunAngle) =>
            GetSunAngle(time / (float)TimeMax, in roll, out sunAngle);

        public static void GetSunAngle(in float time, in float roll, out Vector3 sunAngle)
        {
            var fx = roll * MathEx.Deg2Rad;
            var fy = time * 360f * MathEx.Deg2Rad;

            var fxCos = MathF.Cos(fx);
            var fyCos = MathF.Cos(fy);
            var fySin = MathF.Sin(fy);

            sunAngle.X = fxCos * fyCos;
            sunAngle.Y = fySin * fxCos;
            sunAngle.Z = fySin;

            sunAngle.Normalize();
        }
    }
}
