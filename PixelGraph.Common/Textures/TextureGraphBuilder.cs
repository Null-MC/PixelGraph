using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Encoding;
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

        /// <summary>
        /// Input -> Output; for publishing textures
        /// </summary>
        public async Task ProcessInputGraphAsync(MaterialContext context, CancellationToken token = default)
        {
            var inputFormat = TextureEncoding.GetFactory(context.Input.Format);
            var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
            inputEncoding.Merge(context.Input);
            // TODO: layer material properties on top of pack encoding?

            var outputFormat = TextureEncoding.GetFactory(context.Profile.Output.Format);
            var outputEncoding = outputFormat?.Create() ?? new ResourcePackEncoding();
            outputEncoding.Merge(context.Profile.Output);

            using var graph = provider.GetRequiredService<ITextureGraph>();
            graph.InputEncoding = inputEncoding.GetMapped().ToList();
            graph.OutputEncoding = outputEncoding.GetMapped().ToList();
            graph.UseGlobalOutput = UseGlobalOutput;
            graph.Context = context;

            await ProcessAllTexturesAsync(graph, token);
        }

        /// <summary>
        /// Output -> Input; for importing textures
        /// </summary>
        public async Task ProcessOutputGraphAsync(MaterialContext context, CancellationToken token = default)
        {
            var inputFormat = TextureEncoding.GetFactory(context.Profile.Output.Format);
            var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
            inputEncoding.Merge(context.Profile.Output);

            var outputFormat = TextureEncoding.GetFactory(context.Input.Format);
            var outputEncoding = outputFormat?.Create() ?? new ResourcePackEncoding();
            outputEncoding.Merge(context.Input);
            // TODO: layer material properties on top of pack encoding?

            using var graph = provider.GetRequiredService<ITextureGraph>();
            graph.InputEncoding = context.Profile.Output.GetMapped().ToList();
            graph.OutputEncoding = context.Input.GetMapped().ToList();
            graph.UseGlobalOutput = UseGlobalOutput;
            graph.Context = context;

            await ProcessAllTexturesAsync(graph, token);
        }

        private async Task ProcessAllTexturesAsync(ITextureGraph graph, CancellationToken token)
        {
            var hasOutputNormals = graph.OutputEncoding
                .Where(e => e.HasMapping)
                .Any(e => {
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalX)) return true;
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalY)) return true;
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalZ)) return true;
                    return false;
                });

            if (hasOutputNormals) {
                await graph.BuildNormalTextureAsync(token);
            }

            var allOutputTags = graph.OutputEncoding
                .Select(e => e.Texture).Distinct();

            foreach (var tag in allOutputTags) {
                var tagOutputEncoding = graph.OutputEncoding
                    .Where(e => TextureTags.Is(e.Texture, tag)).ToArray();

                if (tagOutputEncoding.Any())
                    await graph.BuildFinalImageAsync(tag, token);

                await CopyMetaAsync(graph.Context, tag, token);
            }
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
