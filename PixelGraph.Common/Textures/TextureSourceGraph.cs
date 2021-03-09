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
        //Task AddAsync(string tag, string localFile, CancellationToken token = default);
        Task<TextureSource> GetOrCreateAsync(string localFile, CancellationToken token = default);
    }

    internal class TextureSourceGraph : ITextureSourceGraph
    {
        private readonly IInputReader fileReader;
        //private readonly INamingStructure naming;
        private readonly ITextureGraphContext context;
        private readonly Dictionary<string, TextureSource> sourceMap;


        public TextureSourceGraph(
            IInputReader fileReader,
            //INamingStructure naming,
            ITextureGraphContext context)
        {
            this.fileReader = fileReader;
            //this.naming = naming;
            this.context = context;

            sourceMap = new Dictionary<string, TextureSource>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(string tag, out TextureSource source)
        {
            return sourceMap.TryGetValue(tag, out source);
        }

        public async Task<TextureSource> GetOrCreateAsync(string localFile, CancellationToken token = default)
        {
            if (sourceMap.TryGetValue(localFile, out var source)) return source;

            await using var stream = fileReader.Open(localFile);
            var info = await Image.IdentifyAsync(Configuration.Default, stream, token);

            source = new TextureSource {
                LocalFile = localFile,
                FrameCount = 1,
            };

            (source.Width, source.Height) = info.Size();

            if (context.IsAnimated) {
                source.FrameCount = source.Height / source.Width;
                source.Height = source.Width;
            }

            return sourceMap[localFile] = source;
        }

        //private async Task<int> GetFrameCountAsync(string tag, CancellationToken token)
        //{
        //    var metaFile = naming.GetInputMetaName(context.Material, tag);
        //    if (!fileReader.FileExists(metaFile)) return 1;

        //    var root = await LoadJsonAsync(metaFile, token);
        //    var framesNode = root.SelectToken("animation/frames")?.Values<int>();
        //    if (framesNode == null) return 1;

        //    return framesNode.Max() + 1;
        //}

        //private async Task<JToken> LoadJsonAsync(string localFile, CancellationToken token)
        //{
        //    await using var stream = fileReader.Open(localFile);
        //    using var reader = new StreamReader(stream);
        //    using var jsonReader = new JsonTextReader(reader);
        //    return await JToken.ReadFromAsync(jsonReader, token);
        //}
    }

    public class TextureSource
    {
        public string LocalFile {get; set;}
        public int FrameCount {get; set;}
        public int Width {get; set;}
        public int Height {get; set;}
    }
}
