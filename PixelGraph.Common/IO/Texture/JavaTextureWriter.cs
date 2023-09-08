using PixelGraph.Common.Textures;

namespace PixelGraph.Common.IO.Texture;

internal class JavaTextureWriter : TextureWriterBase
{
    public JavaTextureWriter()
    {
        LocalMap[TextureTags.Color] = "basecolor";
        LocalMap[TextureTags.Normal] = "normal";
        LocalMap[TextureTags.Specular] = "specular";
            
        GlobalMap[TextureTags.Color] = name => name;
        GlobalMap[TextureTags.Normal] = name => $"{name}_n";
        GlobalMap[TextureTags.Specular] = name => $"{name}_s";

        // Internal
        LocalMap[TextureTags.Item] = "item";
        GlobalMap[TextureTags.Item] = name => $"{name}_item";
    }
}