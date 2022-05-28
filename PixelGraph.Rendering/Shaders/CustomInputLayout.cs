using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace PixelGraph.Rendering.Shaders
{
    internal class CustomInputLayout
    {
        public static readonly InputElement[] VSBlockInput = {
            new("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new("NORMAL",   0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),
            new("TANGENT",  0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),
            new("BINORMAL", 0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),
            new("TEXCOORD", 0, Format.R32G32_Float,       InputElement.AppendAligned, 0),
            new("TEXCOORD", 1, Format.R32G32_Float,       InputElement.AppendAligned, 0),
            new("TEXCOORD", 2, Format.R32G32_Float,       InputElement.AppendAligned, 0),
            new("COLOR",    0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BlockMeshVertex
    {
        public Vector4 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;
        public Vector2 TexCoord;
        public Vector2 TexCoordMin;
        public Vector2 TexCoordMax;
        public Vector4 Color;
        //public Vector4 Color2;
        public const int SizeInBytes = 4 * (4 + 3 + 3 + 3 + 2 + 2 + 2 + 4); // + 4
    }
}
