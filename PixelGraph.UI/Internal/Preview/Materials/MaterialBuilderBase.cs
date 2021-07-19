using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal.Preview.CubeMaps;
using PixelGraph.UI.Internal.Preview.Textures;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    internal interface IMaterialBuilder : IDisposable, IAsyncDisposable
    {
        string PassName {get; set;}
        string PassNameOIT {get; set;}
        SamplerStateDescription ColorSampler {get; set;}
        SamplerStateDescription HeightSampler {get; set;}

        ResourcePackInputProperties PackInput {get; set;}
        ResourcePackProfileProperties PackProfile {get; set;}
        MaterialProperties Material {get; set;}

        Task UpdateAllTexturesAsync(CancellationToken token = default);
        Task UpdateTexturesByTagAsync(string textureTag, CancellationToken token = default);
        Task ClearAllTexturesAsync();
        void ClearAllTextures();
        Material BuildMaterial();
    }

    internal abstract class MaterialBuilderBase<T> : IMaterialBuilder
        where T : ITexturePreviewBuilder
    {
        private readonly IServiceProvider provider;
        private readonly BmpEncoder encoder;

        protected Dictionary<string, Stream> TextureMap {get;}

        public string PassName {get; set;}
        public string PassNameOIT {get; set;}
        public SamplerStateDescription ColorSampler {get; set;}
        public SamplerStateDescription HeightSampler {get; set;}

        public ResourcePackInputProperties PackInput {get; set;}
        public ResourcePackProfileProperties PackProfile {get; set;}
        public MaterialProperties Material {get; set;}
        public ICubeMapSource EnvironmentCubeMapSource {get; set;}
        public ICubeMapSource IrradianceCubeMapSource {get; set;}
        public TextureModel BrdfLutMap {get; set;}
        public bool RenderEnvironmentMap {get; set;}


        protected MaterialBuilderBase(IServiceProvider provider)
        {
            this.provider = provider;

            encoder = new BmpEncoder {
                BitsPerPixel = BmpBitsPerPixel.Pixel32,
                SupportTransparency = true,
            };

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

        public abstract Material BuildMaterial();

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

        protected ITexturePreviewBuilder GetPreviewBuilder()
        {
            var previewBuilder = provider.GetRequiredService<T>();

            previewBuilder.Input = PackInput;
            previewBuilder.Profile = PackProfile;
            previewBuilder.Material = Material;
            
            return previewBuilder;
        }

        protected async Task<Stream> GetTextureStreamAsync(ITexturePreviewBuilder previewBuilder, string textureTag, int? frame = null, int? part = null, CancellationToken token = default)
        {
            var stream = new MemoryStream();

            try {
                using var image = await previewBuilder.BuildAsync<Rgba32>(textureTag, frame, part);

                await image.SaveAsBmpAsync(stream, encoder, token);
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
