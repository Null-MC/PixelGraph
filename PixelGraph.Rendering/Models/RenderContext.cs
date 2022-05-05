using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.LUTs;

namespace PixelGraph.Rendering.Models
{
    public interface IRenderContext
    {
        RenderPreviewModes RenderMode {get;}
        IProjectDescription Project {get;}
        PublishProfileProperties PackProfile {get;}
        MaterialProperties DefaultMaterial {get;}
        MaterialProperties MissingMaterial {get;}

        ILutMapSource DielectricBrdfLutMap {get;}
        ICubeMapSource EnvironmentCubeMap {get;}
        ICubeMapSource IrradianceCubeMap {get;}
        bool EnvironmentEnabled {get;}
        bool EnableLinearSampling {get;}
        bool EnableTiling {get;}
    }

    public class RenderContext : IRenderContext
    {
        public RenderPreviewModes RenderMode {get; set;}
        public IProjectDescription Project {get; set;}
        public PublishProfileProperties PackProfile {get; set;}
        public MaterialProperties DefaultMaterial {get; set;}
        public MaterialProperties MissingMaterial {get; set;}

        public ILutMapSource DielectricBrdfLutMap {get; set;}
        public ICubeMapSource EnvironmentCubeMap {get; set;}
        public ICubeMapSource IrradianceCubeMap {get; set;}
        public bool EnvironmentEnabled {get; set;}
        public bool EnableLinearSampling {get; set;}
        public bool EnableTiling {get; set;}
    }
}
