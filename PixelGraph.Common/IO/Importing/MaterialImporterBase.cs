using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;
using System;
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

        ResourcePackInputProperties PackInput {get; set;}

        ResourcePackProfileProperties PackProfile {get; set;}

        Task<MaterialProperties> CreateMaterialAsync(string localPath, string name);
        Task ImportAsync(MaterialProperties material, CancellationToken token = default);
    }

    internal abstract class MaterialImporterBase : IMaterialImporter
    {
        protected IServiceProvider Provider {get; set;}
        protected IMaterialWriter MaterialWriter {get; set;}
        protected IInputReader Reader {get; set;}

        /// <inheritdoc />
        public bool AsGlobal {get; set;}

        public ResourcePackInputProperties PackInput {get; set;}

        public ResourcePackProfileProperties PackProfile {get; set;}


        protected MaterialImporterBase(IServiceProvider provider)
        {
            Provider = provider;

            MaterialWriter = provider.GetRequiredService<IMaterialWriter>();
            Reader = provider.GetRequiredService<IInputReader>();
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

            await MaterialWriter.WriteAsync(material);
            return material;
        }

        public async Task ImportAsync(MaterialProperties material, CancellationToken token = default)
        {
            using var scope = Provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();

            context.Input = (ResourcePackInputProperties)PackInput.Clone();
            context.Profile = (ResourcePackProfileProperties)PackProfile.Clone();
            context.PublishAsGlobal = AsGlobal;
            context.Material = material;
            context.IsImport = true;

            context.Mapping = new DefaultPublishMapping();

            await OnImportMaterialAsync(scope.ServiceProvider, token);
        }

        protected virtual Task OnImportMaterialAsync(IServiceProvider scope, CancellationToken token = default)
        {
            var graphBuilder = scope.GetRequiredService<IImportGraphBuilder>();

            return graphBuilder.ImportAsync(token);
        }
    }
}
