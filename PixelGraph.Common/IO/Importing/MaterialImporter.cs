using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Importing
{
    internal interface IMaterialImporter
    {
        /// <summary>
        /// Gets or sets whether imported materials should be global or local.
        /// </summary>
        bool AsGlobal {get; set;}

        string LocalPath {get; set;}

        ResourcePackInputProperties PackInput {get; set;}

        ResourcePackProfileProperties PackProfile {get; set;}

        Task ImportAsync(string name, CancellationToken token = default);
    }

    internal class MaterialImporter : IMaterialImporter
    {
        private readonly ITextureGraphBuilder graphBuilder;
        private readonly IMaterialWriter writer;

        /// <inheritdoc />
        public bool AsGlobal {get; set;}

        public string LocalPath {get; set;}

        public ResourcePackInputProperties PackInput {get; set;}

        public ResourcePackProfileProperties PackProfile {get; set;}


        public MaterialImporter(
            ITextureGraphBuilder graphBuilder,
            IMaterialWriter writer)
        {
            this.graphBuilder = graphBuilder;
            this.writer = writer;
        }

        public async Task ImportAsync(string name, CancellationToken token = default)
        {
            var matFile = AsGlobal
                ? PathEx.Join(LocalPath, $"{name}.pbr.yml")
                : PathEx.Join(LocalPath, name, "pbr.yml");

            var material = new MaterialProperties {
                Name = name,
                LocalPath = LocalPath,
                LocalFilename = matFile,
                UseGlobalMatching = AsGlobal,
            };

            await writer.WriteAsync(material, matFile);

            var context = new MaterialContext {
                Input = PackInput,
                Profile = PackProfile,
                Material = material,
            };

            await graphBuilder.ProcessOutputGraphAsync(context, token);
        }
    }
}
