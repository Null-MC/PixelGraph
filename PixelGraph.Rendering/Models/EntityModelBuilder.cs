using HelixToolkit.SharpDX.Core;
using MinecraftMappings.Internal.Models.Entity;
using PixelGraph.Common.Extensions;
using SharpDX;

namespace PixelGraph.Rendering.Models
{
    public class EntityModelBuilder : ModelBuilder
    {
        public void Clear() => Builder.Clear();

        public void BuildEntity(in float cubeSize, EntityModelVersion entityVersion)
        {
            var m = Matrix.RotationY(180f * MathEx.Deg2RadF);

            foreach (var element in entityVersion.Elements)
                AppendEntityElement(element, in m, in cubeSize, in entityVersion.TextureSize, true);

            Builder.ComputeTangents(MeshFaces.Default);
        }

        private void AppendEntityElement(EntityElement element, in Matrix matParent, in float cubeSize, in Vector2 textureSize, bool isRoot)
        {
            var matLocal = Matrix.Identity;
            if (isRoot) matLocal *= Matrix.Translation(element.Translate);

            if (element.HasRotation) {
                if (element.RotationAngleX != 0) {
                    var angleR = element.RotationAngleX * MathEx.Deg2RadF;
                    matLocal *= Matrix.RotationAxis(Vector3.UnitX, angleR);
                }

                if (element.RotationAngleY != 0) {
                    var angleR = element.RotationAngleY * MathEx.Deg2RadF;
                    matLocal *= Matrix.RotationAxis(Vector3.UnitY, angleR);
                }

                if (element.RotationAngleZ != 0) {
                    var angleR = element.RotationAngleZ * MathEx.Deg2RadF;
                    matLocal *= Matrix.RotationAxis(Vector3.UnitZ, angleR);
                }
            }

            if (!isRoot) matLocal *= Matrix.Translation(element.Translate);
            else matLocal *= Matrix.Translation(-element.Translate);

            if (!element.InvertAxisX || !element.InvertAxisY || !element.InvertAxisZ) {
                var scaleX = element.InvertAxisX ? 1f : -1f;
                var scaleY = element.InvertAxisY ? 1f : -1f;
                var scaleZ = element.InvertAxisZ ? -1f : 1f;
                matLocal *= Matrix.Scaling(scaleX, scaleY, scaleZ);
            }

            var matFinal = matLocal * matParent;

            if (element.Model != null) {
                foreach (var childModel in element.Model.Submodels) {
                    AppendEntityElement(childModel, in matFinal, in cubeSize, in element.Model.TextureSize, false);
                }
            }

            if (element.Submodels != null) {
                foreach (var childElement in element.Submodels) {
                    AppendEntityElement(childElement, in matFinal, in cubeSize, in textureSize, false);
                }
            }

            if (element.Boxes != null) {
                foreach (var cube in element.Boxes) {
                    AppendEntityCube(element, cube, in matFinal, in cubeSize, in textureSize);
                }
            }
        }

        private void AppendEntityCube(EntityElement element, EntityElementCube cube, in Matrix mWorldParent, in float cubeSize, in Vector2 textureSize)
        {
            if (cube.Size.IsZero) return;

            var halfBlock = cube.Size * 0.5f;
            var mWorld = Matrix.Translation(cube.Position + halfBlock);

            mWorld *= mWorldParent;

            mWorld *= Matrix.Scaling(BlockToWorld * cubeSize);

            var uvScaled = new RectangleF();
            float offset, width, height;
            Vector3 normal, up;

            foreach (var face in EntityElementCube.Faces) {
                var region = cube.GetFaceRectangle(face, element.MirrorTexU);

                up = GetUpVector(face);
                normal = GetFaceNormal(face);
                (width, height, offset) = cube.GetWidthHeightOffset(in face);

                if (width == 0 || height == 0) continue;

                uvScaled.Top = region.Top / textureSize.Y;
                uvScaled.Bottom = region.Bottom / textureSize.Y;
                uvScaled.Left = region.Left / textureSize.X;
                uvScaled.Right = region.Right / textureSize.X;

                AddCubeFace(in mWorld, in normal, in up, in offset, in width, in height, in uvScaled, 0);
            }
        }
    }
}
