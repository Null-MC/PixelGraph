using McPbrPipeline.Internal.Encoding;
using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using SixLabors.ImageSharp;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Textures
{
    internal class TextureGraphBuilder
    {
        private readonly PackProperties pack;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;

        public bool UseGlobalOutput {get; set;}


        public TextureGraphBuilder(PackProperties pack, IInputReader reader, IOutputWriter writer)
        {
            this.pack = pack;
            this.reader = reader;
            this.writer = writer;
        }

        public async Task BuildAsync(PbrProperties texture, CancellationToken token = default)
        {
            using var graph = new TextureGraph(reader, pack, texture);

            graph.Build();

            if (graph.ContainsSource(EncodingChannel.Height)) {
                // pre-generate normal map
                await graph.BuildNormalMapAsync(token);
            }

            if (graph.Encoding.OutputAlbedo) await ProcessTextureAsync(graph, TextureTags.Albedo, token);

            if (graph.Encoding.OutputHeight) await ProcessTextureAsync(graph, TextureTags.Height, token);

            if (graph.Encoding.OutputNormal) await ProcessTextureAsync(graph, TextureTags.Normal, token);

            if (graph.Encoding.OutputSpecular) await ProcessTextureAsync(graph, TextureTags.Specular, token);

            if (graph.Encoding.OutputEmissive) await ProcessTextureAsync(graph, TextureTags.Emissive, token);

            if (graph.Encoding.OutputOcclusion) await ProcessTextureAsync(graph, TextureTags.Occlusion, token);

            // smooth/smooth2/rough

            //if (pack.OutputMetal) await ProcessTextureAsync(texture, pack.MetalOutputEncoding, $"{texture.Name}_m.png", token);

            // porosity

            // sss
        }

        private async Task ProcessTextureAsync(TextureGraph graph, string tag, CancellationToken token)
        {
            var name = OutputNamingStructure.Get(graph.Texture.Name, tag, UseGlobalOutput);

            using var image = await graph.BuildFinalImageAsync(tag, token);
            if (image == null) return;

            var destFile = PathEx.Join(graph.Texture.Path, name);
            await using var stream = writer.WriteFile(destFile);
            await image.SaveAsPngAsync(stream, token);
        }
    }
}
