using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Rendering.CubeMaps;
using System.IO;

namespace PixelGraph.Rendering.Models
{
    public interface IRenderContext
    {
        ResourcePackInputProperties PackInput {get;}
        ResourcePackProfileProperties PackProfile {get;}
        MaterialProperties DefaultMaterial {get;}
        MaterialProperties MissingMaterial {get;}

        ICubeMapSource EnvironmentCubeMap {get;}
        ICubeMapSource IrradianceCubeMap {get;}
        bool EnvironmentEnabled {get;}
        Stream BrdfLutMap {get;}
    }

    public class RenderContext : IRenderContext
    {
        public ResourcePackInputProperties PackInput {get; set;}
        public ResourcePackProfileProperties PackProfile {get; set;}
        public MaterialProperties DefaultMaterial {get; set;}
        public MaterialProperties MissingMaterial {get; set;}

        public ICubeMapSource EnvironmentCubeMap {get; set;}
        public ICubeMapSource IrradianceCubeMap {get; set;}
        public bool EnvironmentEnabled {get; set;}
        public Stream BrdfLutMap {get; set;}
    }
}
