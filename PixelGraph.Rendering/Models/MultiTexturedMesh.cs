using System;
using System.Collections.Generic;

namespace PixelGraph.Rendering.Models
{
    public class MultiTexturedMesh : Dictionary<string, TexturedMeshPart>
    {
        public IEnumerable<string> AllTextureIds => Keys;


        public MultiTexturedMesh() : base(StringComparer.InvariantCultureIgnoreCase) {}

        public void Set(string textureId, string textureFile, BlockMeshGeometry3D geometry)
        {
            this[textureId] = new TexturedMeshPart {
                TextureFile = textureFile,
                Geometry = geometry,
            };
        }

        public TexturedMeshPart Get(string textureId)
        {
            return this[textureId];
        }
    }

    public class TexturedMeshPart
    {
        public string TextureFile {get; set;}
        public BlockMeshGeometry3D Geometry {get; set;}
    }
}
