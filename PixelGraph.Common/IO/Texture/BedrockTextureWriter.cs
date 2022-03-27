using PixelGraph.Common.Textures;

namespace PixelGraph.Common.IO.Texture
{
    internal class BedrockTextureWriter : TextureWriterBase
    {
        public BedrockTextureWriter()
        {
            LocalMap[TextureTags.Color] = "basecolor";
            LocalMap[TextureTags.Height] = "heightmap";
            LocalMap[TextureTags.Normal] = "normal";
            LocalMap[TextureTags.MER] = "mer";

            GlobalMap[TextureTags.Color] = name => name;
            GlobalMap[TextureTags.Height] = name => $"{name}_heightmap";
            GlobalMap[TextureTags.Normal] = name => $"{name}_normal";
            GlobalMap[TextureTags.MER] = name => $"{name}_mer";

            // Internal
            LocalMap[TextureTags.Item] = "item";
            GlobalMap[TextureTags.Item] = name => $"{name}_item";
        }
    }
}
