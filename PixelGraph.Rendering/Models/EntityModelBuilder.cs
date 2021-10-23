using HelixToolkit.SharpDX.Core;
using MinecraftMappings.Internal.Models;
using MinecraftMappings.Internal.Models.Entity;
using PixelGraph.Common.Extensions;
using SharpDX;
using System;

namespace PixelGraph.Rendering.Models
{
    public interface IEntityModelBuilder : IModelBuilder
    {
        void BuildEntity(in float cubeSize, EntityModelVersion entity);
    }

    public class EntityModelBuilder : ModelBuilder, IEntityModelBuilder
    {
        //private static readonly Vector3 entityCenter = new(0f, 8f, 0f);


        public void BuildEntity(in float cubeSize, EntityModelVersion entityVersion)
        {
            Builder.Clear();
            
            //foreach (var rotation in entityVersion.Rotations) {
            //    AppendEntityRotation(rotation, in Matrix.Identity, in cubeSize, in entityVersion.TextureSize);
            //}
            
            foreach (var element in entityVersion.Elements) {
                AppendEntityElement(element, in Matrix.Identity, in cubeSize, in entityVersion.TextureSize);
            }

            Builder.ComputeTangents(MeshFaces.Default);
        }

        private void AppendEntityElement(EntityElement element, in Matrix mWorldParent, in float cubeSize, in Vector2 textureSize)
        {
            //var halfBlock = element.Size * 0.5f;
            //var mWorld = Matrix.Translation(element.Position + halfBlock);
            var m2 = Matrix.Identity; //Matrix.Translation(element.Translate);

            if (element.HasRotation) {
                //mWorld *= Matrix.Translation(-element.Translate);
                //m2 *= Matrix.Translation(-element.Translate);

                if (element.RotationAngleX != 0) {
                    var angleR = element.RotationAngleX * MathEx.Deg2RadF;
                    //mWorld *= Matrix.RotationAxis(Vector3.UnitX, angleR);
                    m2 *= Matrix.RotationAxis(Vector3.UnitX, angleR);
                }

                if (element.RotationAngleY != 0) {
                    var angleR = element.RotationAngleY * MathEx.Deg2RadF;
                    //mWorld *= Matrix.RotationAxis(Vector3.UnitY, angleR);
                    m2 *= Matrix.RotationAxis(Vector3.UnitY, angleR);
                }

                if (element.RotationAngleZ != 0) {
                    var angleR = element.RotationAngleZ * MathEx.Deg2RadF;
                    //mWorld *= Matrix.RotationAxis(Vector3.UnitZ, angleR);
                    m2 *= Matrix.RotationAxis(Vector3.UnitZ, angleR);
                }

                //mWorld *= Matrix.Translation(element.Translate);
                m2 *= Matrix.Translation(element.Translate);
            }

            //m2 *= Matrix.Translation(-element.Position);

            //mWorld = mWorld * mWorldParent;
            m2 = m2 * mWorldParent;

            if (element.Submodels != null) {
                foreach (var childElement in element.Submodels) {
                    AppendEntityElement(childElement, in m2, in cubeSize, in textureSize);
                }
            }

            if (element.Cubes != null) {
                foreach (var cube in element.Cubes) {
                    AppendEntityCube(element, cube, in m2, in cubeSize, in textureSize);
                }
            }
        }

        private void AppendEntityCube(EntityElement element, EntityElementCube cube, in Matrix mWorldParent, in float cubeSize, in Vector2 textureSize)
        {
            if (cube.Size.IsZero) return;

            var halfBlock = cube.Size * 0.5f;
            var mWorld = Matrix.Translation(cube.Position + halfBlock);
            //var m2 = Matrix.Translation(cube.Position);

            mWorld *= mWorldParent;

            //mWorld *= Matrix.Translation(-entityCenter);
            mWorld *= Matrix.Scaling(BlockToWorld * cubeSize);

            var uvScaled = new RectangleF();
            float offset, width, height;
            Vector3 normal, up;

            foreach (var face in EntityElementCube.Faces) {
                var region = cube.GetFaceRectangle(face, element.MirrorUVX);

                up = GetUpVector(face);
                normal = GetFaceNormal(face);
                (width, height, offset) = cube.GetWidthHeightOffset(in face);

                if (width == 0 || height == 0) continue;

                uvScaled.Top = region.Top / textureSize.Y;
                uvScaled.Bottom = region.Bottom / textureSize.Y;
                uvScaled.Left = region.Left / textureSize.X;
                uvScaled.Right = region.Right / textureSize.X;

                //if (element.MirrorTextureU) uvScaled.X = 1.0f - uvScaled.X;
                //if (element.MirrorTextureV) uvScaled.Y = 1.0f - uvScaled.Y;

                AddCubeFace(in mWorld, in normal, in up, in offset, in width, in height, in uvScaled, 0);
            }
        }

