using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Preview.Textures;
using PixelGraph.UI.Models;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    internal interface IMaterialBuilder
    {
        Task UpdateAllTexturesAsync(CancellationToken token = default);
        Task UpdateTexturesByTagAsync(string textureTag, CancellationToken token = default);
        Task ClearAllTexturesAsync();
        void ClearAllTextures();
        Material BuildMaterial();
    }

    internal abstract class MaterialBuilderBase<T> : IMaterialBuilder, IDisposable, IAsyncDisposable
        where T : ITexturePreviewBuilder
    {
        private readonly IServiceProvider provider;

        protected Dictionary<string, Stream> TextureMap {get;}
        public MainModel Model {get; set;}


        protected MaterialBuilderBase(IServiceProvider provider)
        {
            this.provider = provider;

            TextureMap = new Dictionary<string, Stream>(StringComparer.InvariantCultureIgnoreCase);
        }

        public virtual void Dispose()
        {
            ClearAllTextures();
        }

        public virtual async ValueTask DisposeAsync()
        {
            await ClearAllTexturesAsync();
        }

        public virtual async Task UpdateAllTexturesAsync(CancellationToken token = default)
        {
            foreach (var tag in TextureMap.Keys) {
                if (TextureMap.TryGetValue(tag, out var existing) && existing != null)
                    await existing.DisposeAsync();

                using var previewBuilder = GetPreviewBuilder();
                TextureMap[tag] = await GetTextureStreamAsync(previewBuilder, tag, 0, 0, token);
            }
        }

        public virtual async Task UpdateTexturesByTagAsync(string textureTag, CancellationToken token = default)
        {
            if (!TextureMap.TryGetValue(textureTag, out var existing)) return;
            if (existing != null) await existing.DisposeAsync();

            using var previewBuilder = GetPreviewBuilder();
            TextureMap[textureTag] = await GetTextureStreamAsync(previewBuilder, textureTag, 0, 0, token);
        }

        public virtual void ClearAllTextures()
        {
            foreach (var stream in TextureMap.Values)
                stream?.Dispose();
        }

        public virtual async Task ClearAllTexturesAsync()
        {
            foreach (var stream in TextureMap.Values) {
                if (stream == null) continue;
                await stream.DisposeAsync();
            }
        }

        public abstract Material BuildMaterial();

        protected ITexturePreviewBuilder GetPreviewBuilder()
        {
            var previewBuilder = provider.GetRequiredService<T>();

            previewBuilder.Input = Model.PackInput;
            previewBuilder.Profile = Model.Profile.Loaded;
            previewBuilder.Material = Model.Material.Loaded;
            
            return previewBuilder;
        }

        protected static async Task<Stream> GetTextureStreamAsync(ITexturePreviewBuilder previewBuilder, string textureTag, int? frame = null, int? part = null, CancellationToken token = default)
        {
            var stream = new MemoryStream();

            try {
                using var image = await previewBuilder.BuildAsync(textureTag, frame, part);
                await image.SaveAsPngAsync(stream, token);
                await stream.FlushAsync(token);
                stream.Position = 0;
                return stream;
            }
            catch {
                await stream.DisposeAsync();
                throw;
            }
        }
    }
}
