using PixelGraph.Common.IO;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureSourceGraph
    {
        bool TryGet(string tag, out TextureSource source);
        Task<TextureSource> GetOrCreateAsync(string localFile, CancellationToken token = default);
    }

    internal class TextureSourceGraph : ITextureSourceGraph
    {
        private readonly IInputReader fileReader;
        private readonly ITextureGraphContext context;
        private readonly Dictionary<string, TextureSource> sourceMap;


        public TextureSourceGraph(
            IInputReader fileReader,
            ITextureGraphContext context)
        {
            this.fileReader = fileReader;
            this.context = context;

            sourceMap = new Dictionary<string, TextureSource>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(string tag, out TextureSource source)
        {
            return sourceMap.TryGetValue(tag, out source);
        }

        public async Task<TextureSource> GetOrCreateAsync(string localFile, CancellationToken token = default)
        {
            if (localFile == null) throw new ArgumentNullException(nameof(localFile));

            if (sourceMap.TryGetValue(localFile, out var source)) return source;

            await using var stream = fileReader.Open(localFile);
            if (stream == null) throw new ApplicationException($"Failed to open image '{localFile}'!");

            var info = await Image.IdentifyAsync(Configuration.Default, stream, token);
            if (info == null) throw new ApplicationException($"Unable to locate decoder for image '{localFile}'!");

            source = new TextureSource {
                LocalFile = localFile,
                FrameCount = 1,
                Width = info.Width,
                Height = info.Height,
            };

            if (context.IsAnimated) {
                source.FrameCount = source.Height / source.Width;
                source.Height = source.Width;
            }

            return sourceMap[localFile] = source;
        }
    }

    public class TextureSource
    {
        public string LocalFile {get; set;}
        public int FrameCount {get; set;}
        public int Width {get; set;}
        public int Height {get; set;}
    }
}
