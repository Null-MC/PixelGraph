using HelixToolkit.SharpDX.Core;
using SharpDX;

namespace PixelGraph.Rendering.Extensions
{
    internal static class MeshBuilderExtensions
    {
        public static void AddEntityCubeFace(this MeshBuilder builder, Vector3 center, Vector3 normal, Vector3 up, float dist, float width, float height, Vector2 uvMin, Vector2 uvMax)
        {
            Vector3.Cross(ref normal, ref up, out var right);

            var n = normal * dist / 2f;
            up *= height / 2f;
            right *= width / 2f;

            var p1 = center + n - up - right;
            var p2 = center + n - up + right;
            var p3 = center + n + up + right;
            var p4 = center + n + up - right;

            var i0 = builder.Positions.Count;
            builder.Positions.Add(p1);
            builder.Positions.Add(p2);
            builder.Positions.Add(p3);
            builder.Positions.Add(p4);

            if (builder.Normals != null) {
                builder.Normals.Add(normal);
                builder.Normals.Add(normal);
                builder.Normals.Add(normal);
                builder.Normals.Add(normal);
            }

            if (builder.TextureCoordinates != null) {
                builder.TextureCoordinates.Add(uvMax);
                builder.TextureCoordinates.Add(new Vector2(uvMin.X, uvMax.Y));
                builder.TextureCoordinates.Add(uvMin);
                builder.TextureCoordinates.Add(new Vector2(uvMax.X, uvMin.Y));
            }

            builder.TriangleIndices.Add(i0 + 2);
            builder.TriangleIndices.Add(i0 + 1);
            builder.TriangleIndices.Add(i0 + 0);
            builder.TriangleIndices.Add(i0 + 0);
            builder.TriangleIndices.Add(i0 + 3);
            builder.TriangleIndices.Add(i0 + 2);
        }
    }
}
