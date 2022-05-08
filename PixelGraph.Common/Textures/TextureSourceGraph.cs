using PixelGraph.Common.IO;
using PixelGraph.Common.Textures.Graphing;
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
        private readonly Lazy<float?> expectedAspect;


        public TextureSourceGraph(
            IInputReader fileReader,
            ITextureGraphContext context)
        {
            this.fileReader = fileReader;
            this.context = context;

            sourceMap = new Dictionary<string, TextureSource>(StringComparer.OrdinalIgnoreCase);
            expectedAspect = new Lazy<float?>(context.GetExpectedAspect);
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
                Gamma = GetImageGamma(info),
            };

            if (context.IsAnimated) {
                var frameHeight = source.Width;
                //var expectedAspect = context.GetExpectedAspect();

                if (expectedAspect.Value.HasValue)
                    frameHeight = (int)(source.Width / expectedAspect.Value.Value + 0.5f);

                source.FrameCount = source.Height / frameHeight;
                source.Height = frameHeight;
            }

            return sourceMap[localFile] = source;
        }

        private float GetImageGamma(IImageInfo info)
        {
            var pngMeta = info.Metadata.GetPngMetadata();
            if (pngMeta is {Gamma: > float.Epsilon}) return pngMeta.Gamma;

            return 1.0f;
        }
    }

    public class TextureSource
    {
        public string LocalFile {get; set;}
        public int FrameCount {get; set;}
        public int Width {get; set;}
        public int Height {get; set;}
        public float Gamma {get; set;}
    }
}
