using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures.Graphing
{
    public interface ITextureGraph
    {
        Task PreBuildNormalTextureAsync(CancellationToken token = default);
        int GetMaxFrameCount();

        Task MapAsync(string textureTag, bool createEmpty, int? frame = null, int? part = null, CancellationToken token = default);
        Task<Image<TPixel>> CreateImageAsync<TPixel>(string textureTag, bool createEmpty, CancellationToken token = default) where TPixel : unmanaged, IPixel<TPixel>;
    }

    internal class TextureGraph : ITextureGraph
    {
        private readonly IServiceProvider provider;
        private readonly ITextureGraphContext context;
        private readonly ITextureNormalGraph normalGraph;
        private readonly Dictionary<string, ITextureBuilder> builderMap;
        private bool hasPreBuiltNormals;


        public TextureGraph(IServiceProvider provider)
        {
            this.provider = provider;

            context = provider.GetRequiredService<ITextureGraphContext>();
            normalGraph = provider.GetRequiredService<ITextureNormalGraph>();

            builderMap = new Dictionary<string, ITextureBuilder>(StringComparer.OrdinalIgnoreCase);
            hasPreBuiltNormals = false;
        }

        private bool HasOutputNormals()
        {
            return context.OutputEncoding.Where(e => e.HasMapping)
                .Any(e => {
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalX)) return true;
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalY)) return true;
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalZ)) return true;
                    return false;
                });
        }

        public async Task PreBuildNormalTextureAsync(CancellationToken token = default)
        {
            if (hasPreBuiltNormals) return;
            hasPreBuiltNormals = true;

            if (!HasOutputNormals()) return;

            if (await normalGraph.TryBuildNormalMapAsync(token)) {
                UpsertInputChannel<ResourcePackNormalXChannelProperties>(channel => {
                    channel.Texture = TextureTags.NormalGenerated;
                    channel.Color = ColorChannel.Red;
                });

                UpsertInputChannel<ResourcePackNormalYChannelProperties>(channel => {
                    channel.Texture = TextureTags.NormalGenerated;
                    channel.Color = ColorChannel.Green;
                });

                UpsertInputChannel<ResourcePackNormalZChannelProperties>(channel => {
                    channel.Texture = TextureTags.NormalGenerated;
                    channel.Color = ColorChannel.Blue;
                });

                context.InputEncoding
                    .Where(c => TextureTags.Is(c.Texture, TextureTags.Normal))
                    .Where(c => c.Color == ColorChannel.Magnitude)
                    .Update(c => {
                        c.Reset();
                        c.Texture = TextureTags.MagnitudeBuffer;
                        c.Color = ColorChannel.Red;
                    });
            }
        }

        public async Task MapAsync(string textureTag, bool createEmpty, int? frame = null, int? part = null, CancellationToken token = default)
        {
            var builder = provider.GetRequiredService<ITextureBuilder>();

            builder.TargetFrame = frame;
            builder.TargetPart = part;
            builder.InputChannels = context.InputEncoding.ToArray();
            builder.OutputChannels = context.OutputEncoding
                .Where(e => TextureTags.Is(e.Texture, textureTag)).ToArray();

            await builder.MapAsync(createEmpty, token);
            builderMap[textureTag] = builder;
        }

        public async Task<Image<TPixel>> CreateImageAsync<TPixel>(string textureTag, bool createEmpty, CancellationToken token = default)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (!builderMap.TryGetValue(textureTag, out var builder))
                throw new ApplicationException($"No texture builder found for tag '{textureTag}'!");

            return await builder.BuildAsync<TPixel>(createEmpty, token);
        }

        public int GetMaxFrameCount()
        {
            if (builderMap.Count == 0) return 1;
            return builderMap.Values.Max(b => b.FrameCount);
        }

        private void UpsertInputChannel<T>(Action<T> channelAction)
            where T : ResourcePackChannelProperties, new()
        {
            var hasChannel = false;
            foreach (var channel in context.InputEncoding.OfType<T>()) {
                channel.Reset();
                channelAction(channel);
                hasChannel = true;
            }

            if (hasChannel) return;

            var newChannel = new T();
            channelAction(newChannel);
            context.InputEncoding.Add(newChannel);
        }
    }
}
