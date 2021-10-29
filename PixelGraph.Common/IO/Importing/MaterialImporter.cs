using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using System;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.Common.Textures.Graphing.Builders;

namespace PixelGraph.Common.IO.Importing
{
    internal interface IMaterialImporter
    {
        /// <summary>
        /// Gets or sets whether imported materials should be global or local.
        /// </summary>
        bool AsGlobal {get; set;}

        ResourcePackInputProperties PackInput {get; set;}

        ResourcePackProfileProperties PackProfile {get; set;}

        Task<MaterialProperties> CreateMaterialAsync(string localPath, string name);
        Task ImportAsync(MaterialProperties material, CancellationToken token = default);
    }

    internal class MaterialImporter : IMaterialImporter
    {
        private readonly IServiceProvider provider;
        private readonly IMaterialWriter writer;

        /// <inheritdoc />
        public bool AsGlobal {get; set;}

        public ResourcePackInputProperties PackInput {get; set;}

        public ResourcePackProfileProperties PackProfile {get; set;}


        public MaterialImporter(
            IServiceProvider provider,
            IMaterialWriter writer)
        {
            this.provider = provider;
            this.writer = writer;
        }

        public async Task<MaterialProperties> CreateMaterialAsync(string localPath, string name)
        {
            var matFile = AsGlobal
                ? PathEx.Join(localPath, $"{name}.mat.yml")
                : PathEx.Join(localPath, name, "mat.yml");

            var material = new MaterialProperties {
                Name = name,
                LocalPath = localPath,
                LocalFilename = matFile,
                UseGlobalMatching = AsGlobal,
            };

            await writer.WriteAsync(material);
            return material;
        }

        public async Task ImportAsync(MaterialProperties material, CancellationToken token = default)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var graphBuilder = scope.ServiceProvider.GetRequiredService<IImportGraphBuilder>();

            context.Input = PackInput;
            context.Profile = PackProfile;
            context.Material = material;
            context.PublishAsGlobal = AsGlobal;
            context.IsImport = true;

            context.Mapping = new DefaultPublishMapping();

            await graphBuilder.ImportAsync(token);
        }
    }
}
