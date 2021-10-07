using SharpDX;
using System;

namespace MinecraftMappings.Internal.Models
{
    public class ModelElementRotation
    {
        public decimal Angle;
        public ModelAxis Axis;
        public Vector3 Origin;


        public Vector3 GetAxisVector()
        {
            switch (Axis) {
                case ModelAxis.X:
                    return Vector3.UnitX;
                case ModelAxis.Y:
                    return Vector3.UnitY;
                case ModelAxis.Z:
                    return Vector3.UnitZ;
                default:
                    throw new ApplicationException($"Unknown model axis '{Axis}'!");
            }
        }
    }
}
