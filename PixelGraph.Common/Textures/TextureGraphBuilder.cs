using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureGraphBuilder
    {
        bool UseGlobalOutput {get; set;}

        ITextureGraph BuildInputGraph(MaterialContext context);
        Task ProcessInputGraphAsync(MaterialContext context, CancellationToken token = default);
        Task ProcessOutputGraphAsync(MaterialContext context, CancellationToken token = default);
    }

    internal class TextureGraphBuilder : ITextureGraphBuilder
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly INamingStructure naming;

        public bool UseGlobalOutput {get; set;}


        public TextureGraphBuilder(IServiceProvider provider)
        {
            this.provider = provider;

            reader = provider.GetRequiredService<IInputReader>();
            writer = provider.GetRequiredService<IOutputWriter>();
            naming = provider.GetRequiredService<INamingStructure>();
        }

        public ITextureGraph BuildInputGraph(MaterialContext context)
        {
            var graph = new TextureGraph(provider, context) {
                UseGlobalOutput = UseGlobalOutput,
            };

            try {
                graph.BuildFromInput();
                return graph;
            }
            catch {
                graph.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Input -> Output; for publishing textures
        /// </summary>
        public async Task ProcessInputGraphAsync(MaterialContext context, CancellationToken token = default)
        {
            using var graph = BuildInputGraph(context);

            foreach (var tag in context.Profile.Output.GetAllTags()) {
                var encoding = context.Profile.Output.GetByTag(tag).ToArray();
                if (encoding.Any()) {
                    await ProcessTextureAsync(graph, encoding, tag, token);
                }
            }
        }

        /// <summary>
        /// Output -> Input; for importing textures
        /// </summary>
        public async Task ProcessOutputGraphAsync(MaterialContext context, CancellationToken token = default)
        {
            using var graph = new TextureGraph(provider, context) {
                UseGlobalOutput = UseGlobalOutput,
            };

            graph.BuildFromOutput();

            foreach (var tag in TextureTags.All) {
                var encoding = context.Input.GetByTag(tag).ToArray();
                if (encoding.Any()) await ProcessTextureAsync(graph, encoding, tag, token);
            }
        }

        private async Task ProcessTextureAsync(ITextureGraph graph, ResourcePackChannelProperties[] encoding, string tag, CancellationToken token)
        {
            await graph.BuildFinalImageAsync(tag, encoding, token);

            await CopyMetaAsync(graph.Context, tag, token);
        }

        private async Task CopyMetaAsync(MaterialContext context, string tag, CancellationToken token)
        {
            var metaFileIn = naming.GetInputMetaName(context.Material, tag);
            if (!reader.FileExists(metaFileIn)) return;

            var metaFileOut = naming.GetOutputMetaName(context.Profile, context.Material, tag, UseGlobalOutput);

            await using var sourceStream = reader.Open(metaFileIn);
            await using var destStream = writer.Open(metaFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }
    }
}
