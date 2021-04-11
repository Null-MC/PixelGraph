using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
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

        string LocalPath {get; set;}

        ResourcePackInputProperties PackInput {get; set;}

        ResourcePackProfileProperties PackProfile {get; set;}

        Task ImportAsync(string name, CancellationToken token = default);
    }

    internal class MaterialImporter : IMaterialImporter
    {
        private readonly IServiceProvider provider;
        private readonly IMaterialWriter writer;

        /// <inheritdoc />
        public bool AsGlobal {get; set;}

        public string LocalPath {get; set;}

        public ResourcePackInputProperties PackInput {get; set;}

        public ResourcePackProfileProperties PackProfile {get; set;}


        public MaterialImporter(
            IServiceProvider provider,
            IMaterialWriter writer)
        {
            this.provider = provider;
            this.writer = writer;
        }

        public async Task ImportAsync(string name, CancellationToken token = default)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var graphBuilder = scope.ServiceProvider.GetRequiredService<ITextureGraphBuilder>();

            var matFile = AsGlobal
                ? PathEx.Join(LocalPath, $"{name}.mat.yml")
                : PathEx.Join(LocalPath, name, "mat.yml");

            var material = new MaterialProperties {
                Name = name,
                LocalPath = LocalPath,
                LocalFilename = matFile,
                UseGlobalMatching = AsGlobal,
            };

            await writer.WriteAsync(material);

            context.Input = PackInput;
            context.Profile = PackProfile;
            context.Material = material;
            context.PublishAsGlobal = AsGlobal;
            context.IsImport = true;

            await graphBuilder.ProcessOutputGraphAsync(token);
        }
    }
}
