using PixelGraph.Common.Textures;

namespace PixelGraph.Common.IO.Texture;

internal class RawTextureWriter : TextureWriterBase
{
    public RawTextureWriter()
    {
        LocalMap[TextureTags.Opacity] = "opacity";
        LocalMap[TextureTags.Color] = "color";
        LocalMap[TextureTags.Height] = "height";
        LocalMap[TextureTags.Bump] = "bump";
        LocalMap[TextureTags.Normal] = "normal";
        LocalMap[TextureTags.Occlusion] = "occlusion";
        LocalMap[TextureTags.Specular] = "specular";
        LocalMap[TextureTags.Smooth] = "smooth";
        LocalMap[TextureTags.Rough] = "rough";
        LocalMap[TextureTags.Metal] = "metal";
        LocalMap[TextureTags.HCM] = "hcm";
        LocalMap[TextureTags.F0] = "f0";
        LocalMap[TextureTags.Porosity] = "porosity";
        LocalMap[TextureTags.SubSurfaceScattering] = "sss";
        LocalMap[TextureTags.Emissive] = "emissive";
        LocalMap[TextureTags.MER] = "mer";
        LocalMap[TextureTags.MERS] = "mers";
            
        GlobalMap[TextureTags.Color] = name => name;
        GlobalMap[TextureTags.Opacity] = name => $"{name}_a";
        GlobalMap[TextureTags.Height] = name => $"{name}_h";
        GlobalMap[TextureTags.Bump] = name => $"{name}_b";
        GlobalMap[TextureTags.Normal] = name => $"{name}_n";
        GlobalMap[TextureTags.Occlusion] = name => $"{name}_ao";
        GlobalMap[TextureTags.Specular] = name => $"{name}_s";
        GlobalMap[TextureTags.Smooth] = name => $"{name}_smooth";
        GlobalMap[TextureTags.Rough] = name => $"{name}_rough";
        GlobalMap[TextureTags.Metal] = name => $"{name}_metal";
        GlobalMap[TextureTags.HCM] = name => $"{name}_hcm";
        GlobalMap[TextureTags.F0] = name => $"{name}_f0";
        GlobalMap[TextureTags.Porosity] = name => $"{name}_p";
        GlobalMap[TextureTags.SubSurfaceScattering] = name => $"{name}_sss";
        GlobalMap[TextureTags.Emissive] = name => $"{name}_e";
        GlobalMap[TextureTags.MER] = name => $"{name}_mer";
        GlobalMap[TextureTags.MERS] = name => $"{name}_mers";

        // Internal
        LocalMap[TextureTags.Item] = "item";
        GlobalMap[TextureTags.Item] = name => $"{name}_item";
    }
}