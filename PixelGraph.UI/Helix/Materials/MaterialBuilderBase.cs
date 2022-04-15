using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.LUTs;
using PixelGraph.UI.Internal.Preview.Textures;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Helix.Materials
{
    internal interface IMaterialBuilder : IDisposable, IAsyncDisposable
    {
        string PassName {get; set;}
        string PassNameOIT {get; set;}
        SamplerStateDescription ColorSampler {get; set;}
        SamplerStateDescription HeightSampler {get; set;}

        ResourcePackContext PackContext {get; set;}
        MaterialProperties Material {get; set;}
        ILutMapSource DielectricBrdfLutMapSource {get; set;}
        ICubeMapSource EnvironmentCubeMapSource {get; set;}
        ICubeMapSource IrradianceCubeMapSource {get; set;}
        //TextureModel BrdfLutMap {get; set;}
        bool RenderEnvironmentMap {get; set;}

        Task UpdateAllTexturesAsync(int part = 0, CancellationToken token = default);
        Task UpdateTexturesByTagAsync(string textureTag, int part = 0, CancellationToken token = default);
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

        public ResourcePackContext PackContext {get; set;}
        public MaterialProperties Material {get; set;}
        public ILutMapSource DielectricBrdfLutMapSource {get; set;}
        public ICubeMapSource EnvironmentCubeMapSource {get; set;}
        public ICubeMapSource IrradianceCubeMapSource {get; set;}
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

        public virtual async Task UpdateAllTexturesAsync(int part = 0, CancellationToken token = default)
        {
            var allKeys = TextureMap.Keys.ToArray();
            foreach (var tag in allKeys) {
                if (TextureMap.TryGetValue(tag, out var existing) && existing != null)
                    await existing.DisposeAsync();

                using var previewBuilder = GetPreviewBuilder(0, part);
                TextureMap[tag] = await GetTextureStreamAsync(previewBuilder, tag, token);
            }
        }

        public virtual async Task UpdateTexturesByTagAsync(string textureTag, int part = 0, CancellationToken token = default)
        {
            if (!TextureMap.TryGetValue(textureTag, out var existing)) return;
            if (existing != null) await existing.DisposeAsync();

            using var previewBuilder = GetPreviewBuilder(0, part);
            TextureMap[textureTag] = await GetTextureStreamAsync(previewBuilder, textureTag, token);
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

        protected ITexturePreviewBuilder GetPreviewBuilder(int? frame = null, int? part = null)
        {
            var previewBuilder = provider.GetRequiredService<T>();

            previewBuilder.Input = PackContext.Input;
            previewBuilder.Profile = PackContext.Profile;
            previewBuilder.Material = Material;

            previewBuilder.TargetFrame = frame;
            previewBuilder.TargetPart = part;
            
            return previewBuilder;
        }

        protected async Task<Stream> GetTextureStreamAsync(ITexturePreviewBuilder previewBuilder, string textureTag, CancellationToken token = default)
        {
            var stream = new MemoryStream();

            try {
                using var image = await previewBuilder.BuildAsync(textureTag, token);

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
