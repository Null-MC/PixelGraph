using HelixToolkit.SharpDX.Core;
using MinecraftMappings.Internal.Models;
using MinecraftMappings.Internal.Models.Block;
using PixelGraph.Common.Extensions;
using SharpDX;
using System;

namespace PixelGraph.Rendering.Models
{
    public class BlockModelBuilder : ModelBuilder
    {
        private static readonly Vector3 blockCenter = new(8f, 8f, 8f);


        public void Clear() => Builder.Clear();

        public void AppendModelTextureParts(in float cubeSize, Vector3 offset, BlockModelVersion modelVersion, string textureId = null)
        {
            Vector3 faceNormal, faceUp;
            float faceOffset, faceWidth, faceHeight;
            Matrix mWorld;

            foreach (var element in modelVersion.Elements) {
                var halfBlock = (element.To - element.From) * 0.5f;
                mWorld = Matrix.Translation(element.From + halfBlock);

                if (element.Rotation != null) {
                    var axis = element.Rotation.GetAxisVector();
                    var angleR = (float)element.Rotation.Angle * MathEx.Deg2RadF;

                    mWorld *= Matrix.Translation(-element.Rotation.Origin);
                    mWorld *= Matrix.RotationAxis(axis, angleR);
                    mWorld *= Matrix.Translation(element.Rotation.Origin);
                }

                mWorld *= Matrix.Translation(-blockCenter + offset);
                mWorld *= Matrix.Scaling(BlockToWorld * cubeSize);

                foreach (var face in ModelElement.AllFaces) {
                    // Skip if no face data
                    var faceData = element.GetFace(face);
                    if (faceData == null) continue;
                    
                    // Skip if building a specific texture that doesn't match this ID
                    if (textureId != null && !string.Equals($"#{textureId}", faceData.Texture, StringComparison.InvariantCultureIgnoreCase)) continue;

                    faceUp = GetUpVector(face);
                    faceNormal = GetFaceNormal(face);
                    (faceWidth, faceHeight, faceOffset) = element.GetWidthHeightOffset(in face);
                    var rotation = faceData.Rotation ?? 0;

                    RectangleF uv;
                    if (faceData.UV.HasValue) uv = faceData.UV.Value;
                    else UVHelper.GetDefaultUv(element, in face, out uv);

                    Multiply(in uv, BlockToWorld, out uv);

                    AddCubeFace(in mWorld, in faceNormal, in faceUp, in faceOffset, in faceWidth, in faceHeight, in uv, in rotation);
                }
            }

            Builder.ComputeTangents(MeshFaces.Default);
        }

        private static void Multiply(in RectangleF region, in float scale, out RectangleF scaledRegion)
        {
            scaledRegion.Left = region.Left * scale;
            scaledRegion.Top = region.Top * scale;
            scaledRegion.Right = region.Right * scale;
            scaledRegion.Bottom = region.Bottom * scale;
        }
    }
}
