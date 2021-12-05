using System;
using PixelGraph.Common.Extensions;
using SharpDX;

namespace PixelGraph.Rendering
{
    public static class MinecraftTime
    {
        //public const int TimeMax = 24_000;


        public static void GetSunAngleX(in float azimuth, in float roll, in float time, out Vector3 sunAngle)
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

            GetSunElevation(in time, out var elevationR);

            sunAngle.X = -MathF.Sin(elevationR);
            sunAngle.Y = MathF.Cos(elevationR) * sunRotationData.X;
            sunAngle.Z = MathF.Cos(elevationR) * sunRotationData.Y;
            sunAngle.Normalize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="azimuth">The compass direction from which the sunlight is coming.</param>
        /// <param name="tilt">The angle of the sun corresponding to altitude.</param>
        /// <param name="time">The normalized Minecraft time-of-day [0-1].</param>
        /// <param name="sunAngle">The resulting normalized vector pointing to the sun.</param>
        public static void GetSunAngle(in float azimuth, in float tilt, in float time, out Vector3 sunAngle)
        {
            //our previous vector was:
            //-sin(ang),
            //cos(ang) * cos(sunPathRotation),
            //-cos(ang) * sin(sunPathRotation)

            GetSunElevation(in time, out var elevationR);

            //if we apply a rotation matrix along the Y axis for azimuth, we get:
            //[ cos(azimuth), 0, -sin(azimuth) ] [ -sin(ang) ]
            //[ 0             1   0            ] [  cos(ang) * cos(sunPathRotation) ]
            //[ sin(azimuth), 0,  cos(azimuth) ] [ -cos(ang) * sin(sunPathRotation) ]

            ////solving this transformation, we get the sum of these 3 vectors:
            //-sin(ang) * cos(azimuth),       0,       -sin(ang) * sin(azimuth)
            //    +
            //0,                                              cos(ang) * cos(sunPathRotation), 0
            //    +
            //cos(ang) * sin(sunPathRotation) * sin(azimuth), 0,        -cos(ang) * sin(sunPathRotation) * cos(azimuth)

            //adding these 3 vectors up, we get:
            sunAngle.X = -MathF.Sin(elevationR) * MathEx.CosD(azimuth) + MathF.Cos(elevationR) * MathEx.SinD(tilt) * MathEx.SinD(azimuth);
            sunAngle.Y = MathF.Cos(elevationR) * MathEx.CosD(tilt);
            sunAngle.Z = -MathF.Sin(elevationR) * MathEx.SinD(azimuth) - MathF.Cos(elevationR) * MathEx.SinD(tilt) * MathEx.CosD(azimuth);
        }

        public static float GetSunStrength(in float time, in float overlap, in float power)
        {
            GetSunElevation(in time, out var elevationR);

            var s = MathF.Cos(elevationR);
            var f = MathF.Max(s * (1f - overlap) + overlap, 0f);
            return MathF.Pow(f, power);
        }

        /// <summary>
        /// Gets the elevation angle of the sun from the normalized time-of-day.
        /// </summary>
        /// <param name="time">The normalized Minecraft time-of-day [0-1].</param>
        /// <param name="elevation">The resulting elevation angle of the sun in radians.</param>
        private static void GetSunElevation(in float time, out float elevation)
        {
            var ang = time - 0.25f;
            MathEx.Wrap(ref ang, 0f, 1f);
            elevation = (ang + (MathF.Cos(ang * MathF.PI) * -0.5f + 0.5f - ang) / 3f) * 2f*MathF.PI; //0-2pi, rolls over from 2pi to 0 at noon.
        }
    }
}
