using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Textures
{
    internal class TextureGraphBuilder
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly PackProperties pack;
        private readonly ILogger logger;

        public bool UseGlobalOutput {get; set;}


        public TextureGraphBuilder(IServiceProvider provider, IInputReader reader, IOutputWriter writer, PackProperties pack)
        {
            this.provider = provider;
            this.reader = reader;
            this.writer = writer;
            this.pack = pack;

            logger = provider.GetRequiredService<ILogger<TextureGraphBuilder>>();
        }

        public async Task BuildAsync(PbrProperties texture, CancellationToken token = default)
        {
            using var graph = new TextureGraph(provider, reader, pack, texture);

            if (graph.Encoding.OutputAlbedo) await ProcessTextureAsync(graph, TextureTags.Albedo, token);

            if (graph.Encoding.OutputHeight) await ProcessTextureAsync(graph, TextureTags.Height, token);

            if (graph.Encoding.OutputNormal) await ProcessTextureAsync(graph, TextureTags.Normal, token);

            if (graph.Encoding.OutputOcclusion) await ProcessTextureAsync(graph, TextureTags.Occlusion, token);

            if (graph.Encoding.OutputSpecular) await ProcessTextureAsync(graph, TextureTags.Specular, token);

            if (graph.Encoding.OutputSmooth) await ProcessTextureAsync(graph, TextureTags.Smooth, token);

            if (graph.Encoding.OutputRough) await ProcessTextureAsync(graph, TextureTags.Rough, token);

            if (graph.Encoding.OutputMetal) await ProcessTextureAsync(graph, TextureTags.Metal, token);

            if (graph.Encoding.OutputPorosity) await ProcessTextureAsync(graph, TextureTags.Porosity, token);

            if (graph.Encoding.OutputSubSurfaceScattering) await ProcessTextureAsync(graph, TextureTags.SubSurfaceScattering, token);

            if (graph.Encoding.OutputEmissive) await ProcessTextureAsync(graph, TextureTags.Emissive, token);
        }

        private async Task ProcessTextureAsync(TextureGraph graph, string tag, CancellationToken token)
        {
            var name = NamingStructure.GetOutputTextureName(tag, graph.Texture.Name, UseGlobalOutput);

            using var image = await graph.BuildFinalImageAsync(tag, token);

            if (image != null) {
                Resize(image, graph.Texture);

                var destFile = PathEx.Join(graph.Texture.Path, name);
                await using var stream = writer.WriteFile(destFile);
                await image.SaveAsPngAsync(stream, token);

                logger.LogInformation("Published texture {Name} tag {tag}.", graph.Texture.Name, tag);
            }

            await CopyMetaAsync(graph, tag, token);
        }

        private void Resize(Image image, PbrProperties texture)
        {
            if (!(texture?.ResizeEnabled ?? true)) return;
            if (!pack.TextureSize.HasValue && !pack.TextureScale.HasValue) return;

            var (width, height) = image.Size();

            var resampler = KnownResamplers.Bicubic;
            if (pack.Sampler != null && Samplers.TryParse(pack.Sampler, out var _resampler))
                resampler = _resampler;

            if (pack.TextureSize.HasValue) {
                if (width == pack.TextureSize) return;

                image.Mutate(c => c.Resize(pack.TextureSize.Value, 0, resampler));
            }
            else {
                var targetWidth = (int)Math.Max(width * pack.TextureScale.Value, 1f);
                var targetHeight = (int)Math.Max(height * pack.TextureScale.Value, 1f);

                image.Mutate(c => c.Resize(targetWidth, targetHeight, resampler));
            }
        }

        private async Task CopyMetaAsync(TextureGraph graph, string tag, CancellationToken token)
        {
            var metaFileIn = NamingStructure.GetInputMetaName(tag, graph.Texture);
            if (!reader.FileExists(metaFileIn)) return;

            var metaFileOut = NamingStructure.GetOutputMetaName(tag, graph.Texture, UseGlobalOutput);

            await using var sourceStream = reader.Open(metaFileIn);
            await using var destStream = writer.WriteFile(metaFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }
    }
}