        //private void AppendEntityRotation(EntityRotation rotation, in Matrix mWorldParent, in float cubeSize, in Vector2 textureSize)
        //{
        //    //var halfBlock = element.Size * 0.5f;
        //    //var mWorld = Matrix.Translation(element.Position + halfBlock);
        //    var m2 = Matrix.Translation(rotation.Translation);

        //    if (rotation.HasRotation) {
        //        //mWorld *= Matrix.Translation(-element.Translate);
        //        //m2 *= Matrix.Translation(-rotation.Translate);

        //        if (rotation.RotationAngleX != 0) {
        //            var angleR = rotation.RotationAngleX * MathEx.Deg2RadF;
        //            //mWorld *= Matrix.RotationAxis(Vector3.UnitX, angleR);
        //            m2 *= Matrix.RotationAxis(Vector3.UnitX, angleR);
        //        }

        //        if (rotation.RotationAngleY != 0) {
        //            var angleR = rotation.RotationAngleY * MathEx.Deg2RadF;
        //            //mWorld *= Matrix.RotationAxis(Vector3.UnitY, angleR);
        //            m2 *= Matrix.RotationAxis(Vector3.UnitY, angleR);
        //        }

        //        if (rotation.RotationAngleZ != 0) {
        //            var angleR = rotation.RotationAngleZ * MathEx.Deg2RadF;
        //            //mWorld *= Matrix.RotationAxis(Vector3.UnitZ, angleR);
        //            m2 *= Matrix.RotationAxis(Vector3.UnitZ, angleR);
        //        }

        //        //mWorld *= Matrix.Translation(element.Translate);
        //        //m2 *= Matrix.Translation(rotation.Translate);
        //    }

        //    //m2 *= Matrix.Translation(-element.Position);

        //    if (rotation.Rotations != null) {
        //        foreach (var childRotation in rotation.Rotations) {
        //            var m3 = m2 * mWorldParent;
        //            AppendEntityRotation(childRotation, in m3, in cubeSize, in textureSize);
        //        }
        //    }

        //    if (rotation.Elements != null) {
        //        foreach (var childElement in rotation.Elements) {
        //            var m3 = m2 * mWorldParent;
        //            AppendEntityElement(childElement, in m3, in cubeSize, in textureSize);
        //        }
        //    }

        //    //if (element.Size.IsZero) return;

        //    //mWorld = mWorld * mWorldParent;
        //}

        private static Vector3 GetFaceNormal(ElementFaces face)
        {
            return face switch {
                ElementFaces.Up => new Vector3(0, 1, 0),
                ElementFaces.Down => new Vector3(0, -1, 0),
                ElementFaces.North => new Vector3(0, 0, -1),
                ElementFaces.South => new Vector3(0, 0, 1),
                ElementFaces.East => new Vector3(1, 0, 0),
                ElementFaces.West => new Vector3(-1, 0, 0),
                _ => throw new ApplicationException($"Unknown element face '{face}'!"),
            };
        }

        private static Vector3 GetUpVector(ElementFaces face)
        {
            return face switch {
                ElementFaces.Up => -Vector3.UnitZ,
                ElementFaces.Down => Vector3.UnitZ,
                ElementFaces.North => Vector3.UnitY,
                ElementFaces.South => Vector3.UnitY,
                ElementFaces.East => Vector3.UnitY,
                ElementFaces.West => Vector3.UnitY,
                _ => throw new ApplicationException($"Unknown element face '{face}'!"),
            };
        }
    }
}
