using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Internal
{
    internal interface ITexturePreviewBuilder : IDisposable
    {
        ResourcePackInputProperties Input {get; set;}
        MaterialProperties Material {get; set;}
        CancellationToken Token {get;}

        Task<ImageSource> BuildAsync(string tag);
        void Cancel();
    }

    internal class TexturePreviewBuilder : ITexturePreviewBuilder
    {
        private readonly IServiceProvider provider;
        private readonly CancellationTokenSource tokenSource;

        public ResourcePackInputProperties Input {get; set;}
        public MaterialProperties Material {get; set;}

        public CancellationToken Token => tokenSource.Token;


        public TexturePreviewBuilder(IServiceProvider provider)
        {
            this.provider = provider;
            tokenSource = new CancellationTokenSource();
        }

        public async Task<ImageSource> BuildAsync(string tag)
        {
            using var graph = provider.GetRequiredService<ITextureGraph>();

            graph.Context = new MaterialContext {
                Input = Input,
                Material = Material,
                //Profile = ,
            };

            graph.CreateEmpty = true;

            //builder.DefaultSize = GetSourceSize(); TODO

            // ERROR: Layer pack format
            var inputFormat = TextureEncoding.GetFactory(Input?.Format);
            var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
            inputEncoding.Merge(Input);
            inputEncoding.Merge(Material);
            graph.InputEncoding = inputEncoding.GetMapped().ToList();

            if (TextureTags.Is(tag, TextureTags.Alpha))
                graph.OutputEncoding.AddRange(GetAlphaChannels());

            if (TextureTags.Is(tag, TextureTags.Diffuse))
                graph.OutputEncoding.AddRange(GetDiffuseChannels());

            if (TextureTags.Is(tag, TextureTags.Albedo))
                graph.OutputEncoding.AddRange(GetAlbedoChannels());

            if (TextureTags.Is(tag, TextureTags.Height))
                graph.OutputEncoding.AddRange(GetHeightChannels());

            if (TextureTags.Is(tag, TextureTags.Occlusion))
                graph.OutputEncoding.AddRange(GetOcclusionChannels());

            if (TextureTags.Is(tag, TextureTags.Normal)) {
                graph.OutputEncoding.AddRange(GetNormalChannels());
                await graph.BuildNormalTextureAsync(tokenSource.Token);
            }

            if (TextureTags.Is(tag, TextureTags.Specular))
                graph.OutputEncoding.AddRange(GetSpecularChannels());

            if (TextureTags.Is(tag, TextureTags.Smooth))
                graph.OutputEncoding.AddRange(GetSmoothChannels());

            if (TextureTags.Is(tag, TextureTags.Rough))
                graph.OutputEncoding.AddRange(GetRoughChannels());

            if (TextureTags.Is(tag, TextureTags.Metal))
                graph.OutputEncoding.AddRange(GetMetalChannels());

            if (TextureTags.Is(tag, TextureTags.Porosity))
                graph.OutputEncoding.AddRange(GetPorosityChannels());

            if (TextureTags.Is(tag, TextureTags.SubSurfaceScattering))
                graph.OutputEncoding.AddRange(GetSSSChannels());

            if (TextureTags.Is(tag, TextureTags.Emissive))
                graph.OutputEncoding.AddRange(GetEmissiveChannels());

            using var image = await graph.GetPreviewAsync(tag, tokenSource.Token);
            return await CreateImageSourceAsync(image, tokenSource.Token);
        }

        public void Cancel()
        {
            tokenSource.Cancel();
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        private static async Task<ImageSource> CreateImageSourceAsync(Image image, CancellationToken token)
        {
            await using var stream = new MemoryStream();
            await image.SaveAsync(stream, PngFormat.Instance, token);
            await stream.FlushAsync(token);
            stream.Seek(0, SeekOrigin.Begin);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.CacheOption = BitmapCacheOption.OnLoad;
            imageSource.StreamSource = stream;
            imageSource.EndInit();
            imageSource.Freeze();

            return imageSource;
        }

        private static IEnumerable<ResourcePackChannelProperties> GetAlphaChannels()
        {
            yield return new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Red);
            yield return new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Green);
            yield return new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetDiffuseChannels()
        {
            yield return new ResourcePackDiffuseRedChannelProperties(TextureTags.Diffuse, ColorChannel.Red);
            yield return new ResourcePackDiffuseGreenChannelProperties(TextureTags.Diffuse, ColorChannel.Green);
            yield return new ResourcePackDiffuseBlueChannelProperties(TextureTags.Diffuse, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetAlbedoChannels()
        {
            yield return new ResourcePackAlbedoRedChannelProperties(TextureTags.Albedo, ColorChannel.Red);
            yield return new ResourcePackAlbedoGreenChannelProperties(TextureTags.Albedo, ColorChannel.Green);
            yield return new ResourcePackAlbedoBlueChannelProperties(TextureTags.Albedo, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetHeightChannels()
        {
            yield return new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Red); //{Invert = true};
            yield return new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Green); //{Invert = true};
            yield return new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Blue); //{Invert = true};
        }

        private static IEnumerable<ResourcePackChannelProperties> GetOcclusionChannels()
        {
            yield return new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Red); //{Invert = true};
            yield return new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Green); //{Invert = true};
            yield return new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Blue); //{Invert = true};
        }

        private static IEnumerable<ResourcePackChannelProperties> GetNormalChannels()
        {
            yield return new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red);
            yield return new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green);
            yield return new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetSpecularChannels()
        {
            yield return new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Red);
            yield return new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Green);
            yield return new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetSmoothChannels()
        {
            yield return new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Red);
            yield return new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Green);
            yield return new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetRoughChannels()
        {
            yield return new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Red);
            yield return new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Green);
            yield return new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetMetalChannels()
        {
            yield return new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Red);
            yield return new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Green);
            yield return new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetPorosityChannels()
        {
            yield return new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Red);
            yield return new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Green);
            yield return new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetSSSChannels()
        {
            yield return new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Red);
            yield return new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Green);
            yield return new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Blue);
        }

        private static IEnumerable<ResourcePackChannelProperties> GetEmissiveChannels()
        {
            yield return new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Red);
            yield return new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Green);
            yield return new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Blue);
        }
    }
}
